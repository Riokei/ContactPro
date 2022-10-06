using ContactPro.Models;

namespace ContactPro.Services.Interfaces
{
    public interface IAddressBookService
    {
        Task AddContactToCategoryAsync(int categoryId, int contactId);
        Task<bool> IsContactInCategory(int categoryId, int contactId);

        Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId);
        Task<ICollection<int>> GetContactCategory(int contactId);
        Task<ICollection<Category>> GetContactCategoriesAsync(int contactId);
        Task RemoveContactFromCategoryAsync(int categoryId, int contactId);
        IEnumerable<Category> SearchForContacts(string seachString, string userId);
    }
}
