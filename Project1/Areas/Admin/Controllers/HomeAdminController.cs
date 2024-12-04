using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project1.Models;
using Project1.Models.Authentication;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Project1.Areas.Admin.Controllers
{
	
	[Area("admin")]
	[Route("admin")]
	[Route("admin/homeadmin")]
	public class HomeAdminController : Controller
	{
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeAdminController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        PizzaOnlineContext db= new PizzaOnlineContext();
		[Authentication]
		[Route("")]
		[Route("index")]
		public IActionResult Index()
		{
            
            return View();
		}

		
		[Route("ListProduct")]
		public IActionResult ListProduct(int? page)
		{
            
            int pageSize = 15;
			int pageNumber =page ==null || page<0 ? 1:page.Value;	
			var lstProduct = db.TProducts.AsNoTracking().Include(x=>x.Category).OrderBy(x=>x.Name);
			PagedList<TProduct> lst= new PagedList<TProduct>(lstProduct,pageNumber,pageSize);
			return View(lst);
		}
        [Route("ListProductOptimizeByCategory")]
        public IActionResult ListProductOptimizeByCategory(int? page,string categoryId)
        {
            long change = long.Parse( categoryId);
            int pageSize = 15;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).Where(x=>x.CategoryId==change).OrderBy(x => x.Name);
            PagedList<TProduct> lst = new PagedList<TProduct>(lstProduct, pageNumber, pageSize);
            ViewBag.CurrentCategoryId = categoryId;
            return View(lst);
        }

		


		//từ đây trở xuống là các action tạo các địa điểm riêng cho từng loại
		[Route("AddProduct")]
        [HttpGet]
        public IActionResult AddProduct()
		{
            ViewBag.Category= new SelectList(db.TCategories.ToList(),"CategoryId","Name");
            ViewBag.Deleted= 0;
            int lastProductId = (int)db.TProducts.Max(p => p.ProductId) ;
            ViewBag.NewProductId = lastProductId +1 ;
            var createdBy = HttpContext.Session.GetString("UserNickname") ;
            var product = new TProduct
            {
                CreatedBy = null,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = null,
                LastModifiedDate = DateTime.UtcNow
            };
            return View(product);
		}

        [Route("AddProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProduct(TProduct product)
        {
			
			db.TProducts.Add(product);
			db.SaveChanges();
			return RedirectToAction("ListProduct");           
        }

        









        //từ đây trở xuống là các action sửa

        [Route("UpdateProduct")]
        [HttpGet]

        public IActionResult UpdateProduct(string productId)
        {
            ViewBag.CategoryID = new SelectList(db.TCategories.ToList(), "CategoryId", "Name");
            long change = long.Parse(productId);
            var product = db.TProducts.Find(change);
            product.LastModifiedDate = DateTime.Now;
            product.LastModifiedBy = HttpContext.Session.GetString("UserNickname"); ;
            return View(product);
        }

        [Route("UpdateProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStreet(TProduct product)
        {

            db.TProducts.Update(product);
            db.SaveChanges();
            return RedirectToAction("ListProduct");
        }









        [Route("DeleteProduct")]
        [HttpGet]
        public IActionResult DeleteProduct(string productId)
        {
            long change = long.Parse(productId);
            var product = db.TProducts.Include(p => p.TProductDetails).ThenInclude(pd => pd.TProductDetailCombos)
                                  .FirstOrDefault(p => p.ProductId == change);

            if (product == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm
            }

            // Xóa tất cả các ProductionDetail liên quan
            foreach (var detail in product.TProductDetails)
            {
                // Xóa tất cả các ProductionDetail_Combo liên quan trước
                var detailCombos = db.TProductDetailCombos.Where(dc => dc.ProductDetailId == detail.ProductDetailId);
                db.TProductDetailCombos.RemoveRange(detailCombos);

                // Xóa ProductionDetail
                db.TProductDetails.Remove(detail);
            }

            db.TProducts.Remove(product);
            db.SaveChanges();
            TempData["Message"] = "Xoá thành công sản phẩm";
            return RedirectToAction("ListProduct", "HomeAdmin");


        }

        

    }
}
