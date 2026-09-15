using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Services;
using AuthApi.Middleware;
using AuthApi.Policies;
using Microsoft.AspNetCore.Authorization;

namespace AuthApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((hostContext, services) =>
                    {
                        services.AddCors(options =>
                        {
                            options.AddPolicy("CorsPolicy", builder =>
                            {
                                builder.WithOrigins("http://localhost:5173", "http://localhost:5174")
                                       .AllowAnyMethod()
                                       .AllowAnyHeader()
                                       .AllowCredentials();
                            });
                        });

                        services.AddControllers();
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection")));

                        services.Configure<Argon2Options>(hostContext.Configuration.GetSection("Argon2"));
                        services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
                        services.AddSingleton<ISessionService, InMemorySessionService>();
                        services.AddSingleton<IVerificationService, MockVerificationService>();
                        
                        services.AddAuthentication("SessionAuth")
                            .AddCookie("SessionAuth", options =>
                            {
                                options.Cookie.Name = "session_id";
                                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                                options.SlidingExpiration = true;
                            });

                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy("VerifiedEmail", policy => 
                                policy.Requirements.Add(new VerifiedEmailRequirement()));
                        });
                        services.AddScoped<IAuthorizationHandler, VerifiedEmailHandler>();
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseCors("CorsPolicy");
                        app.UseMiddleware<SessionMiddleware>();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                    });
                });
        }
    }