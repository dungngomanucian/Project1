using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Project1.Models;

namespace Project1.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private PizzaOnlineContext db = new PizzaOnlineContext();

        [HttpPost("UpdateCustomerFeeling")]
        public IActionResult UpdateCustomerFeeling([FromQuery] long? id, [FromBody] string? customerFeeling)
        {
            if(id == null || string.IsNullOrWhiteSpace(customerFeeling))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Thông tin không hợp lệ"
                });
            }
            var order = db.TOrders.FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Đơn hàng không tồn tại"
                });
            }
            order.CustomerFeeling = customerFeeling;
            db.SaveChanges();
            return Ok(new
            {
                success = true,
                message = "Cảm nhận đã được cập nhật thành công",
                orderId = id
            });
        }

        [HttpPost("CancelOrder")]
        public IActionResult CancelOrder([FromQuery]long id)
        {
            var order = db.TOrders.FirstOrDefault(o => o.OrderId == id);
            if (order == null) return NotFound();           
            if(order.StatusId >= 2)
            {
                if (order.StatusId == 6)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Đơn hàng đã được hủy trước đó"
                    });
                }
                return Ok(new
                {
                    success = false,
                    message = "Bạn chỉ có thể hủy đơn hàng chưa được xác nhận"
                });
            }
            order.StatusId = 6;
            db.SaveChanges();
            return Ok(new
            {
                success = true,
                message = "Bạn đã hủy đơn hàng thành công"
            });
        }
    }
}
