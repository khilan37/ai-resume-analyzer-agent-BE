using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartResumeAnalyzerAgent.Application.Abstractions.AI;
using SmartResumeAnalyzerAgent.Application.Abstractions.Auth;
using SmartResumeAnalyzerAgent.Application.Abstractions.Files;
using SmartResumeAnalyzerAgent.Application.Abstractions.Persistence;
using SmartResumeAnalyzerAgent.Application.Features.Auth;
using SmartResumeAnalyzerAgent.Application.Features.Resumes;
using SmartResumeAnalyzerAgent.Infrastructure.AI;
using SmartResumeAnalyzerAgent.Infrastructure.Auth;
using SmartResumeAnalyzerAgent.Infrastructure.Files;
using SmartResumeAnalyzerAgent.Infrastructure.Persistence;
using SmartResumeAnalyzerAgent.Infrastructure.Persistence.Repositories;

namespace SmartResumeAnalyzerAgent.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IResumeRepository, ResumeRepository>();
        services.AddScoped<IResumeTextExtractor, ResumeTextExtractor>();
        services.AddHttpClient<IAiResumeAnalyzerService, GeminiSemanticKernelResumeAnalyzerService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<ResumeService>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = key
                };
            });

        services.AddAuthorization();
        return services;
    }
}
