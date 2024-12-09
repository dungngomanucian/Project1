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

        PizzaOnlineContext db = new PizzaOnlineContext();
        [Authentication]
        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {

            return View();
        }

        //từ đây trở xuống là các action phục vụ cho quản lý sản phẩm
        [Route("ListProduct")]
        public IActionResult ListProduct(int? page)
        {

            int pageSize = 9;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).OrderBy(x => x.Deleted);
            PagedList<TProduct> lst = new PagedList<TProduct>(lstProduct, pageNumber, pageSize);
            return View(lst);
        }

        /*  [Route("ListProductOptimizeByCategory")]
          public IActionResult ListProductOptimizeByCategory(int? page,string categoryId)
          {
              long change = long.Parse( categoryId);
              int pageSize = 15;
              int pageNumber = page == null || page < 0 ? 1 : page.Value;
              var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).Where(x=>x.CategoryId==change).OrderBy(x => x.Name);
              PagedList<TProduct> lst = new PagedList<TProduct>(lstProduct, pageNumber, pageSize);
              ViewBag.CurrentCategoryId = categoryId;
              return View(lst);
          }*/
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
                return View(lst);
            }
            else
            {
                int pageSize = 9;
                int pageNumber = page == null || page < 0 ? 1 : page.Value;
                var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).Where(x => x.Name.Contains(search_Keyword)).OrderBy(x => x.Name);
                PagedList<TProduct> lst = new PagedList<TProduct>(lstProduct, pageNumber, pageSize);
                return View(lst);
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

            return View(viewModel);
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
                    return RedirectToAction("ListProduct");
                }
            }

            // Nếu có lỗi, trả lại view
            ViewBag.Category = new SelectList(db.TCategories.ToList(), "CategoryId", "Name");
            ViewBag.Sizes = new SelectList(db.TSizes.ToList(), "SizeId", "Name");
            ViewBag.Crusts = new SelectList(db.TCrusts.ToList(), "CrustId", "Name");

            return View(viewModel);
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
            return RedirectToAction("ListProduct", "HomeAdmin");


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
            return View(product);
        }

        [Route("UpdateProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProduct(TProduct product)
        {
            db.TProducts.Update(product);
            db.SaveChanges();
            return RedirectToAction("ListProduct");
        }
        //kết thúc các action xử lý sản phẩm





        //từ đây trở xuống là các action xử lý hoá đơn
        [Route("Accept")]
        public IActionResult Accept(int orderId)
        {
            var order = db.TOrders.Where(x => x.OrderId == orderId).FirstOrDefault();
            if (order != null)
            {
                order.StatusId = 2; // Đặt trạng thái thành "Đã xác nhận"
                db.TOrders.Update(order);
                db.SaveChanges();
            }
            var orders = db.TOrders
                  .AsNoTracking()
                  .Where(x => x.StatusId == 1)
                  .OrderBy(x => x.OrderId)
                  .ToList();

            // Trả về view `ListUnAcceptOrder` cùng dữ liệu danh sách chưa xác nhận
            return View("ListUnAcceptOrder", orders);
        }

        //action trả về view hiển thị danh sách các hoá đơn mới
        [Route("ListUnAcceptOrder")]
        public IActionResult ListUnAcceptOrder()
        {
            return View();
        }

        //action trả về partialview _orderlist, dùng để hiển thị các hoá đơn theo trạng thái
        [Route("ListOrderCategory")]
        [HttpGet]
        public IActionResult ListOrderCategory(int? statusId)
        {
            // Nếu statusId null, lấy tất cả hoá đơn
            var orders = db.TOrders
                                 .AsNoTracking()
                                 .Include(x => x.Status)  // Bao gồm bảng Status để lấy tên trạng thái
                                 .Where(x => !statusId.HasValue || x.StatusId == statusId.Value)  // Lọc theo status_id
                                 .OrderBy(x => x.OrderId)
                                 .ToList();

            // Trả về view với danh sách hoá đơn đã lọc
            return PartialView("PartialView/_OrderList", orders);
        }

        //action với mục đích show form hiển thị chi tiết đơn hàng
        [Route("GetOrderDetail")]
        [Route("Admin/HomeAdmin/GetOrderDetail")]
        [HttpGet]
        public IActionResult GetOrderDetail(int orderId)
        {
            var order = db.TOrders.Include(x => x.Voucher).Where(od => od.OrderId == orderId).FirstOrDefault();
            double? total_price = order.TotalPrice;
            // Lấy thông tin chi tiết Order
            var orderDetails = (from odd in db.TOrderDetails
                                join pd in db.TProductDetails on odd.ProductDetailId equals pd.ProductDetailId
                                join p in db.TProducts on pd.ProductId equals p.ProductId
                                join ca in db.TCategories on p.CategoryId equals ca.CategoryId
                                join s in db.TSizes on pd.SizeId equals s.SizeId
                                join c in db.TCrusts on pd.CrustId equals c.CrustId
                                where odd.OrderId == orderId
                                select new ProductDetail_ViewModel
                                {
                                    CategoryName = ca.Name,
                                    ProductDetailId = pd.ProductDetailId,
                                    ProductName = p.Name, // Lấy tên sản phẩm từ bảng Product
                                    Size = s.Name,
                                    Crust = c.Name,
                                    Price = pd.Price,
                                    Quantity = odd.Number, // Lấy số lượng từ OrderDetail
                                    ProductImage = p.Image // Lấy ảnh sản phẩm từ bảng Product
                                }).ToList();



            // Lấy thông tin Combo (nếu có)
            var orderCombo = db.TOrderCombos
                .Include(oc => oc.Combo)
                .FirstOrDefault(oc => oc.OrderId == orderId);

            // Tạo ViewModel
            var viewModel = new OrderDetail_ViewModel
            {
                SelectedProducts = orderDetails,
                Order = order,
                OCombo = orderCombo,
                TotalPriceAfterUseVoucher = total_price
            };

            return PartialView("PartialView/_OrderDetail", viewModel);
        }


        [Route("ListOldOrder")]
        public IActionResult ListOldOrder(int? page)
        {

            int pageSize = 9;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listOOrder = db.TOrders.AsNoTracking().Include(x => x.Status).Include(x => x.PaymentMethod).Include(x => x.Manager).Where(x => x.StatusId == 5).ToList();
            PagedList<TOrder> lst = new PagedList<TOrder>(listOOrder, pageNumber, pageSize);
            return View(lst);
        }
        [Route("searchOrder")]
        public IActionResult searchOrder(int? page, string search_Keyword)
        {
            ViewBag.tukhoa = search_Keyword;
            long change = 0;
            if (long.TryParse(search_Keyword, out change))
            {
                int pageSize = 9;
                int pageNumber = page == null || page < 0 ? 1 : page.Value;
                var lstOrder = db.TOrders.AsNoTracking().Where(x => x.OrderId == change).ToList();
                PagedList<TOrder> lst = new PagedList<TOrder>(lstOrder, pageNumber, pageSize);
                return View(lst);
            }
            else
            {
                return View();
                ViewBag.saidinhdang = "Mã hoá đơn nhập vào của bạn không là một số!";
            }


        }
        //kết thúc các action xử lý hoá đơn
    }
}
