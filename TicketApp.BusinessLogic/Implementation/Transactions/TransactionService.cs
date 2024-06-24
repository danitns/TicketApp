using Azure;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Subscriptions;
using TicketApp.BusinessLogic.Implementation.Tickets;
using TicketApp.BusinessLogic.Implementation.Tickets.Models;
using TicketApp.Common.DTOs;
using TicketApp.Common.Exceptions;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Transactions
{
    public class TransactionService : BaseService
    {
        private readonly Stripe.SubscriptionService SubscriptionService;

        private readonly IConfiguration Configuration;

        static Random Random;
        public TransactionService(ServiceDependencies serviceDependencies, IConfiguration configuration) : base(serviceDependencies)
        {
            SubscriptionService = new Stripe.SubscriptionService();
            if (Random == null)
                Random = new Random();
            Configuration = configuration;
        }

        private async Task CreatePromoCode(string customerId)
        {
            var promoCodeService = new PromotionCodeService();
            var promoCode = new PromotionCodeCreateOptions
            {
                Customer = customerId,
                Coupon = await GetFreeCoupon()
            };
            promoCodeService.Create(promoCode);
        }

        private async Task<string> GetFreeCoupon()
        {
            var couponService = new CouponService();
            var coupons = await couponService.ListAsync();
            var freeCoupon = coupons
                .Where(c => c.Name == Configuration["FreeCoupon"])
                .SingleOrDefault();
            string couponId;
            if (freeCoupon != null)
            {
                couponId = freeCoupon.Id;
            }
            else
            {
                couponId = CreateCoupon(100, Configuration["FreeCoupon"]);
            }
            return couponId;
        }

        private async Task<Transaction> GetUserTransaction(string userEmail, bool isFree = false, bool isSubscription = false)
        {
            var userCart = await UnitOfWork.Transactions
                .Get()
                .Include(t => t.TicketTransactions)
                .Include(t => t.User)
                .Include(t => t.TicketTransactions)
                    .ThenInclude(tt => tt.Ticket)
                        .ThenInclude(t => t.Event)
                .SingleOrDefaultAsync(t =>
                    t.User.Email == userEmail
                    && t.ProcessingDate == null
                    && t.IsFree == isFree
                    && (isSubscription ? (t.SubscriptionId != null) : (t.SubscriptionId == null)));
            return userCart;
        }

        private async Task<Transaction> CreateTransaction(bool isFree = false, Guid? subscriptionId = null)
        {
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                IsFree = isFree,
                SubscriptionId = subscriptionId
            };
            UnitOfWork.Transactions.Insert(transaction);
            await UnitOfWork.SaveChangesAsync();
            return transaction;
        }

        public async Task<ShoppingCartModel> DisplayShoppingCart()
        {
            var userTransaction = await GetUserTransaction(CurrentUser.Email);

            if (userTransaction == null)
            {
                userTransaction = await CreateTransaction();
            }

            var displayUserCart = Mapper.Map<ShoppingCartModel>(userTransaction);

            var userSubscription = await GetUserSubscriptionByEmail(CurrentUser.Email);
            if (userSubscription != null && userSubscription.Status == SubscriptionStatuses.Active)
            {
                displayUserCart.Discount = userSubscription.Discount;
            }

            return displayUserCart;
        }

        public async Task AddTicketsToCart(List<ShoppingCartTicket> ticketsAndQuantity)
        {
            var userTransaction = await GetUserTransaction(CurrentUser.Email);

            if (userTransaction == null)
                userTransaction = await CreateTransaction();
            foreach (var tq in ticketsAndQuantity)
            {
                var ticket = await GetTicketByName(tq.Name, tq.EventName);

                if (ticket == null)
                    throw new NotFoundErrorException();

                for (int index = 0; index < tq.Quantity; index++)
                {
                    var ticketTransaction = new TicketTransaction()
                    {
                        Id = Guid.NewGuid(),
                        TicketId = ticket.Id,
                        TransactionId = userTransaction.Id
                    };
                    userTransaction.TicketTransactions.Add(ticketTransaction);
                }
            }
            await UnitOfWork.SaveChangesAsync();
        }

        private async Task<int> NOTicketsLeft(Ticket ticket)
        {
            var maxValue = await UnitOfWork.TicketTransactions
                .Get()
                .Include(tt => tt.Transaction)
                .Where(t => t.TicketId == ticket.Id && t.Transaction.ProcessingDate != null)
                .CountAsync();
            return ticket.Notickets - maxValue;
        }

        public async Task<Dictionary<string, int>> CheckStockForCheckout()
        {
            var shoppingCart = await DisplayShoppingCart();
            var errors = await CheckStockForTickets(shoppingCart.AddedTickets);
            return errors;
        }

        public async Task<Dictionary<string, int>> CheckStockForTickets(List<ShoppingCartTicket> ticketsAndQuantity)
        {
            var errors = new Dictionary<string, int>();
            foreach (var t in ticketsAndQuantity)
            {
                var ticket = await GetTicketByName(t.Name, t.EventName);
                if (t.Quantity > 10)
                    errors.Add(ticket.Name, 10);
                else
                {
                    var maxValue = await NOTicketsLeft(ticket);
                    if (maxValue < t.Quantity)
                    {
                        errors.Add(ticket.Name, maxValue);
                    }
                }

            }

            if (errors.Count != 0)
                return errors;

            return null;
        }

        private async Task<Ticket> GetTicketByName(string name, string eventName)
        {
            var ticket = await UnitOfWork.Tickets
                    .Get()
                    .Include(t => t.Event)
                    .SingleOrDefaultAsync(t => t.Name == name && t.Event.Name == eventName);
            if (ticket == null)
                throw new NotFoundErrorException();
            return ticket;
        }

        public async Task<Dictionary<string, int>> UpdateShoppingCart(List<ShoppingCartTicket> ticketsAndQuantity)
        {

            var errors = await CheckStockForTickets(ticketsAndQuantity);
            if (errors != null)
            {
                return errors;
            }

            var userTransaction = await GetUserTransaction(CurrentUser.Email);
            foreach (var ticket in ticketsAndQuantity)
            {
                var ticketTransactions = userTransaction.TicketTransactions
                    .Where(t => t.Ticket.Name == ticket.Name && t.Ticket.Event.Name == ticket.EventName)
                    .ToList();

                var countTicketTransactions = ticketTransactions.Count();
                if (ticket.Quantity == 0)
                    UnitOfWork.TicketTransactions.DeleteRange(ticketTransactions);

                else if (ticket.Quantity > countTicketTransactions)
                {
                    var productsToAdd = ticket.Quantity - countTicketTransactions;

                    var ticketFromDb = await GetTicketByName(ticket.Name, ticket.EventName);

                    for (int index = 0; index < productsToAdd; index++)
                    {
                        var ticketTransaction = new TicketTransaction()
                        {
                            Id = Guid.NewGuid(),
                            TicketId = ticketFromDb.Id,
                            TransactionId = userTransaction.Id
                        };
                        userTransaction.TicketTransactions.Add(ticketTransaction);
                    }
                }
                else if (ticket.Quantity < countTicketTransactions)
                {
                    UnitOfWork.TicketTransactions.DeleteRange(ticketTransactions.Take(countTicketTransactions - ticket.Quantity));
                }
            }

            UnitOfWork.SaveChanges();

            return null;
        }

        public async Task<Session> BuySubscription(string subscriptionIdInput)
        {
            var result = Guid.TryParse(subscriptionIdInput, out var subscriptionId);
            if (result == false)
                throw new NotFoundErrorException();

            var userTransaction = await GetUserTransaction(CurrentUser.Email, false, true);
            if (userTransaction == null)
                userTransaction = await CreateTransaction(false, subscriptionId);

            var price = await GetStripePrice(subscriptionId, true);

            var productList = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions()
                {
                    Price = price.Id,
                    Quantity = 1
                }
            };

            var domain = Configuration["Domain"];

            var customer = await GetStripeCustomerByEmail(CurrentUser.Email);

            var options = new SessionCreateOptions
            {
                Customer = customer.Id,
                Mode = "subscription",
                SuccessUrl = $"{domain}/Subscription/Index",
                CancelUrl = $"{domain}/Subscription/Index",
            };

            var session = CreateCheckout(productList, options);

            return session;
        }

        public async Task<string> BuyTickests()
        {
            var shoppingCart = await DisplayShoppingCart();

            var ticketList = await CreateItemListForTickets(shoppingCart.AddedTickets);

            var couponId = await GetCouponId();

            var domain = Configuration["Domain"];
            var customer = await GetStripeCustomerByEmail(CurrentUser.Email);
            var options = new SessionCreateOptions
            {
                Customer = customer.Id,
                Mode = "payment",
                SuccessUrl = $"{domain}/Ticket/MyTickets",
                CancelUrl = $"{domain}/Transaction/ShoppingCart",
            };

            if (couponId != null)
            {
                options.Discounts = new List<SessionDiscountOptions>
                {
                    new SessionDiscountOptions
                    {
                        Coupon = couponId,
                    }
                };
            }

            var session = CreateCheckout(ticketList, options);

            return session.Url;
        }

        private async Task<string> GetCouponId()
        {
            var userSubscription = await GetUserSubscriptionByEmail(CurrentUser.Email);
            if (userSubscription != null && userSubscription.Status == SubscriptionStatuses.Active)
            {
                var couponService = new CouponService();
                var coupon = couponService.List().Where(c => c.Name == userSubscription.Name).SingleOrDefault();
                return coupon.Id;
            }
            return null;

        }

        public Session CreateCheckout(List<SessionLineItemOptions> itemList, SessionCreateOptions createOptions)
        {
            var options = createOptions;
            options.LineItems = itemList;
            var service = new SessionService();
            Session session = service.Create(options);

            return session;
        }



        public async Task<Customer> GetStripeCustomerByEmail(string userEmail)
        {
            var customerService = new CustomerService();

            var listOptions = new CustomerListOptions
            {
                Email = userEmail
            };

            var customerSearch = await customerService.ListAsync(listOptions);

            var customer = customerSearch.FirstOrDefault();
            return customer;
        }

        public async Task<Customer> GetStripeCustomerById(string customerId)
        {
            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(customerId);
            return customer;
        }

        private async Task<List<SessionLineItemOptions>> CreateItemListForTickets(List<ShoppingCartTicket> ticketsWithQuantity)
        {
            var productList = new List<SessionLineItemOptions>();
            foreach (var product in ticketsWithQuantity)
            {
                var ticket = await GetTicketByName(product.Name, product.EventName);

                var price = await GetStripePrice(ticket.Id, false);

                productList.Add(new SessionLineItemOptions()
                {
                    Price = price.Id,
                    Quantity = product.Quantity
                });
            }
            return productList;
        }

        private async Task<Price> GetStripePrice(Guid ProductId, bool isSubscription)
        {
            var priceService = new PriceService();

            var product = await GetStripeProduct(ProductId, isSubscription);

            if (product == null)
                throw new NotFoundErrorException();

            var listOptions = new PriceListOptions
            {
                Product = product.Id
            };

            var prices = await priceService.ListAsync(listOptions);
            var price = prices.FirstOrDefault();

            return price;

        }

        private string CreateCoupon(int discount, string name)
        {
            var couponService = new CouponService();
            var options = new CouponCreateOptions
            {
                PercentOff = discount,
                Name = name,
                Currency = CurrencyTypes.ron.ToString()
            };
            var coupon = couponService.Create(options);
            return coupon.Id;
        }

        private async Task<Product> GetStripeProduct(Guid Id, bool isSubscription)
        {
            var productService = new ProductService();
            var product = await productService.GetAsync(Id.ToString());
            return product;
        }

        public async Task EndTransaction(Charge charge)
        {
            var customer = await GetStripeCustomerById(charge.CustomerId);

            var shoppingCart = await GetUserTransaction(customer.Email);
            if (!shoppingCart.TicketTransactions.IsNullOrEmpty())
            {
                shoppingCart.ProcessingDate = DateTime.Now;
                UnitOfWork.Transactions.Update(shoppingCart);
                UnitOfWork.SaveChanges();
            }
            UpdateFavouriteGenre(customer.Email);
        }

        public async Task EndSubscriptionTransaction(Charge charge)
        {
            var user = await GetUserByCustomerId(charge.CustomerId);
            var subscriptionTransaction = await UnitOfWork.Transactions
                .Get()
                .SingleOrDefaultAsync(t => t.UserId == user.Id && t.ProcessingDate == null && t.SubscriptionId != null);
            if (subscriptionTransaction != null)
            {
                subscriptionTransaction.ProcessingDate = DateTime.Now;
                UnitOfWork.Transactions.Update(subscriptionTransaction);
                UnitOfWork.SaveChanges();

                //var currentUser = CurrentUser;
                //var subscription = await GetUserSubscription();
                //currentUser.Subscription = subscription.Name;
            }
            else
            {
                var subscriptionId = GetSubscriptionByInvoiceId(charge.InvoiceId);
                var transactionToAdd = new Transaction
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscriptionId,
                    UserId = user.Id
                };
                UnitOfWork.Transactions.Insert(transactionToAdd);
                await UnitOfWork.SaveChangesAsync();
            }

            await CreateOrUpdatePromoCode(charge.CustomerId);

        }

        private async Task CreateOrUpdatePromoCode(string customerId)
        {
            var promoCode = await GetPromoCode(customerId);
            if (promoCode == null)
                await CreatePromoCode(customerId);
            else
                await UpdatePromoCode(promoCode.Id, true);
        }

        private Guid GetSubscriptionByInvoiceId(string invoiceId)
        {
            var invoiceService = new InvoiceService();
            var invoice = invoiceService.Get(invoiceId, new InvoiceGetOptions
            {
                Expand = new List<string> { "subscription.items.price.product" }
            });
            var subscriptionName = invoice.Subscription.Items.First().Price.Product.Name;
            var subscription = UnitOfWork.Subscriptions
                .Get()
                .SingleOrDefault(s => s.Name == subscriptionName);
            return subscription.Id;
        }

        private async Task<User> GetUserByCustomerId(string customerId)
        {
            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(customerId);
            var user = await UnitOfWork.Users
                .Get()
                .SingleOrDefaultAsync(u => u.Email == customer.Email);
            return user;
        }

        public async Task DeleteLastSubscriptionTransaction()
        {
            var subscriptionTransaction = await UnitOfWork.Transactions
                .Get()
                .SingleOrDefaultAsync(t => t.UserId == CurrentUser.Id && t.ProcessingDate == null && t.SubscriptionId != null);

            UnitOfWork.Transactions.Delete(subscriptionTransaction);
            UnitOfWork.SaveChanges();

        }

        public async Task<bool> CheckForSubscription()
        {
            var customer = await GetStripeCustomerByEmail(CurrentUser.Email);
            var stripeSubscription = GetStripeSubscriptionByCustomerId(customer.Id);

            if (stripeSubscription == null)
                return false;

            if (stripeSubscription.Status == SubscriptionStatuses.Active)
                return true;
            return false;
        }

        public Dictionary<Guid, int> parseTickets(Dictionary<string, int> tickets)
        {
            var parsedTickets = new Dictionary<Guid, int>();
            foreach (var t in tickets)
            {
                var result = Guid.TryParse(t.Key, out var ticketId);
                parsedTickets.Add(ticketId, t.Value);
            }
            return parsedTickets;
        }

        public async Task<TicketTransactionValidationModel> ValidateTicketTransaction(Guid ticketTransactionId)
        {
            var ticketTransaction = await UnitOfWork.TicketTransactions
                .Get()
                .Include(tt => tt.Ticket)
                    .ThenInclude(t => t.Event)
                    .ThenInclude(e => e.Picture)
                .Where(tt => ticketTransactionId == tt.Id)
                .SingleOrDefaultAsync();

            if (ticketTransaction == null)
                return null;
            var displayValidationResult = Mapper.Map<TicketTransactionValidationModel>(ticketTransaction);

            return displayValidationResult;
        }

        public async Task<bool> CancelSubscription()
        {
            var customer = await GetStripeCustomerByEmail(CurrentUser.Email);
            var subscription = GetStripeSubscriptionByCustomerId(customer.Id);
            if (subscription.Status == SubscriptionStatuses.Active)
            {
                var jobName = Configuration["BackgroundJobName"] + CurrentUser.Email;

                RecurringJob.RemoveIfExists(jobName);
                SubscriptionService.Cancel(subscription.Id, null);
                return true;
            }


            return false;
        }

        public async Task<UserSubscriptionModel> GetUserSubscriptionByEmail(string email)
        {
            var userSubscription = UnitOfWork.Transactions
                .Get()
                .Include(t => t.User)
                .Include(t => t.Subscription)
                .Where(t => t.User.Email == email && t.SubscriptionId != null)
                .OrderByDescending(t => t.ProcessingDate)
                .Select(t => Mapper.Map<UserSubscriptionModel>(t))
                .FirstOrDefault();

            if (userSubscription == null)
                return null;

            var customer = await GetStripeCustomerByEmail(email != null ? email : CurrentUser.Email);

            var stripeSubscription = GetStripeSubscriptionByCustomerId(customer.Id);

            if (stripeSubscription == null)
                return null;

            userSubscription.Status = stripeSubscription.Status;

            return userSubscription;

        }

        private Stripe.Subscription GetStripeSubscriptionByCustomerId(string customerId)
        {
            var options = new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "all",
            };

            var subscriptionList = SubscriptionService.List(options);


            var subscription = subscriptionList.Data
                .OrderByDescending(s => s.Created)
                .FirstOrDefault();

            return subscription;

        }

        private void UpdateFavouriteGenre(string userEmail)
        {
            var favoriteGenre = UnitOfWork.Users
                .Get()
                .Where(user => user.Email == userEmail)
                .Select(user => new
                {
                    UserId = user.Id,
                    FavoriteGenreId = user.Transactions
                        .SelectMany(transaction => transaction.TicketTransactions)
                        .Where(ticketTransaction =>
                            ticketTransaction.Ticket != null &&
                            ticketTransaction.Ticket.Event != null &&
                            ticketTransaction.Ticket.Event.EventNavigation != null)
                        .GroupBy(ticketTransaction => ticketTransaction.Ticket.Event.EventGenreId)
                        .OrderByDescending(group => group.Sum(ticketTransaction => 1))
                        .Select(group => group.Key)
                        .FirstOrDefault()
                })
                .FirstOrDefault();

            var user = UnitOfWork.Users
                .Get()
                .SingleOrDefault(u => u.Email == userEmail);
            user.FavouriteGenre = favoriteGenre.FavoriteGenreId;
            UnitOfWork.Users.Update(user);
            UnitOfWork.SaveChanges();
        }

        public async Task<string> GenerateGift()
        {
            var transaction = await GetUserTransaction(CurrentUser.Email, true);

            if(transaction == null)
            {
                transaction = await CreateTransaction(true);
            }

            if (transaction.TicketTransactions.Count == 0)
            {
                var ticketGift = await GetTicketForGift();

                var ticketTransaction = new TicketTransaction()
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketGift.Id,
                };
                transaction.TicketTransactions.Add(ticketTransaction);

                UnitOfWork.SaveChanges();
            }

            var domain = Configuration["Domain"];

            var price = await GetStripePrice(transaction.TicketTransactions.ElementAt(0).TicketId.Value, false);

            var productList = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions()
                {
                    Price = price.Id,
                    Quantity = 1
                }
            };

            var customer = await GetStripeCustomerByEmail(CurrentUser.Email);

            var promoCode = await GetPromoCode(customer.Id);

            var options = new SessionCreateOptions
            {
                Customer = customer.Id,
                Mode = "payment",
                SuccessUrl = $"{domain}/Ticket/MyTickets",
                CancelUrl = $"{domain}/Ticket/MyTickets",
                Discounts = new List<SessionDiscountOptions>
                {
                    new SessionDiscountOptions
                    {
                        PromotionCode = promoCode.Id
                    }
                }
            };

            var session = CreateCheckout(productList, options);

            return session.Url;
        }

        private async Task UpdatePromoCode(string promoCodeId, bool status)
        {
            var promoCodeService = new PromotionCodeService();
            await promoCodeService.UpdateAsync(promoCodeId, new PromotionCodeUpdateOptions { Active = status });
        }

        private async Task<PromotionCode> GetPromoCode(string customerId)
        {
            var promoCodeService = new PromotionCodeService();
            var promoCodes = await promoCodeService.ListAsync(new PromotionCodeListOptions
            {
                Customer = customerId
            });

            var promoCode = promoCodes.FirstOrDefault();

            return promoCode;
        }

        public async Task<Ticket> GetTicketForGift()
        {
            var user = await UnitOfWork.Users
                .Get()
                .Where(u => u.Id == CurrentUser.Id)
                .SingleOrDefaultAsync();
            var ticketQuery = UnitOfWork.Tickets
                    .Get()
                    .Include(t => t.Event)
                    .Where(t => t.Price <= 150);

            Ticket ticket;
            if (user.FavouriteGenre != null)
            {
                ticketQuery = ticketQuery.Where(t => t.Event.EventGenreId == user.FavouriteGenre);
                var favouriteTickets = await ticketQuery.ToListAsync();

                if (favouriteTickets.Count > 0)
                {
                    var randomIndex = Random.Next(0, favouriteTickets.Count);

                    ticket = favouriteTickets.ElementAt(randomIndex);

                    return ticket;
                }

            }

            var tickets = await ticketQuery.ToListAsync();
            var rndIndex = Random.Next(0, tickets.Count);
            ticket = tickets.ElementAt(rndIndex);
            return ticket;

        }

        public async Task<bool> CheckAvailableGift()
        {
            var subscription = await GetUserSubscriptionByEmail(CurrentUser.Email);
            if (subscription == null || subscription.Status != SubscriptionStatuses.Active || subscription.Name == SubscriptionTypes.Basic.ToString())
                return false;
            var customer = await GetStripeCustomerByEmail(CurrentUser.Email);
            var promoCode = await GetPromoCode(customer.Id);
            return promoCode.Active;
        }

        public async Task CompleteFreeGiftTransaction(Session session)
        {
            var transaction = await GetUserTransaction(session.CustomerDetails.Email, true);
            transaction.ProcessingDate = DateTime.Now;
            var promoCode = await GetPromoCode(session.CustomerId);
            await UpdatePromoCode(promoCode.Id, false);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task HandleChargeSucceeded(Charge charge)
        {
            if (charge.InvoiceId == null)
            {
                await EndTransaction(charge);
            }
            else
            {
                await EndSubscriptionTransaction(charge);
            }
        }
    }
}
