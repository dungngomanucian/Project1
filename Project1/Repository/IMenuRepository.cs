using Project1.Models;
namespace Project1.Repository_Menu
{
    public interface IMenuRepository
    {
        TCategory Add(TCategory category);
        TCategory Update (TCategory category);
        TCategory Delete (String categoryID);
        TCategory GetCategory(String categoryID);
        IEnumerable<TCategory> GetAllCategories();
    }
}
