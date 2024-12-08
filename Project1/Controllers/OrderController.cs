using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project1.DTO;
using Project1.Models;
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
    }
}
