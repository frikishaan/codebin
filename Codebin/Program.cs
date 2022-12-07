using Codebin.Models;
using Codebin.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Codebin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            
            // App settings
            builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection("Auth0"));
            builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

            // Lowercase URLs
            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            // Auth0 Service
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddOpenIdConnect("Auth0", options =>
                {
                    options.Authority = builder.Configuration["Auth0:Domain"] ?? Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
                    options.ClientId = builder.Configuration["Auth0:ClientId"] ?? Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID");
                    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET");

                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");

                    options.CallbackPath = new PathString("/callback");

                    options.ClaimsIssuer = "Auth0";

                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProviderForSignOut = (context) =>
                        {
                            var logoutUri = $"${builder.Configuration["Auth0:Domain"] ?? Environment.GetEnvironmentVariable("AUTH0_DOMAIN")}/v2/logout?client_id=${builder.Configuration["Auth0:ClientId"] ?? Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID")}";

                            var postLogoutUri = context.Properties.RedirectUri;
                            if (!string.IsNullOrEmpty(postLogoutUri))
                            {
                                if (postLogoutUri.StartsWith("/"))
                                {
                                    var request = context.Request;
                                    postLogoutUri = $"https://{request.Host}{request.PathBase}{postLogoutUri}";
                                }
                                logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                            }

                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        },
                        OnRedirectToIdentityProvider = (context) =>
                        {
                            var builder = new UriBuilder(context.ProtocolMessage.RedirectUri);

                            builder.Scheme = "https";

                            context.ProtocolMessage.RedirectUri = $"{builder.Scheme}://{builder.Host}" + (builder.Port != 80 ? ":" + builder.Port : "") + $"{builder.Path}";

                            return Task.FromResult(0);
                        }
                    };
                });

            // MongoDB Service
            builder.Services.AddSingleton<MongoDBService>();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
    }
}