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
    [Route("admin/manageorder")]
    public class ManageOrderController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ManageOrderController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        PizzaOnlineContext db = new PizzaOnlineContext();
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

        [Route("Preparing")]
        public IActionResult Preparing(int orderId)
        {
            var order = db.TOrders.Where(x => x.OrderId == orderId).FirstOrDefault();
            if (order != null)
            {
                order.StatusId = 3; // Đặt trạng thái thành "Đang làm"
                db.TOrders.Update(order);
                db.SaveChanges();
            }

            var orders = db.TOrders
                              .AsNoTracking()
                              .Where(x => x.StatusId == 3)
                              .OrderBy(x => x.OrderId)
                              .ToList();

            // Trả về view `ListUnAcceptOrder` cùng dữ liệu danh sách chưa xác nhận
            return View("ListUnAcceptOrder", orders);
        }

        [Route("Delivering")]
        public IActionResult Delivering(int orderId)
        {
            var order = db.TOrders.Where(x => x.OrderId == orderId).FirstOrDefault();
            if (order != null)
            {
                order.StatusId = 4; // Đặt trạng thái thành "Đang giao hàng"
                db.TOrders.Update(order);
                db.SaveChanges();
            }
            var orders = db.TOrders
                  .AsNoTracking()
                  .Where(x => x.StatusId == 3)
                  .OrderBy(x => x.OrderId)
                  .ToList();

            // Trả về view `ListUnAcceptOrder` cùng dữ liệu danh sách chưa xác nhận
            return View("ListUnAcceptOrder", orders);
        }

        [Route("Finish")]
        public IActionResult Finish(int orderId)
        {
            var order = db.TOrders.Where(x => x.OrderId == orderId).FirstOrDefault();
            if (order != null)
            {
                order.StatusId = 5; // Đặt trạng thái thành "Giao hàng thành công"
                db.TOrders.Update(order);
                db.SaveChanges();
            }
            var orders = db.TOrders
                  .AsNoTracking()
                  .Where(x => x.StatusId == 4)
                  .OrderBy(x => x.OrderId)
                  .ToList();

            // Trả về view `ListUnAcceptOrder` cùng dữ liệu danh sách chưa xác nhận
            return View("ListUnAcceptOrder", orders);
        }
        //action trả về view hiển thị danh sách các hoá đơn mới
        [Route("ListUnAcceptOrder")]
        public IActionResult ListUnAcceptOrder( )
        {
            return View("ListUnAcceptOrder");
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
            return PartialView("../PartialView/_OrderListbyState", orders);
        }

        //action với mục đích show form hiển thị chi tiết đơn hàng
        [Route("GetOrderDetail")]
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

            return PartialView("../PartialView/_OrderDetail", viewModel);
        }

        //action với mục đích show bảng liệt kê các đơn hàng cũ có trạng thái: đã giao hoặc huỷ
        [Route("ListOldOrder")]
        public IActionResult ListOldOrder(int? page)
        {

            int pageSize = 9;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listOOrder = db.TOrders.AsNoTracking().Include(x => x.Status).Include(x => x.PaymentMethod).Include(x => x.Manager).Where(x => x.StatusId == 5 || x.StatusId==6).OrderBy(x => x.CreatedDate).ToList();
            PagedList<TOrder> lst = new PagedList<TOrder>(listOOrder, pageNumber, pageSize);
            return View("ListOldOrder", lst);
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
                return PartialView("../PartialView/_OrderListAfterSearch", lst);

                
            }
            else //Từ khoá không là 1 số
            {
                ViewBag.saidinhdang = "Mã hoá đơn nhập vào của bạn không là một số!";
                return PartialView("../PartialView/_OrderListAfterSearch");
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
            return PartialView("../PartialView/_OrderListAfterFilter", lst);
        }

        [Route("CancelOrder")]
        public IActionResult CancelOrder(int orderId)
        {
            var order = db.TOrders.Where(X => X.OrderId == orderId).FirstOrDefault();
            order.Deleted = true;
            order.StatusId = 6;
            db.TOrders.Update(order);
            db.SaveChanges();
            return RedirectToAction("ListUnAcceptOrder","ManageOrder"); 
        }
        //kết thúc các action xử lý hoá đơn
    }
}
