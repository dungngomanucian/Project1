using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project1.Models;
using Project1.Models.Authentication;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Azure;
using System.Diagnostics.Eventing.Reader;
using Project1.Areas.Admin.ViewModels;
using NuGet.Protocol;
using Microsoft.AspNetCore.Authorization;
using System.Drawing.Printing;
using System.Collections.Generic;
using X.PagedList.Extensions;


namespace Project1.Areas.Admin.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Area("admin")]
    [Route("admin")]
    [Route("admin/manageproduct")]
    public class ManageProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ManageProductController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        PizzaOnlineContext db = new PizzaOnlineContext();
        //từ đây trở xuống là các action phục vụ cho quản lý sản phẩm
        [Route("ListProduct")]
        public IActionResult ListProduct(int? page)
        {

            int pageSize = 9;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).Include(x => x.TProductDetails).OrderBy(x => x.Deleted);
            PagedList<TProduct> lst = new PagedList<TProduct>(lstProduct, pageNumber, pageSize);
            return View("ListProduct", lst);
        }

        [Route("GetProductDetail")]
        [HttpGet]
        public IActionResult GetProductDetail(int productId)
        {
            // Lấy thông tin chi tiết Order
            var productDetails = (from pr in db.TProducts
                                  join prDe in db.TProductDetails
                                  on pr.ProductId equals prDe.ProductId
                                  where pr.ProductId == productId
                                  select new ProductDetail_ViewModel
                                  {
                                      Size = prDe.Size.Name,
                                      Crust = prDe.Crust.Name,
                                      Price = prDe.Price,
                                      Quantity = prDe.Number, // Lấy số lượng từ OrderDetail  
                                  }).ToList();
            return PartialView("../PartialView/_ProductDetail", productDetails);
        }
        [Route("searchProduct")]
        public IActionResult searchProduct(int? page, string search_Keyword)
        {
            ViewBag.tukhoa = search_Keyword;
            long change = 0;
            if (long.TryParse(search_Keyword, out change))
            {
                int pageSize = 9;
                int pageNumber = page == null || page < 0 ? 1 : page.Value;
                var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).Where(x => x.CategoryId == change).OrderBy(x => x.Name);
                PagedList<TProduct> lst = new PagedList<TProduct>(lstProduct, pageNumber, pageSize);
                return View("searchProduct", lst);
            }
            else
            {
                int pageSize = 9;
                int pageNumber = page == null || page < 0 ? 1 : page.Value;
                var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).Where(x => x.Name.Contains(search_Keyword)).OrderBy(x => x.Name);
                PagedList<TProduct> lst = new PagedList<TProduct>(lstProduct, pageNumber, pageSize);
                return View("searchProduct", lst);
            }


        }

        [Route("AddProduct")]
        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Category = new SelectList(db.TCategories.ToList(), "CategoryId", "Name");
            ViewBag.Sizes = new SelectList(db.TSizes.ToList(), "SizeId", "Name");
            ViewBag.Crusts = new SelectList(db.TCrusts.ToList(), "CrustId", "Name");

            var viewModel = new Product_ProductDetail_ViewModel
            {
                Product = new TProduct(),
                ProductDetail = new TProductDetail()
            };

            return View("AddProduct", viewModel);
        }

        [Route("AddProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProduct(Product_ProductDetail_ViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                long change = long.Parse(HttpContext.Session.GetString("UserId"));
                var user = db.TUsers.FirstOrDefault(x => x.UserId == change);

                if (user != null)
                {
                    // Xử lý sản phẩm
                    int lastProductId = (int)db.TProducts.DefaultIfEmpty().Max(p => (int?)p.ProductId ?? 0);
                    viewModel.Product.ProductId = lastProductId + 1;
                    viewModel.Product.CreatedBy = user.Nickname;
                    viewModel.Product.CreatedDate = DateTime.UtcNow;
                    viewModel.Product.LastModifiedBy = user.Nickname;
                    viewModel.Product.LastModifiedDate = DateTime.UtcNow;
                    viewModel.Product.Deleted = false;
                    int lastProductDetailId = (int)db.TProductDetails.DefaultIfEmpty().Max(p => (int?)p.ProductDetailId ?? 0);
                    viewModel.ProductDetail.ProductDetailId = lastProductDetailId + 1;
                    viewModel.ProductDetail.ProductId = viewModel.Product.ProductId;
                    viewModel.ProductDetail.CreatedBy = user.Nickname;
                    viewModel.ProductDetail.CreatedDate = DateTime.UtcNow;
                    viewModel.ProductDetail.LastModifiedBy = user.Nickname;
                    viewModel.ProductDetail.LastModifiedDate = DateTime.UtcNow;
                    viewModel.ProductDetail.Deleted = false;

                    if (viewModel.Image != null && viewModel.Image.Length > 0)
                    {
                        var fileName = Path.GetFileName(viewModel.Image.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ImagesProduct", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            viewModel.Image.CopyTo(stream);
                        }

                        viewModel.Product.Image = fileName;
                    }

                    // Lưu sản phẩm vào cơ sở dữ liệu
                    db.TProducts.Add(viewModel.Product);
                    db.SaveChanges();

                    // Xử lý chi tiết sản phẩm
                    viewModel.ProductDetail.ProductId = viewModel.Product.ProductId;
                    db.TProductDetails.Add(viewModel.ProductDetail);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Bạn đã thêm sản phẩm mới thành công!";
                    return RedirectToAction("ListProduct","ManageProduct");
                }
            }

            // Nếu có lỗi, trả lại view
            ViewBag.Category = new SelectList(db.TCategories.ToList(), "CategoryId", "Name");
            ViewBag.Sizes = new SelectList(db.TSizes.ToList(), "SizeId", "Name");
            ViewBag.Crusts = new SelectList(db.TCrusts.ToList(), "CrustId", "Name");

            return View("AddProduct", viewModel);
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

            product.Deleted = true;

            db.TProducts.Update(product);
            db.SaveChanges();
            TempData["Message"] = "Xoá thành công sản phẩm";
            return RedirectToAction("ListProduct", "ManageProduct");


        }

        [Route("UpdateProduct")]
        [HttpGet]
        public IActionResult UpdateProduct(string productId)
        {
            ViewBag.CategoryID = new SelectList(db.TCategories.ToList(), "CategoryId", "Name");
            long change = long.Parse(productId);
            var product = db.TProducts.Find(change);
            product.LastModifiedDate = DateTime.Now;
            product.LastModifiedBy = HttpContext.Session.GetString("UserNickname"); ;
            return View("UpdateProduct", product);
        }

        [Route("UpdateProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProduct(TProduct product)
        {
            db.TProducts.Update(product);
            db.SaveChanges();
            return RedirectToAction("ListProduct","ManageProduct");
        }
        //kết thúc các action xử lý sản phẩm
    }
}
