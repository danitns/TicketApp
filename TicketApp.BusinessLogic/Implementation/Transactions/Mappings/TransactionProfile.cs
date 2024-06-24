using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Subscriptions;
using TicketApp.BusinessLogic.Implementation.Tickets.Models;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Transactions
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, ShoppingCartModel>()
                .ForMember(d => d.AddedTickets, d => d.MapFrom(s => s.TicketTransactions
                        .GroupBy(tt => new { ticketName = tt.Ticket.Name, eventName = tt.Ticket.Event.Name })
                        .Select(group => new ShoppingCartTicket
                        {
                            Name = group.Key.ticketName,
                            EventName = group.Key.eventName,
                            Quantity = group.Count(),
                            Description = group.First().Ticket.Description,
                            Price = group.First().Ticket.Price
                        })));

            CreateMap<Transaction, UserSubscriptionModel>()
                .ForMember(d => d.Name, d => d.MapFrom(s => s.Subscription.Name))
                .ForMember(d => d.Price, d => d.MapFrom(s => s.Subscription.Price))
                .ForMember(d => d.Discount, d => d.MapFrom(s => s.Subscription.Discount))
                .ForMember(d => d.Status, d => d.Ignore());

            CreateMap<TicketTransaction, TicketTransactionValidationModel>()
                .ForMember(d => d.EventPicture, d => d.MapFrom(s => s.Ticket.Event.Picture.PictureContent))
                .ForMember(d => d.StartDate, d => d.MapFrom(s => s.Ticket.Event.StartDate))
                .ForMember(d => d.EventId, d => d.MapFrom(s => s.Ticket.Event.Id))
                .ForMember(d => d.EndDate, d => d.MapFrom(s => s.Ticket.Event.EndDate))
                .ForMember(d => d.Name, d => d.MapFrom(s => s.Ticket.Name))
                .ForMember(d => d.Description, d => d.MapFrom(s => s.Ticket.Description));

            CreateMap<Transaction, DisplayTransactionModel>()
                .ForMember(d => d.SubscriptionName, d => d.MapFrom(s => s.Subscription.Name))
                .ForMember(d => d.Tickets, d => d.Ignore())
                .ForMember(d => d.TotalPrice, d => d.MapFrom(s => s.TicketTransactions.Sum(tt => tt.Ticket.Price) + (s.Subscription != null ? s.Subscription.Price : 0)));
        }
    }
}
