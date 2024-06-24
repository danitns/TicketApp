using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Stripe.TestHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Subscriptions;
using TicketApp.BusinessLogic.Implementation.Transactions;
using TicketApp.Common.DTOs;
using TicketApp.Common.Exceptions;
using TicketApp.Common.Extensions;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Account
{
    public class UserAccountService : BaseService
    {
        private readonly RegisterUserValidator RegisterUserValidator;
        private readonly EditAccountValidator EditAccountValidator;
        private readonly TransactionService TransactionService;
        private readonly IConfiguration Configuration;

        public UserAccountService(ServiceDependencies serviceDependencies, TransactionService transactionService, IConfiguration configuration) : base(serviceDependencies)
        {
            RegisterUserValidator = new RegisterUserValidator(UnitOfWork);
            EditAccountValidator = new EditAccountValidator(UnitOfWork);
            TransactionService = transactionService;
            Configuration = configuration;
        }

        public async Task<CurrentUserDto> Login(string email, string password)
        {

            var user = await UnitOfWork.Users.Get()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new CurrentUserDto { IsAuthenticated = false };
            }

            var hashedPassword = PasswordHash(password, user.Id);

            var subscription = await TransactionService.GetUserSubscriptionByEmail(user.Email);

            if (hashedPassword == user.Password)
            {
                var currentUser = new CurrentUserDto()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = $"{user.FirstName} {user.LastName}",
                    IsAuthenticated = true,
                    Role = Enum.GetName(typeof(UserRoles), user.RoleId),
                    IsDisabled = user.DeletedAt != null
                };

                if (subscription != null && subscription.Status == SubscriptionStatuses.Active)
                    currentUser.Subscription = subscription.Name;
                else currentUser.Subscription = SubscriptionTypes.None.ToString();

                return currentUser;
            }

            return new CurrentUserDto { IsAuthenticated = false };
        }

        public async Task<CurrentUserDto> RegisterNewUser(RegisterModel model)
        {
            ExecuteInTransaction(uow =>
            {
                RegisterUserValidator.Validate(model).ThenThrow(model);

                var user = Mapper.Map<RegisterModel, User>(model);

                if (model.Picture != null)
                {
                    var picture = new Picture();
                    picture.Id = Guid.NewGuid();
                    using (var ms = new MemoryStream())
                    {
                        model.Picture.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        picture.PictureContent = fileBytes;
                    }

                    user.Picture = picture;
                }

                var hashedPassword = PasswordHash(user.Password, user.Id);

                user.Password = hashedPassword;

                uow.Users.Insert(user);

                uow.SaveChanges();

            });

            var customerService = new Stripe.CustomerService();


            var options = new CustomerCreateOptions
            {
                Email = model.Email,
                Name = model.FirstName + ' ' + model.LastName,
            };

            customerService.Create(options);


            return await Login(model.Email, model.Password);
        }

        public async Task<UserDetailsModel> AccountDetails()
        {
            if (CurrentUser.IsAuthenticated)
            {
                var user = await UnitOfWork.Users.Get()
                    .Include(u => u.Transactions)
                        .ThenInclude(t => t.TicketTransactions)
                        .ThenInclude(t => t.Ticket)
                    .Include(u => u.Transactions)
                        .ThenInclude(t => t.Subscription)
                    .Include(p => p.Picture)
                    .SingleOrDefaultAsync(u => u.Id == CurrentUser.Id);

                if (user == null)
                    throw new NotFoundErrorException();

                var displayTransactionModels = new List<DisplayTransactionModel>();

                var userTransactions = user.Transactions.Where(t => t.ProcessingDate != null).ToList();

                foreach (var transaction in userTransactions)
                {
                    var transactionModel = Mapper.Map<DisplayTransactionModel>(transaction);

                    transactionModel.Tickets = transaction
                        .TicketTransactions
                        .GroupBy(tt => tt.Ticket.Name)
                        .Select(group => new Tuple<string, int>(group.Key, group.Count()))
                        .ToList();

                    displayTransactionModels.Add(transactionModel);
                }

                var userDetails = Mapper.Map<User, UserDetailsModel>(user);

                userDetails.Transactions = displayTransactionModels;

                return userDetails;
            }
            return null;
        }

        private string PasswordHash(string password, Guid userId)
        {
            var salt = userId.ToString();
            SHA256 sha256Hash = SHA256.Create();
            byte[] passwordHash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
            var hashedPasswordToString = System.Text.Encoding.UTF8.GetString(passwordHash);
            return hashedPasswordToString;
        }

        public async Task BecomeOrganizer()
        {
            var organizer = await UnitOfWork.Users
                .Get()
                .SingleOrDefaultAsync(u => u.Id == CurrentUser.Id);
            if (organizer == null)
                throw new NotFoundErrorException();
            if (organizer.RoleId == (int)RoleTypes.User)
            {
                organizer.RoleId = (int)RoleTypes.PendingOrganizer;
                UnitOfWork.Users.Update(organizer);
                UnitOfWork.SaveChanges();
            }
        }

        public async Task DeleteAccount(Guid? userId)
        {
            var userIdToSearch = CurrentUser.Id;
            if (userId != null)
                userIdToSearch = userId.Value;
            var user = await UnitOfWork.Users
                .Get()
                .SingleOrDefaultAsync(u => u.Id == userIdToSearch);
            if (user == null)
                throw new NotFoundErrorException();

            var jobName = Configuration["BackgroundJobName"] + user.Email;
            RecurringJob.RemoveIfExists(jobName);

            user.DeletedAt = DateTime.Now;
            user.IsEdited = true;
            UnitOfWork.Users.Update(user);
            UnitOfWork.SaveChanges();
        }

        public async Task<EditUserModel> GetUserForEdit(Guid? id)
        {
            var userIdForSearch = CurrentUser.Id;
            if (id != null)
            {
                if (CurrentUser.Role != "Admin" && id != CurrentUser.Id)
                    throw new AccessViolationException();
                userIdForSearch = id.Value;
            }


            var user = await UnitOfWork.Users
                .Get()
                .Include(u => u.Picture)
                .SingleOrDefaultAsync(u => u.Id == userIdForSearch);

            var userToEdit = Mapper.Map<EditUserModel>(user);
            return userToEdit;
        }

        public async Task EditUser(EditUserModel model)
        {
            var validationResult = EditAccountValidator.Validate(model);

            var oldPicture = await UnitOfWork.Pictures
                .Get()
                .SingleOrDefaultAsync(p => p.Id == model.PictureId);

            if (!validationResult.IsValid)
            {
                model.Picture = oldPicture.PictureContent;
                validationResult.ThenThrow(model);
            }

            var editedUser = await UnitOfWork.Users
                .Get()
                .Include(u => u.Picture)
                .SingleOrDefaultAsync(u => u.Email == model.Email);

            if (editedUser == null)
                throw new NotFoundErrorException();


            Mapper.Map<EditUserModel, User>(model, editedUser);
            if (model.NewPicture != null)
            {
                using (var ms = new MemoryStream())
                {
                    model.NewPicture.CopyTo(ms);
                    var fileBytes = ms.ToArray();

                    if (oldPicture == null)
                    {
                        oldPicture = new Picture()
                        {
                            Id = Guid.NewGuid()
                        };

                    }
                    oldPicture.PictureContent = fileBytes;
                    editedUser.Picture = oldPicture;

                }
            }
            editedUser.IsEdited = true;

            UnitOfWork.Users.Update(editedUser);
            UnitOfWork.SaveChanges();

        }

        public bool UserNeedsUpdate(CurrentUserDto currentUser)
        {
            var user = UnitOfWork.Users.Get()
                .SingleOrDefault(u => u.Id == currentUser.Id);

            if (user == null)
                return false;

            return user.IsEdited;
        }

        public async Task<CurrentUserDto> UpdateUserIsEdited(Guid id)
        {

            var currentUser = new CurrentUserDto();

            var user = UnitOfWork.Users
                .Get()
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == id);

            var subscription = await TransactionService.GetUserSubscriptionByEmail(user.Email);

            ExecuteInTransaction(uow =>
            {
                user.IsEdited = false;

                currentUser = new CurrentUserDto()
                {
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    Id = user.Id,
                    IsAuthenticated = true,
                    Role = user.Role.Name,
                    IsDisabled = user.DeletedAt != null
                };

                if (subscription != null && subscription.Status == SubscriptionStatuses.Active)
                    currentUser.Subscription = subscription.Name;
                else currentUser.Subscription = SubscriptionTypes.None.ToString();

                uow.Users.Update(user);
                uow.SaveChanges();
            });
            return currentUser;
        }
    }
}
