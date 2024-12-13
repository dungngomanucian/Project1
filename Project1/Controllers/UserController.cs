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
                var existingUser = _db.TUsers.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    // Nếu có, kiểm tra Google_ID
                    if (existingUser.GoogleId == null)
                    {
                        TempData["Title"] = "Thất bại";
                        TempData["Content"] = "Email đã tồn tại trong hệ thống! Vui lòng đăng kí bằng tài khoản email khác";
                        TempData["Type"] = "Error";
                        return View(model);
                    }
                    else
                    {
                        // Nếu tài khoản đã tồn tại và GoogleId != null
                        TempData["Title"] = "Thất bại";
                        TempData["Content"] = "Email này đã được liên kết với Google. Vui lòng đăng nhập bằng Google!";
                        TempData["Type"] = "Error";
                        return View(model);
                    }
                }
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
                    IsGoogleAccount = false,
                    Deleted = false
                };

                _db.TUsers.Add(user);
                _db.SaveChanges();

                TempData["Title"] = "Đăng ký thành công!";
                TempData["Content"] = $"Chào mừng {user.Nickname} đến với Sixter's Pizza. Vui lòng đăng nhập để tiếp tục.";
                TempData["Type"] = "Success";
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
                string resetToken = Guid.NewGuid().ToString();
                user.ResetToken = resetToken;
                user.ResetTokenExpiry = DateTime.Now.AddHours(24);
                _db.SaveChanges();

                string resetLink = Url.Action("ResetPassword", "User",
                    new { token = resetToken }, Request.Scheme);

                string subject = "Đặt lại mật khẩu - Pizza Online";
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
                             <img src='https://scontent.fhan5-11.fna.fbcdn.net/v/t39.30808-6/469167161_2056725571432618_6103467900661450282_n.jpg?stp=dst-jpg_p526x296_tt6&_nc_cat=103&ccb=1-7&_nc_sid=127cfc&_nc_eui2=AeEBvVd9kr3vn8xYHw1wsiao-6CDmnBRdR_7oIOacFF1H1OvlxGgAJ4bCHJfG8SDG-YXgJeK3KBlLHDYQdFZdReo&_nc_ohc=NNz-vs4Q4fEQ7kNvgGI96Os&_nc_zt=23&_nc_ht=scontent.fhan5-11.fna&_nc_gid=AaVLUEkWylrSwLyOmzxJCmc&oh=00_AYAFUfBRH9__bBcI40dJj4CT5TlP6kKdmgIK9N3Iu6wdTA&oe=6761956C' style='border-radius: 50%; width: 130px; height: 130px; object-fit: cover;' alt='Pizza Online Logo' class='logo'>
                            <h1 style='color: #ff6b6b;'>Sixter's Pizza</h1>
                        </div>
            
                        <div class='content'>
                            <h2>Xin chào {user.LastName} {user.FirstName},</h2>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu từ bạn.</p>
                
                            <div class='password-box'>
                                <p>Vui lòng click vào link dưới đây để đặt lại mật khẩu:</p>
                                <p><a href='{resetLink}' style='background-color: #ff6b6b; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Đặt lại mật khẩu</a></p>
                                <p>Link này sẽ hết hạn sau 24 giờ.</p>
                            </div>
                
                            <p><strong>Lưu ý:</strong> Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
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

                TempData["Title"] = "Thành công";
                TempData["Content"] = "Link đặt lại mật khẩu đã được gửi đến email của bạn!";
                TempData["Type"] = "Success";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                TempData["Title"] = "Thất bại";
                TempData["Content"] = $"Có lỗi xảy ra khi gửi email: {ex.Message}";
                TempData["Type"] = "Error";
                return View();
            }
        }
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var user = _db.TUsers.FirstOrDefault(u => u.ResetToken == token
                && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                TempData["Title"] = "Thất bại";
                TempData["Content"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn!";
                TempData["Type"] = "Error";
                return RedirectToAction("Index", "Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _db.TUsers.FirstOrDefault(u =>
                u.ResetToken == model.Token &&
                u.ResetTokenExpiry > DateTime.Now);

            if (user == null)
            {
                TempData["Title"] = "Thất bại";
                TempData["Content"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn!";
                TempData["Type"] = "Error";
                return RedirectToAction("Index", "Login");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            _db.SaveChanges();

            TempData["Title"] = "Thành công";
            TempData["Content"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.";
            TempData["Type"] = "Success";
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult GetUserProfile()
        {
            try
            {
                var userIdString = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng trong session" });
                }

                // Chuyển đổi string sang int
                if (!int.TryParse(userIdString, out int userId))
                {
                    return Json(new { success = false, message = "UserId không hợp lệ" });
                }

                var user = _db.TUsers.FirstOrDefault(u => u.UserId == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                var viewModel = new UserProfileViewModel
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Nickname = user.Nickname ?? "",
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Address = user.Address ?? ""
                };

                return Json(new
                {
                    success = true,
                    data = viewModel,
                    message = "Lấy thông tin người dùng thành công"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserProfile: {ex.Message}"); // Debug log
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateProfile([FromBody] UserProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var user = _db.TUsers.Find(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Cập nhật thông tin
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Nickname = model.Nickname;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;

                _db.TUsers.Update(user);
                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật thông tin thành công",
                    data = new
                    {
                        userId = user.UserId,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        nickname = user.Nickname,
                        email = user.Email,
                        phoneNumber = user.PhoneNumber,
                        address = user.Address
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateProfile: {ex.Message}"); // Debug log
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin mật khẩu." });
            }
            if (model.NewPassword.Length < 6)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu mới và xác nhận mật khẩu không trùng khớp." });
            }

            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng trong session" });
            }

            // Chuyển đổi string sang int
            if (!int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "UserId không hợp lệ" });
            }

            var user = _db.TUsers.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _db.SaveChanges();

            return Json(new { success = true, message = "Đổi mật khẩu thành công" });
        }
    }
}
