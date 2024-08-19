using SAWSCore8API.Interfaces;
using SAWSCore8API.Services;

namespace SAWSCore8API.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            // services.AddScoped<IProductRepository, ProductRepository>();
            // services.AddScoped<IOrderRepository, OrderRepository>();

            // services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IAdvertService, AdvertService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISawsService, SawsService>();

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