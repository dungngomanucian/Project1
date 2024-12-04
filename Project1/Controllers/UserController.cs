using Project1.Models;
using Project1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Project1.Services;

namespace Project1.Controllers
{
    public class UserController : Controller
    {
        private readonly PizzaOnlineContext _db;
        private readonly EmailService _emailService;

        public UserController(PizzaOnlineContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Resgister()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Resgister(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maxUserId = _db.TUsers.Max(u => u.UserId);

                var lastUserCode = _db.TUsers
               .Where(u => u.Code.StartsWith("USER"))
               .OrderByDescending(u => u.Code)
               .Select(u => u.Code)
               .FirstOrDefault();

                // Tách phần số ra khỏi Code
                int newCodeNumber = 1;
                if (lastUserCode != null && lastUserCode.Length > 4)
                {
                    int.TryParse(lastUserCode.Substring(4), out newCodeNumber);
                    newCodeNumber++;
                }

                // Tạo Code mới dạng USER + số tăng dần
                var newUserCode = "USER" + newCodeNumber.ToString("D2");

                var user = new TUser
                {
                    UserId = maxUserId + 1,
                    RoleId = 2,
                    Code = newUserCode,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Nickname = model.Nickname,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Address = model.Address,
                    Point = 0,
                    Deleted = false
                };

                _db.TUsers.Add(user);
                _db.SaveChanges(); 

                return RedirectToAction("Index", "Login");
            }

            return View(model);
        }

        // GET: Hiển thị form quên mật khẩu
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Xử lý gửi mail quên mật khẩu
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _db.TUsers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại trong hệ thống!";
                return View();
            }

            try
            {
                string subject = "Khôi phục mật khẩu - Pizza Online";
                string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        .email-container {{
                            max-width: 600px;
                            margin: 0 auto;
                            font-family: Arial, sans-serif;
                            background-color: #ffffff;
                            padding: 20px;
                            border-radius: 10px;
                            box-shadow: 0 0 10px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            text-align: center;
                            padding: 20px 0;
                            border-bottom: 2px solid #f0f0f0;
                        }}
                        .logo {{
                            max-width: 200px;
                            height: auto;
                        }}
                        .content {{
                            padding: 20px 0;
                            line-height: 1.6;
                            color: #333333;
                        }}
                        .password-box {{
                            background-color: #f8f9fa;
                            padding: 15px;
                            margin: 15px 0;
                            border-radius: 5px;
                            border-left: 4px solid #ff6b6b;
                        }}
                        .footer {{
                            text-align: center;
                            padding-top: 20px;
                            border-top: 2px solid #f0f0f0;
                            color: #666666;
                            font-size: 12px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <img src='https://scontent.fhan14-3.fna.fbcdn.net/v/t39.30808-1/308879100_1524090248029489_1362350270885674186_n.jpg?stp=dst-jpg_s200x200&_nc_cat=111&ccb=1-7&_nc_sid=0ecb9b&_nc_eui2=AeGVXSxlD4qYvg4oYProsMSXA750HLRreo4DvnQctGt6jg56k4MrkTYmA2mxYHdcLcS0amCoVhHXCA7YXH50TrJ5&_nc_ohc=oTofn4dILysQ7kNvgFoAvqh&_nc_zt=24&_nc_ht=scontent.fhan14-3.fna&_nc_gid=AqyQcQW8Uo4_nUUtOS-0jJJ&oh=00_AYD_Pe6vIPcqTPfpzLh5mVrRkrhVQ1dUOiDf5PEU5TJGVg&oe=674CA27C' alt='Pizza Online Logo' class='logo'>
                            <h1 style='color: #ff6b6b;'>Sixter's Pizza</h1>
                        </div>
                
                        <div class='content'>
                            <h2>Xin chào {user.Nickname},</h2>
                            <p>Chúng tôi nhận được yêu cầu khôi phục mật khẩu từ bạn.</p>
                    
                            <div class='password-box'>
                                <p>Mật khẩu của bạn là: <strong>{user.Password}</strong></p>
                            </div>
                    
                            <p>Vì lý do bảo mật, chúng tôi khuyến nghị bạn nên:</p>
                            <ul>
                                <li>Đăng nhập ngay sau khi nhận được email này</li>
                                <li>Thay đổi mật khẩu của bạn</li>
                                <li>Không chia sẻ mật khẩu với người khác</li>
                            </ul>
                        </div>
                
                        <div class='footer'>
                            <p>Đây là email tự động, vui lòng không trả lời email này.</p>
                            <p>© 2024 Sister's Pizza . Tất cả các quyền được bảo lưu.</p>
                            <p>Địa chỉ: Số 3 Cầu Giấy, Hà Nội<br>
                            Điện thoại: 0889762488<br>
                            Email: thinhdo0606@gmail.com</p>
                        </div>
                    </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(email, subject, body);

                TempData["Success"] = "Mật khẩu đã được gửi đến email của bạn!";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                // Log the actual error
                TempData["Error"] = $"Có lỗi xảy ra khi gửi email: {ex.Message}";
                return View();
            }
        }
    }
}
