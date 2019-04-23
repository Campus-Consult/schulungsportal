using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schulungsportal_2.Data
{
    public static class DefaultAdminSeed
    {
        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task CreateAdminUser(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            IConfigurationSection adminSettings = configuration.GetSection("DefaultAdminUser");
            if (!adminSettings.Exists())
            {
                logger.Error("The config section \"DefaultAdminUser\" is missing, no initial Admin could be created!");
                return;
            }
            string username = adminSettings["UserEmail"];
            string password = adminSettings["UserPassword"];
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Create default Admin role "Verwaltung"
            var verwaltung = "Verwaltung";
            if (!await RoleManager.RoleExistsAsync(verwaltung))
            {
                await RoleManager.CreateAsync(new IdentityRole(verwaltung));
            }

            // Create Admin user, if not already exists
            var user = await UserManager.FindByEmailAsync(username);
            if (user == null)
            {
                var newUser = new IdentityUser
                {
                    Email = username,
                    UserName = username,
                };
                var createUser = await UserManager.CreateAsync(newUser, password);
                if (createUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(newUser, verwaltung);
                }
            }
        }
    }
}
