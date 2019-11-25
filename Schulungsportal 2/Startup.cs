using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schulungsportal_2.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Npgsql.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Schulungsportal_2.Services;
using Microsoft.AspNetCore.HttpOverrides;

namespace Schulungsportal_2
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment()) {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<EMailOptions>(Configuration.GetSection("EMailOptions"));

            var dbtype = Configuration.GetSection("ConnectionStrings")["type"];
            var connectionString = Configuration.GetSection("ConnectionStrings")["DefaultConnection"];
            if (dbtype == "mssql") {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
            } else if (dbtype == "postgresql") {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(connectionString));
            } else if (dbtype == "mysql") {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseMySql(connectionString));
            } else{
                throw new Exception("Dbtype has to be 'mssql', 'postgresql' or 'mysql', got "+dbtype);
            }
            services.Configure<IdentityOptions>(o =>
                {
                    // disable some requirements for password, cause I haven't found a way to easily check that on client side
                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequiredLength = 8;
                    o.Password.RequiredUniqueChars = 1;
                    o.SignIn.RequireConfirmedEmail = true;
                });

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // if api request are not authorized return 401 instead of redirect to avoid problems with frontend
            services.ConfigureApplicationCookie(options => {
                options.Events.OnRedirectToLogin = ctx => {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
            
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            // For the framework, only method to send mails
            services.AddTransient<IEmailSender, AuthMessageSender>();
            // For some of our stuff, has some more specific methods to send mails
            services.AddTransient<ISchulungsportalEmailSender, AuthMessageSender>();
            // Background Task for reminders
            services.AddHostedService<AnwesenheitslisteReminderService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddAutoMapper();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            // For requests not going to WebAPI controllers

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
