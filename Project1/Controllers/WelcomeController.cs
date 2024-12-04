using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project1.Models;

namespace Project1.Controllers
{
    public class WelcomeController : Controller
    {
        private readonly ILogger<WelcomeController> _logger;

        public WelcomeController(ILogger<WelcomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult WelcomePage()
        {
            // Lấy địa chỉ từ Session và gán vào ViewBag để truyền vào _PartialHeader
            var selectedAddress = HttpContext.Session.GetString("SelectedAddress");
            ViewBag.SelectedAddress = selectedAddress;
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //Đọc địa chỉ
        //Begin
        [HttpPost]
        public IActionResult SaveSelectedAddress([FromBody] AddressModel model)
        {
            if (model != null && !string.IsNullOrEmpty(model.Address))
            {
                // Lưu địa chỉ vào Session
                HttpContext.Session.SetString("SelectedAddress", model.Address);
                return Ok(); // Trả về kết quả thành công
            }

            return BadRequest("Địa chỉ không hợp lệ");
        }

        // Model để nhận dữ liệu từ AJAX
        public class AddressModel
        {
            public string Address { get; set; }
        }
        //End

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
