using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project1.Models;
using Project1.ViewModels;

namespace Project1.Controllers
{
    public class MenuController : Controller
    {
        private PizzaOnlineContext db = new PizzaOnlineContext();
        public IActionResult Index()
        { 
            var lstProduct = db.TProducts.Include(p => p.TProductDetails).ToList();

            // Lấy địa chỉ từ Session
            var selectedAddress = HttpContext.Session.GetString("SelectedAddress");

            // Truyền giá trị vào ViewBag để hiển thị trong view nếu cần
            ViewBag.SelectedAddress = selectedAddress;
            
            return View(lstProduct);
        }

        public IActionResult ProductByCategory(long CategoryID)
        {
            List<CategoryViewModel> lstProduct = db.TCategories
                .Where(x => x.CategoryId == CategoryID)
                .Select(category => new CategoryViewModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Products = category.TProducts.Select(product => new ProductViewModel
                    {
                        ProductId = product.ProductId,
                        Image = product.Image,
                        Description = product.Description,
                        Name = product.Name,
                        ProductDetails = product.TProductDetails.ToList()
                    }).ToList()
                })
                .ToList();

            // Lấy địa chỉ từ Session
            var selectedAddress = HttpContext.Session.GetString("SelectedAddress");

            // Truyền giá trị vào ViewBag để hiển thị trong view nếu cần
            ViewBag.SelectedAddress = selectedAddress;

            return View(lstProduct);
        }
    }
}
