using Authentication.Mfa.Twilio.Data;
using Authentication.Mfa.Twilio.Security;
using Authentication.Mfa.Twilio.Services.Implementations;
using Authentication.Mfa.Twilio.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Authentication.Mfa.Twilio.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
        services.Configure<MfaTokenOptions>(configuration.GetSection("MfaTokenOptions"));
        services.Configure<TwilioOptions>(configuration.GetSection("Twilio"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=auth-mfa.db"));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<ITwilioVerifyService, TwilioVerifyService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddHttpClient<ITwilioVerifyService, TwilioVerifyService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtOptions:SecretKey"] ?? "super-secret-key-for-dev-only")),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        return services;
    }
}
