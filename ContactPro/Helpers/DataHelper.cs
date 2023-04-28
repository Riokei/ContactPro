using ContactPro.Data;
using ContactPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace ContactPro.Helpers
{
    
    public static class DataHelper

    {        
        
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();  

            //migration: this is equivalent to update-database
            await dbContextSvc.Database.MigrateAsync();
        }

        //Attempt to create demo log in
        public static async Task SeedAdminAsync (IServiceProvider svcProvider)
        {
            //Get dependencies from service provider.
            UserManager<AppUser>? userManager = svcProvider.GetRequiredService<UserManager<AppUser>>();
            IConfiguration config = svcProvider.GetRequiredService<IConfiguration>();
            //Make sure the user doesn't exist already
            if (await userManager.FindByEmailAsync("email@email.com") == null)
            {
                //create the user
                AppUser demoUser = new()
                {
                    Email = "email@email.com",
                    UserName = "email@email.com",
                    FirstName = "Demo",
                    LastName = "User",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(demoUser, config.GetSection("demoPassword")["Password"] ?? Environment.GetEnvironmentVariable("demoPassword"));
            }
        }
    }
}
