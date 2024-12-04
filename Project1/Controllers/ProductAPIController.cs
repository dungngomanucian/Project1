using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project1.Models;
using Project1.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly PizzaOnlineContext db = new PizzaOnlineContext();

        [HttpGet]
        public IEnumerable<ProductViewModel> GetAllProducts()
        {
            // Lấy danh sách TProducts từ database và ánh xạ sang CategoryViewModel
            var productViewModels = db.TProducts.Select(product => new ProductViewModel
            {
                ProductId = product.ProductId,  
                Image = product.Image,
                Description = product.Description,
                Name = product.Name,
                ProductDetails = product.TProductDetails.ToList()
            }).ToList();

            return productViewModels;
        }
        [HttpGet("{CategoryID}")]
        public IEnumerable<ProductViewModel> GetProductsByCategory(long CategoryID)
        {
            // Lấy danh sách TProducts từ database và ánh xạ sang CategoryViewModel
            var productViewModels = db.TProducts.Where(x=>x.CategoryId == CategoryID)
                .Select(product => new ProductViewModel{
                    ProductId = product.ProductId,  
                    Image = product.Image,
                    Description = product.Description,
                    Name = product.Name,
                    ProductDetails = product.TProductDetails.ToList()
                }).ToList();

            return productViewModels;
        }
    }
}
