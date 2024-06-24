using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Common.DTOs;
using TicketApp.Common.Exceptions;
using TicketApp.Common.Extensions;
using TicketApp.Entities;
using System.Reflection.Metadata;
using Stripe;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Tickets
{
    public class TicketService : BaseService
    {
        private readonly CreateTicketValidator CreateTicketValidator;
        public TicketService(ServiceDependencies serviceDependencies) : base(serviceDependencies)
        {
            CreateTicketValidator = new CreateTicketValidator();
        }


        public async Task CreateTicket(CreateTicketModel model)
        {
            CreateTicketValidator.Validate(model).ThenThrow(model);

            var newTicket = Mapper.Map<CreateTicketModel, Ticket>(model);

            var ticketEvent = await UnitOfWork.Events
                .Get()
                .Where(e => e.Id == newTicket.EventId)
                .SingleOrDefaultAsync();

            if (CurrentUser.Role != "Admin" && CurrentUser.Id != ticketEvent.OrganizerId)
            {
                throw new AccessViolationException();
            }

            UnitOfWork.Tickets.Insert(newTicket);
            UnitOfWork.SaveChanges();


            var productOptions = new ProductCreateOptions
            {
                Id = newTicket.Id.ToString(),
                Name = newTicket.Name
            };

            var productService = new ProductService();
            var stripeTicket = productService.Create(productOptions);

            var priceOptions = new PriceCreateOptions
            {
                Currency = CurrencyTypes.ron.ToString(),
                Product = stripeTicket.Id,
                UnitAmount = (long?)newTicket.Price * 100
            };

            var priceService = new PriceService();
            priceService.Create(priceOptions);
        }

        public async Task DeleteTicket(Guid id)
        {
            var ticketToDelete = await UnitOfWork.Tickets
                .Get()
                .SingleOrDefaultAsync(e => e.Id == id);

            if (ticketToDelete == null)
                throw new NotFoundErrorException();

            if (ticketToDelete.Event.OrganizerId != CurrentUser.Id && CurrentUser.Role != UserRoles.Admin.ToString())
                throw new AccessViolationException();

            UnitOfWork.Tickets.Delete(ticketToDelete);

            UnitOfWork.SaveChanges();
        }

        public async Task<List<DisplayTicketModel>> GetTicketDetails(string[] ticketIds)
        {
            List<Guid> ticketGuids = new();
            foreach (var id in ticketIds)
            {
                var parseResult = Guid.TryParse(id, out var ticketIdToGuid);
                if (parseResult == false)
                {

                }
                ticketGuids.Add(ticketIdToGuid);
            }

            var tickets = await UnitOfWork.Tickets
                .Get()
                .Where(t => ticketGuids.Contains(t.Id))
                .ToListAsync();

            var ticketDetails = new List<DisplayTicketModel>();

            foreach (var t in tickets)
            {
                var displayTicket = Mapper.Map<DisplayTicketModel>(t);
                ticketDetails.Add(displayTicket);
            }

            return ticketDetails;
        }

        public async Task<List<MyTicketModel>> GetTicketsForCurrentUser()
        {

            var tickets = await UnitOfWork.Tickets
            .Get()
            .Include(t => t.TicketTransactions)
                .ThenInclude(tt => tt.Transaction)
            .Include(t => t.Event)
                .ThenInclude(t => t.Picture)
            .Include(t => t.Event)
                .ThenInclude(t => t.Location)
            .Where(t => t.TicketTransactions.Any(tt => tt.Transaction.UserId == CurrentUser.Id) && t.Event.EndDate > DateTime.Now)
            .ToListAsync();

            foreach (var t in tickets)
            {
                t.TicketTransactions = t.TicketTransactions.Where(tt => tt.Transaction.UserId == CurrentUser.Id).ToList();
            }

            var myTickets = tickets
                .Select(t => Mapper.Map<MyTicketModel>(t))
                .ToList();

            return myTickets;

        }

        public async Task<List<string>> GetQrCodes(string ticketName, Guid eventId)
        {
            var buyedTickets = await UnitOfWork.TicketTransactions
                .Get()
                .Include(tt => tt.Ticket)
                .Include(tt => tt.Transaction)
                .Where(tt => tt.Transaction.UserId == CurrentUser.Id && tt.Ticket.Name == ticketName && tt.Ticket.EventId == eventId)
                .Select(tt => ConvertGuidToQRCode(tt.Id))
                .ToListAsync();
            return buyedTickets;
        }

        private static string ConvertGuidToQRCode(Guid id)
        {
            var text = $"https://localhost:7034/Transaction/ValidateTicket?ticketTransactionId={id}";
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(60);
            byte[] BitmapArray = BitmapToByteArray(QrBitmap);
            string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            return QrUri;
        }
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
