using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Configurations;
using System.Reflection;
using System.Text;
using SAWSCore8API.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Identity.UI.Services;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ConnStr") ?? throw new InvalidOperationException("Connection string 'ConnStr' not found.");

builder.Services.ResolveDependencies(builder.Configuration);
// Register HttpClient
// builder.Services.AddHttpClient();
builder.Services.AddControllers();

const string DefaultCorsPolicy = "DefaultCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<SAWSDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// add/setup Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 4;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddRoleManager<RoleManager<IdentityRole>>()
.AddEntityFrameworkStores<SAWSDbContext>()
.AddDefaultTokenProviders();

// add/setup Authentication via JWT tokens
builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    // ValidateLifetime = true,
                    // ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                };
            });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SAWS API",
        Description = "An ASP.NET Core Web API for managing SAWS application",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }

    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "SAWS API",
        Description = "An ASP.NET Core Web API for managing SAWS application version 2"
    });

});

builder.Logging.AddConsole();

var app = builder.Build();

// configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });

    app.UseSwaggerUI(options =>
    {
        options.InjectStylesheet("/swagger-ui/custom.css");
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aviation API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Aviation API V2");
        //options.RoutePrefix = string.Empty;
    });
}

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    try
    {
        var dbContext = service.GetRequiredService<SAWSDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = service.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying migrations.");
    }
}

app.UseHttpsRedirection();

app.UseStaticFiles();

/*app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/Uploads"
});*/

app.UseCors(DefaultCorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/", () => "Default Web API endpoint");

app.MapControllers();

app.Run();
