using Project1.Models;
namespace Project1.Repository_Menu
{
    public class MenuRepository : IMenuRepository
    {
        private readonly PizzaOnlineContext _context;
        public MenuRepository (PizzaOnlineContext context)
        {
            _context = context;
        }
        public TCategory Add(TCategory category)
        {
            _context.TCategories.Add(category);
            _context.SaveChanges();
            return category;
        }

        public TCategory Delete(string categoryID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TCategory> GetAllCategories()
        {
            return _context.TCategories;
        }

        public TCategory GetCategory(string categoryID)
        {
            return _context.TCategories.Find(categoryID);
        }

        public TCategory Update(TCategory category)
        {
            _context.Update(category);
            _context.SaveChanges();
            return category;
        }
    }
}
