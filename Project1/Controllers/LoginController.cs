using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Project1.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Net.WebSockets;
namespace Project1.Controllers
{
    public class LoginController : Controller
    {
        PizzaOnlineContext db = new PizzaOnlineContext();
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Lấy thông tin từ claims
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

                // Lưu vào session
                if (emailClaim != null)
                    HttpContext.Session.SetString("Email", emailClaim);
                if (userIdClaim != null)
                    HttpContext.Session.SetString("UserId", userIdClaim);

                TempData["Title"] = "Thành công";
                TempData["Content"] = "Bạn đã đăng nhập thành công.";

                // Lấy role từ claims để redirect về đúng trang
                var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                if (roleClaim != null && (roleClaim.Value == "1"|| roleClaim.Value == "3")) // Admin
                {
                    if (roleClaim.Value == "3") { HttpContext.Session.SetString("RoleId", "3"); }
                    if (roleClaim.Value == "1") { HttpContext.Session.SetString("RoleId", "1"); }
                    return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                }
                else // User thường
                {
                    if (userIdClaim != null)
                    {
                        return RedirectToAction("Index", "Menu", new { userId = userIdClaim });
                    }
                }
                return RedirectToAction("Index", "Menu");
            }

            if (HttpContext.Session.GetString("Email") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(TUser user)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                var u = db.TUsers.FirstOrDefault(x => x.Email.Equals(user.Email));

                if (u != null)
                {
                    if (u.GoogleId != null)
                    {
                        TempData["Title"] = "Thất bại";
                        TempData["Content"] = "Email này đã được sử dụng cho tài khoản Google!"; 
                        TempData["Type"] = "Error";
                        return View();
                    }

                    // Kiểm tra password
                    if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(u.Password))
                    {
                        TempData["Title"] = "Thất bại";
                        TempData["Content"] = "Mật khẩu không hợp lệ.";
                        TempData["Type"] = "Error";
                        return View();
                    }

                    if (BCrypt.Net.BCrypt.Verify(user.Password, u.Password))
                    {
                        HttpContext.Session.SetString("Email", u.Email.ToString());
                        HttpContext.Session.SetString("UserId", u.UserId.ToString());
                        ViewData["UserId"] = this.User;
                        TempData["Title"] = "Thành công";
                        TempData["Content"] = "Đăng nhập thành công.";
                        TempData["Type"] = "Success";
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, u.Email),
                            new Claim(ClaimTypes.Role, u.RoleId.ToString()),
                            new Claim("UserId", u.UserId.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var clainsPricipal = new ClaimsPrincipal(claimsIdentity);

                        /*AuthenticationProperties authProperties = new AuthenticationProperties();*/

                        /* if (user.RememberMe)
                         {
                             authProperties.IsPersistent = true;
                             authProperties.ExpiresUtc = DateTime.Now.AddDays(2);
                         }

                         await HttpContext.SignInAsync(clainsPricipal, authProperties);*/

                        // Cấu hình cookie authentication
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = user.RememberMe, // Kiểm tra null
                            ExpiresUtc = user.RememberMe == true
                                ? DateTimeOffset.UtcNow.AddDays(7)  // Nếu remember me = true, cookie tồn tại 7 ngày
                                : DateTimeOffset.UtcNow.AddMinutes(15) // Nếu không, cookie tồn tại 30 phút
                        };

                        // Sign in
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties
                        );

                        if (u.RoleId == 1 || u.RoleId == 3)
                        {
                            HttpContext.Session.SetString("UserNickname", u.Email.ToString());
                            HttpContext.Session.SetString("RoleId", u.RoleId.ToString());
                            return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Menu", new { userId = u.UserId });
                        }

                    }
                    else
                    {
                        TempData["Title"] = "Thất bại";
                        TempData["Content"] = "Email hoặc mật khẩu không đúng.";
                        TempData["Type"] = "Error";
                        return RedirectToAction("Index", "Login");
                    }
                }
                else
                {
                    TempData["Title"] = "Thất bại";
                    TempData["Content"] = "Email hoặc mật khẩu không đúng.";
                    TempData["Type"] = "Error";
                    if (!ModelState.IsValid)
                    {
                        return View(user);
                    }
                    return RedirectToAction("Index", "Login");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            // Kiểm tra nếu có bất kỳ lỗi nào hoặc người dùng từ chối đăng nhập
            var error = Request.Query["error"].ToString();
            if (!string.IsNullOrEmpty(error) && error.Equals("access_denied", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Login");
            }

            // Kiểm tra xem có truy vấn "error" khác hay không và điều hướng về login
            if (Request.Query.ContainsKey("error"))
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result?.Principal == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy thông tin từ Google claims
            var emailClaim = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var nameClaim = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Google Account ID


            // Kiểm tra xem user với Google ID này đã tồn tại chưa
            var existingUser = db.TUsers.FirstOrDefault(u => u.Email == emailClaim && u.IsGoogleAccount == true);

            if (existingUser != null)
            {
                // User đã tồn tại, set session và đăng nhập
                HttpContext.Session.SetString("Email", existingUser.Email);
                HttpContext.Session.SetString("UserId", existingUser.UserId.ToString());
                HttpContext.Session.SetString("Name", existingUser.Nickname ?? nameClaim);
                HttpContext.Session.SetString("GoogleID", existingUser.GoogleId ?? googleId);
            }
            else
            {
                var maxUserId = db.TUsers.Max(u => u.UserId);
                var lastUserCode = db.TUsers
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
                // Tạo user mới
                var newUser = new TUser
                {
                    UserId = maxUserId + 1,
                    Code = newUserCode,
                    Email = emailClaim,
                    Nickname = nameClaim,
                    GoogleId = googleId,
                    IsGoogleAccount = true,
                    RoleId = 2, // Set role mặc định (2 = user thường)
                    CreatedDate = DateTime.Now
                    // Thêm các trường khác nếu cần
                };

                db.TUsers.Add(newUser);
                await db.SaveChangesAsync();

                // Set session cho user mới
                HttpContext.Session.SetString("Email", newUser.Email);
                HttpContext.Session.SetString("UserId", newUser.UserId.ToString());
                HttpContext.Session.SetString("Name", newUser.Nickname);
                HttpContext.Session.SetString("GoogleID", newUser.GoogleId);
            }

            TempData["Title"] = "Thành công";
            TempData["Content"] = "Đăng nhập Google thành công.";
            TempData["Type"] = "Success";
            return RedirectToAction("Index", "Menu");
        }
        public IActionResult LogoutUsual()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Email");
            return RedirectToAction("Index", "Menu");
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Menu");
        }
    }
}
