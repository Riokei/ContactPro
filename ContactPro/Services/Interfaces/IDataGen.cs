using ContactPro.Models;

namespace ContactPro.Services.Interfaces
{
    public interface IDataGen
    {
        Task<List<Contact>> GenerateContacts(AppUser User);
        Task<List<Category>> GenerateCategories(AppUser User);
    }
}
