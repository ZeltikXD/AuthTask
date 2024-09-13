using AuthTask.Data;
using AuthTask.Interfaces;
using AuthTask.Interfaces.Implementations;
using AuthTask.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AuthDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("SqlConnection")));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthManager, AuthManager>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = builder.Environment.IsProduction(),
                    ValidateAudience = builder.Environment.IsProduction(),
                    ValidAudience = builder.Configuration["JWTConfig:Audience"],
                    ValidIssuer = builder.Configuration["JWTConfig:Issuer"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.Unicode.GetBytes(builder.Configuration["JWTConfig:SecretKey"] ?? string.Empty))
                };
            });
            builder.Services.AddOptions<TokenOptions>().Configure(o 
                => o.SetSecretKey(builder.Configuration["JWTConfig:SecretKey"])
                    .SetIssuer(builder.Configuration["JWTConfig:Issuer"])
                    .SetAudience(builder.Configuration["JWTConfig:Audience"])
                    .SetExpiresInHours(builder.Configuration.GetValue<double>("JWTConfig:ExpiresInHours")));
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            if (!EnsureDatabaseCreated(app)) return;

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (app.Environment.IsProduction())
                app.UseForwardedHeaders(new()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

            app.UseStatusCodePages(context => {
                var request = context.HttpContext.Request;
                var response = context.HttpContext.Response;

                if (response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    var returnUrl = request.Path + request.QueryString;
                    response.Redirect($"/auth/login?returnUrl={returnUrl}");
                }

                return Task.CompletedTask;
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Cookies.TryGetValue("session_token", out var token) && !string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");
                }
                await next(context);
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static bool EnsureDatabaseCreated(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                dbContext.Database.Migrate();
                return true;
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthDbContext>>();
                logger.LogError(ex, "Migrations couldn't be applied. The application will stop.");
                return false;
            }
        }
    }
}
