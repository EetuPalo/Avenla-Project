using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Login_System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Login_System.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http; // For Caching
using Microsoft.AspNetCore.Mvc.Razor;
using SQLitePCL;
using Resources;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Login_System
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<AppUser>(options => 
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddRoles<AppRole>()
                .AddEntityFrameworkStores<IdentityDataContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();

            //Configuring the database context with the right connection string
            services.AddDbContext<GeneralDataContext>(options =>
            {
                //Getting the connection string from appsettings.json
                var connectionString = Configuration.GetConnectionString("ApplicationConnection");
                //Using the connection string
                options.UseSqlServer(connectionString);
            });
            services.AddDbContext<IdentityDataContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("ApplicationConnection");
                options.UseSqlServer(connectionString);
            });

            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.LoginPath = "/Account/Login";
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            //services.AddLocalization(options => options.ResourcesPath = "");
            //services.TryAddSingleton<IStringLocalizer, ZLocalizer>();
            services.Configure<RequestLocalizationOptions>(
            options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
               new CultureInfo("en-GB"),
               new CultureInfo("fi-FI"),
                };
                options.DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                services.AddSingleton(options);
            });

            //Sets lifetime for password reset tokens
            services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromHours(24));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("NoAdminRequired", policy => policy.RequireClaim("Id"));
            });

            services
                .AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager, GeneralDataContext _context)
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

            //LOCALIZATION STUFF
            /*
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("fi-FI")
            };

            var requestLocalizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            app.UseRequestLocalization(requestLocalizationOptions);
            */

            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOptions.Value);

            /*
            app.UseRouter(routes =>
            {
                routes.MapMiddlewareRoute("{culture=hu-HU}/{*mvcRoute}", subApp =>
                {
                    #region Multilanguage
                    subApp.UseRequestLocalization(localizationOptions.Value);
                    #endregion

                    subApp.UseEndpoints(endpoints =>
                    {

                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{culture=en-GB}/{controller=Home}/{action=Index}/{id?}");
                    });
                });
            });
            */
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();



            app.UseAuthentication();
            int id = MyIdentityDataInitializer.SeedData(userManager, roleManager,_context);
            MyIdentityDataInitializer.SeedUsers(userManager, id);

            app.UseAuthorization();

            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{culture=fi_FI}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });  
        }

        public static class MyIdentityDataInitializer
        {
            public static int SeedData
        (
        UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, GeneralDataContext _context)
            {
                int id = 0;
                Company company = new Company
                {
                    Name = "Superadmin",
                    Description = "Placeholder"
                };
                AppRole role = new AppRole
                {
                    Name = "Superadmin"
                };

                roleManager.CreateAsync(role);

                AppRole adminrole = new AppRole
                {
                    Name = "Admin"
                };

                roleManager.CreateAsync(adminrole);

                AppRole userrole = new AppRole
                {
                    Name = "User"
                };

                roleManager.CreateAsync(userrole);

                if (!_context.Company.Any())
                {

                    _context.Company.Add(company);
                    _context.SaveChanges();

                    id = company.Id;

                    Console.Write("");
                  
                }

                return id;
            }

            public static void SeedUsers(UserManager<AppUser> userManager, int id)
            {
            
                if (!userManager.Users.Any())
                {
                    
                    AppUser user = new AppUser
                    {
                        UserName = "adminadmin",
                        Email = "admin@admin.fi",
                        FirstName = "Admin",
                        LastName = "Admin",
                        EmpStatus = "Active",
                        PhoneNumber = "87650",
                        Company = id

                    };
                    IdentityResult result = userManager.CreateAsync(user , "Koodaus1!").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user,
                               "Superadmin").Wait();
                    }
                }
            }

            public static void SeedRoles
        (RoleManager<AppRole> roleManager)
            {
            }
        }
    }
}
