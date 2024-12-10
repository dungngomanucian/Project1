using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project1.DTO;
using Project1.Models;

namespace Project1.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuAPIController : ControllerBase
    {
        private PizzaOnlineContext db = new PizzaOnlineContext();

        [HttpGet("{id}/details")]
        public IActionResult GetProductDetails(int id)
        {
            var productDetails = db.TProductDetails.Where(d => d.ProductId == id).Select(pd => new ProductDetailDTO
            {
                ProductDetailId = pd.ProductDetailId,
                Price = pd.Price,
                CrustId = pd.CrustId,
                SizeId = pd.SizeId,
                Number = pd.Number
            }).ToList();

            if (!productDetails.Any())
            {
                return NotFound();
            }

            return Ok(productDetails);
        }
    }
}
