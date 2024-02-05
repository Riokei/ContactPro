using ContactPro.Data;
using ContactPro.Models;
using ContactPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
namespace ContactPro.Helpers
{
    
    public static class DataHelper 

    {
        //private readonly static ApplicationDbContext context;
        //private readonly static IDataGen dataGen;
        

        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();  

            //migration: this is equivalent to update-database
            await dbContextSvc.Database.MigrateAsync();
        }

        //Attempt to create demo log in
        public static async Task SeedAdminAsync (IServiceProvider svcProvider, IDataGen data, ApplicationDbContext context)
        {
            //Get dependencies from service provider.
            UserManager<AppUser>? userManager = svcProvider.GetRequiredService<UserManager<AppUser>>();
            IConfiguration config = svcProvider.GetRequiredService<IConfiguration>();           
            //Make sure the user doesn't exist already
           try {
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
                    List<Contact> contacts = await data.GenerateContacts(demoUser);
                    List<Category> categories = await data.GenerateCategories(demoUser);
                    Random random = new();
                    foreach (Contact contact in contacts)
                    {
                        int numCat = random.Next(1,5);
                        var category = categories.OrderBy(categories => Guid.NewGuid()).Take(numCat).ToList();

                        //Fix this.
                        contact.Categories.AddRange(category);
                    }

                    foreach (Contact contact in contacts) { context.Add(contact); }
                    foreach (Category category in categories) { context.Add(category); }

                    await context.SaveChangesAsync();

                }
            } catch {
                throw;
            }
            
        }
    }
}
