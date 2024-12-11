using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project1.DTO;
using Project1.Models;
using Project1.Services;
using Project1.ViewModels;
using System.Drawing;
using System.Net.WebSockets;
using X.PagedList;

namespace Project1.Controllers
{
    public class OrderController : Controller
    {
        private PizzaOnlineContext db = new PizzaOnlineContext();
        public IActionResult Index(int ?page)
        {
            // Lấy địa chỉ từ Session
            var selectedAddress = HttpContext.Session.GetString("SelectedAddress");

            // Truyền giá trị vào ViewBag để hiển thị trong view nếu cần
            ViewBag.SelectedAddress = selectedAddress;

            int pageSize = 6;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            if (!long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
            {
                return NotFound();              
            }
            var lstOrder = db.TOrders
                .Include(o => o.Status)
                .Include(o => o.Voucher)
                .Include(o => o.PaymentMethod)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.Date);
            PagedList<TOrder> lst = new PagedList<TOrder>(lstOrder, pageNumber, pageSize);
            return View(lst);
        }

        public IActionResult OrderDetail(long? orderId)
        {
            // Lấy địa chỉ từ Session
            var selectedAddress = HttpContext.Session.GetString("SelectedAddress");

            // Truyền giá trị vào ViewBag để hiển thị trong view nếu cần
            ViewBag.SelectedAddress = selectedAddress;

            var order = db.TOrders
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Status)
                .Include(o => o.Voucher)
                .Include(o => o.TOrderDetails)
                    .ThenInclude(od => od.ProductDetail)
                        .ThenInclude(pd => pd.Size)
                .Include(o => o.TOrderDetails)
                    .ThenInclude(od => od.ProductDetail)
                        .ThenInclude(pd => pd.Crust)
                .Include(o => o.TOrderDetails)
                    .ThenInclude(od => od.ProductDetail)
                        .ThenInclude(pd => pd.Product)
                            .ThenInclude(p => p != null ? p.Category : null)
                .SingleOrDefault(o => o.OrderId == orderId);
                 
            return View(order);
        }

        private readonly PaypalClient _paypalClient;

        public OrderController(PizzaOnlineContext context, PaypalClient paypalClient)
        {
            db = context;
            _paypalClient = paypalClient;
        }

        [Authorize]
        [HttpPost("/Order/RefundOrder")]
        public async Task<IActionResult> RefundOrder(string captureId, string amount, string currency = "USD")
        {
            if (string.IsNullOrEmpty(captureId) || string.IsNullOrEmpty(amount))
            {
                return BadRequest(new { Message = "Thông tin không hợp lệ." });
            }

            try
            {
                // Gọi RefundPayment từ PaypalClient
                var refundResponse = await _paypalClient.RefundPayment(captureId, amount, currency);

                if (refundResponse.status == "COMPLETED")
                {
                    // Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu
                    var order = db.TOrders.FirstOrDefault(o => o.Code == captureId);
                    if (order != null)
                    {
                        order.StatusId = 6; // Cập nhật trạng thái hủy
                        order.LastModifiedDate = DateTime.UtcNow;
                        db.SaveChanges();
                    }

                    return Ok(new
                    {
                        Message = "Hoàn tiền thành công.",
                        RefundId = refundResponse.id,
                        RefundAmount = refundResponse.amount.value
                    });
                }
                else
                {
                    return BadRequest(new { Message = "Hoàn tiền thất bại.", Status = refundResponse.status });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi thực hiện hoàn tiền.", Error = ex.Message });
            }
        }

    }
}
