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
			int pageNumber =page ==null || page<0 ? 1:page.Value;	
			var lstProduct = db.TProducts.AsNoTracking().Include(x=>x.Category).OrderBy(x=>x.Deleted);
			PagedList<TProduct> lst= new PagedList<TProduct>(lstProduct,pageNumber,pageSize);
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
            if(long.TryParse(search_Keyword, out change))
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
                var lstProduct = db.TProducts.AsNoTracking().Include(x => x.Category).Where(x => x.Name.Contains( search_Keyword)).OrderBy(x => x.Name);
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
                    viewModel.ProductDetail.ProductDetailId= lastProductDetailId+1;
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
        public IActionResult ListUnAcceptOrder( )
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
            return PartialView("PartialView/_OrderListbyState", orders);
        }

        //action với mục đích show form hiển thị chi tiết đơn hàng
        [Route("GetOrderDetail")]
        [Route("Admin/HomeAdmin/GetOrderDetail")]
        [HttpGet]
        public IActionResult GetOrderDetail(int orderId)
        {
            var order= db.TOrders.Include(x=>x.Voucher).Where(od=>od.OrderId == orderId).FirstOrDefault();
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
                                    Size=s.Name,
                                    Crust=c.Name,
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
                Order=order,
                OCombo = orderCombo,
                TotalPriceAfterUseVoucher=total_price
            };

            return PartialView("PartialView/_OrderDetail", viewModel);
        }

        //action với mục đích show bảng liệt kê các đơn hàng cũ có trạng thái: đã giao hoặc huỷ
        [Route("ListOldOrder")]
        public IActionResult ListOldOrder(int? page)
        {

            int pageSize = 9;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listOOrder = db.TOrders.AsNoTracking().Include(x => x.Status).Include(x => x.PaymentMethod).Include(x => x.Manager).Where(x => x.StatusId == 5 || x.StatusId==6).OrderBy(x => x.CreatedDate).ToList();
            PagedList<TOrder> lst = new PagedList<TOrder>(listOOrder, pageNumber, pageSize);
            return View(lst);
        }

        //action với mục đích tạo partialview điền vào phần chứa nội dung của view ListOldOrder
        [Route("searchOrder")]
        public IActionResult searchOrder(int? page, string search_Keyword)
        {
            ViewBag.searchKeyword = search_Keyword;
            long change = 0;

            // Kiểm tra nếu từ khóa là một số
            if (long.TryParse(search_Keyword, out change))
            {
                var lstOrder = db.TOrders
                                 .AsNoTracking().Include(x=>x.Status).Include(x=>x.PaymentMethod)
                                 .Where(x => x.OrderId == change && (x.StatusId == 5 || x.StatusId == 6))
                                 .ToList();

                if (lstOrder.Count() == 0) // Không tìm thấy
                {
                    ViewBag.NoResult = "Hoá đơn bạn tìm kiếm không có trong dữ liệu.";
                }
                int pageSize = 1;
                int pageNumber = page == null || page < 0 ? 1 : page.Value;
                PagedList<TOrder> lst = new PagedList<TOrder>(lstOrder, pageNumber, pageSize);
                return PartialView("PartialView/_OrderListAfterSearch", lst);

                
            }
            else //Từ khoá không là 1 số
            {
                ViewBag.saidinhdang = "Mã hoá đơn nhập vào của bạn không là một số!";
                return PartialView("PartialView/_OrderListAfterSearch");
            }
        }

        [Route("FilterOrder")]
        [HttpGet]
        public IActionResult FilterOrder(string filterType, string filterValue, int? page)
        {
            // Bắt đầu query từ bảng TOrder
            var orders = db.TOrders
                .Include(o => o.PaymentMethod) // Include để lấy thông tin phương thức thanh toán
                .Include(o => o.Status).
                Where(o=>o.StatusId==5 || o.StatusId==6)// Include để lấy thông tin trạng thái
                .AsQueryable();

            // Lọc theo filterType và filterValue
            if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(filterValue))
            {
                if (filterType == "paymentMethod")
                {
                    orders = orders.Where(o => o.PaymentMethod.Name == filterValue);
                }
                else if (filterType == "status")
                {
                    long change=long.Parse(filterValue);
                    orders = orders.Where(o => o.StatusId == change);
                }
            }

            // Phân trang
            int pageSize = 8; // Số hóa đơn trên mỗi trang
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            PagedList<TOrder> lst = new PagedList<TOrder>(orders, pageNumber, pageSize);
            

            // Ghi nhận thông tin lọc vào ViewBag để phân trang giữ được trạng thái
            ViewBag.FilterType = filterType;
            ViewBag.FilterValue = filterValue;

            // Trả về PartialView với danh sách hóa đơn được lọc
            return PartialView("PartialView/_OrderListAfterFilter", lst);
        }
        //kết thúc các action xử lý hoá đơn



        //từ đây trở xuống là các action xử lý voucher
        [Route("ListVoucher")]
        public IActionResult ListVoucher(int state)
        {
            if( state == 1)
            {
                var listVoucher = db.TVouchers.AsNoTracking().Where(x => x.Deleted == false && x.Number > 0).ToList();
                ViewBag.Null = "List thành công";
                return View(listVoucher);
            }
            else if (state==2)
            {

                var listVoucher = db.TVouchers.AsNoTracking().Where(x => x.Deleted == true || x.Number == 0).ToList();
                if(listVoucher.Count()==0) { ViewBag.Null = "Không có voucher nào bị hết hạn hoặc hết số lượng"; }
                return View(listVoucher);
            }
            else
            {
                return View();
            }
        }

        [Route("AddVoucher")]
        [HttpGet]
        public IActionResult AddVoucher()
        {
            
            return View();
        }

        [Route("AddVoucher")]
        [HttpPost]
        public IActionResult AddVoucher(TVoucher voucher)
        {
            long change = long.Parse(HttpContext.Session.GetString("UserId"));
            var user = db.TUsers.FirstOrDefault(x => x.UserId == change);
            int lastVoucherId = (int)db.TVouchers.DefaultIfEmpty().Max(p => (int?)p.VoucherId ?? 0);
            voucher.VoucherId = lastVoucherId + 1;
            voucher.CreatedBy = user.Nickname;
            voucher.CreatedDate = DateTime.UtcNow;
            voucher.LastModifiedBy = user.Nickname;
            voucher.LastModifiedDate = DateTime.UtcNow;
            voucher.Deleted = false;
            if (voucher != null)
            {
                db.TVouchers.Add(voucher);
                db.SaveChanges();
                return RedirectToAction("ListVoucher");
            }
            return View(voucher);
        }
        //kết thúc các action xử lý voucher

        //từ đây trở xuống là các action xử lý combo
        //kết thúc các action xử lý combo

        //từ đây trở xuống là các action xử lý user
        //kết thúc các action xử lý user
    }
}
