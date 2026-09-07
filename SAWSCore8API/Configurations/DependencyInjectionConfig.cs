using DeviceDetectorNET.Parser.Device;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Options;
using SAWSCore8API.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace SAWSCore8API.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Register HttpClient
            services.AddHttpClient();

            services.Configure<PayFastOptions>(configuration.GetSection("PayFast"));

            services.AddScoped<IAdvertService, AdvertService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<IFileManagerService, FileManagerService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IRawSourceService, RawSourceService>();
            services.AddScoped<IPagedService, PagedService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IActivityLoggerService, ActivityLoggerService>();

            services.AddSingleton<IUriService>(o =>
            {
                var accessor = o.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext?.Request;
                var uri = request == null
                    ? string.Empty
                    : string.Concat(request.Scheme, "://", request.Host.ToUriComponent() + request.PathBase);
                return new UriService(uri);
            });

            return services;
        }
    }
}