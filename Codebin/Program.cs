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

            string domain = string.Empty;
            string client_id = string.Empty;
            string client_secret = string.Empty;

            if(!builder.Environment.IsDevelopment())
            {
                domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
                client_id = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID");
                client_secret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET");
            }
            else
            {
                domain = builder.Configuration["Auth0:Domain"];
                client_id = builder.Configuration["Auth0:ClientId"];
                client_secret = builder.Configuration["Auth0:ClientSecret"];
            }
            
            // App settings
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
                .AddCookie(options =>
                {
                    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                })
                .AddOpenIdConnect("Auth0", options =>
                {
                    options.Authority =  domain;
                    options.ClientId = client_id;
                    options.ClientSecret = client_secret;

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
                            var logoutUri = $"{domain}/v2/logout?client_id={client_id}";

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
                            var uri = new UriBuilder(context.ProtocolMessage.RedirectUri);

                            uri.Scheme = "https";

                            if(!builder.Environment.IsDevelopment())
                            {
                                context.ProtocolMessage.RedirectUri = $"{uri.Scheme}://{uri.Host}{uri.Path}";
                            }
                            else
                            {
                                context.ProtocolMessage.RedirectUri = $"{uri.Scheme}://{uri.Host}:{uri.Port}{uri.Path}";
                            }

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

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}