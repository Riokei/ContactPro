using ContactPro.Data;
using ContactPro.Models;
using ContactPro.Services.Interfaces;
using Microsoft.Extensions.Hosting.Internal;


namespace ContactPro.Services
{
    public class DataGenService : IDataGen
    {

        private List<Contact> _contacts;
        private List<Category> _categories;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        
        public DataGenService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<List<Category>> GenerateCategories(AppUser User)
        {
            if (User is not null)
            {
                _categories = new List<Category>()
                {
                    new Category(){AppUserId = User.Id, Name = "Work"},
                    new Category(){AppUserId = User.Id, Name = "Family"},
                    new Category(){AppUserId = User.Id, Name = "Surprise Party 2025"}
                };
            }
            
            return _categories;
        }

        public async Task<List<Contact>> GenerateContacts(AppUser User)
        {
            string currentDirectory = Directory.GetCurrentDirectory().ToString();
            
           
            if (User is not null)
            {
                _contacts = new List<Contact>()
            {
                    //TO-DO
                    //✔️ 1. Change images to something not provided by coder foundry.  
                    //2. Make more contacts, set some contacts to categories.
                    //3. See about how to test the email properly...
                new Contact() {AppUserId = User.Id,FirstName = "Joan", LastName = "Smith", Address1 = "1234 Apple Street", BirthDate = new DateTime(1994, 08, 16), City = "Paris", State = Enums.States.TX, Email = "test@test.com", ZipCode = 21000, PhoneNumber = "0000000000", ImageData = File.ReadAllBytes(Path.Combine(currentDirectory, "wwwroot//img//alex-shaw-7TntdeL9FZ0-unsplash.jpg")), ImageType = ".png" },

                new Contact() {AppUserId = User.Id, FirstName = "Erin", LastName = "Doe", Address1 = "1 Katz Place", BirthDate = new DateTime(1976, 01, 02), City = "Charleston", State = Enums.States.WV, Email = "test@test.com", ZipCode = 18701, PhoneNumber = "0000000000", ImageData = File.ReadAllBytes(Path.Combine(currentDirectory, "wwwroot//img//megan-ruth-nBbB795RRV4-unsplash.jpg")), ImageType = ".png" },

                new Contact() {AppUserId = User.Id, FirstName = "Ty", LastName = "Long", Address1 = "11 Dell Place", BirthDate = new DateTime(1979, 05, 12), City = "Roanoke", State = Enums.States.VA, Email = "test@test.com", ZipCode = 18701, PhoneNumber = "0000000000", ImageData = File.ReadAllBytes(Path.Combine(currentDirectory, "wwwroot//img//marya_volk-SbYRkfmgwUU-unsplash.jpg")), ImageType = ".png" }
            };
            }

            

            return _contacts;
        }
    }
}
