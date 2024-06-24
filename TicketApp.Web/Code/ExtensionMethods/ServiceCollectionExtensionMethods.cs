using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TicketApp.Common.DTOs;
using TicketApp.WebApp.Code.Base;
using System;
using System.Linq;
using TicketApp.BusinessLogic;
using System.Security.Claims;
using TicketApp.BusinessLogic.Implementation.Account;
using TicketApp.BusinessLogic.Implementation.Events;
using TicketApp.BusinessLogic.Implementation.Locations;
using TicketApp.BusinessLogic.Implementation.Tickets;
using TicketApp.BusinessLogic.Implementation.Artists;
using TicketApp.BusinessLogic.Implementation.Subscriptions;
using TicketApp.BusinessLogic.Implementation.Transactions;
using TicketApp.BusinessLogic.Implementation.Admin;

namespace TicketApp.WebApp.Code.ExtensionMethods
{
    public static class ServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddScoped<ControllerDependencies>();

            return services;
        }

        public static IServiceCollection AddTicketAppBusinessLogic(this IServiceCollection services)
        {
            services.AddScoped<ServiceDependencies>();
            services.AddScoped<UserAccountService>();
            services.AddScoped<EventService>();
            services.AddScoped<LocationService>();
            services.AddScoped<TicketService>();
            services.AddScoped<ArtistService>();
            services.AddScoped<SubscriptionService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<AdminService>();
            return services;
        }

        public static IServiceCollection AddTicketAppCurrentUser(this IServiceCollection services)
        {
            services.AddScoped(s =>
            {
           
                var accessor = s.GetService<IHttpContextAccessor>();
                var httpContext = accessor.HttpContext;
                if (httpContext != null)
                {
                    var claims = httpContext.User.Claims;

                    var userIdClaim = claims?.FirstOrDefault(c => c.Type == "Id")?.Value;
                    var isParsingSuccessful = Guid.TryParse(userIdClaim, out Guid id);
                    var emailClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    var roleClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                    var subscriptionClaim = claims?.FirstOrDefault(c => c.Type == "Subscription")?.Value;
                    var isDisabledClaim = claims?.FirstOrDefault(c => c.Type == "IsDisabled")?.Value;
                    var isParsingIsDisabledSuccessful = Boolean.TryParse(isDisabledClaim, out bool isDisabledResult);

                    return new CurrentUserDto
                    {
                        Id = id,
                        IsAuthenticated = httpContext.User.Identity.IsAuthenticated,
                        Name = httpContext.User.Identity.Name,
                        Email = emailClaim,
                        Role = roleClaim,
                        Subscription = subscriptionClaim,
                        IsDisabled = isDisabledResult

                    };
                }
                return new CurrentUserDto() { };
            });

            return services;
        }
    }
}
