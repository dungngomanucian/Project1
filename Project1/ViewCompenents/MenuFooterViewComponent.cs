using Project1.Models;
using Microsoft.AspNetCore.Mvc;
using Project1.Repository_Menu;
using Microsoft.AspNetCore.Mvc.ViewComponents;
namespace Project1.ViewCompenents;

public class MenuFooterViewComponent: ViewComponent
{
    private readonly IMenuRepository _category;
    public MenuFooterViewComponent(IMenuRepository categoryRepository)
    {
        _category = categoryRepository;
    }
    public IViewComponentResult Invoke()
    {
        var category = _category.GetAllCategories().OrderBy(x => x.CategoryId);
        return View(category);
    }
}
