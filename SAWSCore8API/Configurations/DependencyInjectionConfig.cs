using SAWSCore8API.Interfaces;
using SAWSCore8API.Services;

namespace SAWSCore8API.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            // Register HttpClient
            services.AddHttpClient();
            services.AddScoped<IAdvertService, AdvertService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<IFileManagerService, FileManagerService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IPagedService, PagedService>();
            services.AddTransient<IEmailService, EmailService>();

            services.AddSingleton<IUriService>(o =>
            {
                var accessor = o.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent() + request.PathBase);
                return new UriService(uri);
            });

            return services;
        }
    }
}