using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
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

        var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        var connectionString = NormalizePostgresConnectionString(rawConnectionString);

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dataSource));

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

    private static string NormalizePostgresConnectionString(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return connectionString;
        }

        var userInfoParts = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfoParts[0])
        };

        if (userInfoParts.Length > 1)
        {
            builder.Password = Uri.UnescapeDataString(userInfoParts[1]);
        }

        var query = ParseQueryString(uri.Query);

        if (query.TryGetValue("sslmode", out var sslMode))
        {
            builder.SslMode = Enum.Parse<SslMode>(sslMode, ignoreCase: true);
        }

        if (query.TryGetValue("trust server certificate", out var trustServerCertificate) ||
            query.TryGetValue("trustservercertificate", out trustServerCertificate))
        {
            builder.TrustServerCertificate = bool.Parse(trustServerCertificate);
        }

        if (query.TryGetValue("pooling", out var pooling))
        {
            builder.Pooling = bool.Parse(pooling);
        }

        return builder.ConnectionString;
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var keyValue = part.Split('=', 2);
            var key = Uri.UnescapeDataString(keyValue[0]);
            var value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1]) : string.Empty;
            result[key] = value;
        }

        return result;
    }
}
