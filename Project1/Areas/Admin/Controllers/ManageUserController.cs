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
    [Route("admin/manageuser")]
    public class ManageUserController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ManageUserController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        PizzaOnlineContext db = new PizzaOnlineContext();

        [Route("ManageAdmin")]
        public IActionResult ManageAdmin()
        {
            var lstAdmin = db.TUsers.AsNoTracking().Where(x => (x.RoleId == 1 || x.RoleId == 3) && x.Deleted==false).ToList();
            var viewmodel = new Email_Password_forAdmin
            {
                listAdmin=lstAdmin
            };
            return View("ManageAdmin", viewmodel);
        }


        [Route("AddAdmin")]
        [HttpPost]
        public IActionResult AddAdmin(Email_Password_forAdmin viewmodel)
        {
            var check_user = db.TUsers.Where(x => x.Email == viewmodel.Email).FirstOrDefault();
            if (check_user!=null &&  (check_user.RoleId==1 || check_user.RoleId == 3)) { TempData["TrungEmail"]="Email đã được sử dụng cho 1 tài khoản admin"; return RedirectToAction("ManageAdmin",viewmodel); }
            int lastUserId = (int)db.TUsers.DefaultIfEmpty().Max(p => (int?)p.UserId ?? 0);
            string password = BCrypt.Net.BCrypt.HashPassword(viewmodel.Password);
            var admin_new_account = new TUser
            {
                UserId = lastUserId + 1,
                RoleId = 1,
                Deleted = false,
                Email = viewmodel.Email,
                Password = password
            };
            db.TUsers.Add(admin_new_account);
            db.SaveChanges();
            var lstAdmin = db.TUsers.AsNoTracking().Where(x => (x.RoleId == 1 || x.RoleId == 3)).ToList();
            var model = new Email_Password_forAdmin
            {
                listAdmin = lstAdmin
            };
            TempData["AddAdminSuccess"] = "Thêm admin thành công";
            return RedirectToAction("ManageAdmin",model);
        }

        [Route("GrantSuperAdmin")]
        [HttpPost]
        public IActionResult GrantSuperAdmin(int adminId)
        {
            if (int.Parse(HttpContext.Session.GetString("RoleId")) == 3)
            {
                
                var superadmin = db.TUsers.FirstOrDefault(a => a.RoleId == 3);
                if (superadmin != null)
                {
                    superadmin.RoleId = 1;
                    db.TUsers.Update(superadmin);
                    db.SaveChanges();
                }
                var admin = db.TUsers.Where(a => a.UserId == adminId).FirstOrDefault();
                if (admin != null)
                {
                    admin.RoleId = 3; // Cập nhật thành Super Admin
                    db.TUsers.Update(admin);
                    db.SaveChanges();
                }

                //TempData["SuccessMessage"] = "Cấp quyền Super Admin thành công!";
                return Json(new { success = true, message = "Cấp quyền Super Admin thành công!" });
            }
            else
            {
                //TempData["WrongRole"]= "Không có quyền hạn để thực hiện thao tác này";
                return Json(new { success = false, message = "Không có quyền hạn để thực hiện thao tác này" });
            }
        }

        [Route("DeleteAdmin")]
        [HttpPost]
        public IActionResult DeleteAdmin(int adminId)
        {
            var admin_delete = db.TUsers.Where(x=>x.UserId==adminId).FirstOrDefault();
            
            if (admin_delete != null)
            {
                // Kiểm tra quyền để chắc chắn không xoá admin quyền cao (Super Admin)
                if (admin_delete.RoleId != 3) // Tránh xoá Super Admin
                {
                    admin_delete.Deleted = true;
                    db.TUsers.Update(admin_delete);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xoá Super Admin!" });
                }
            }
            return Json(new { success = false, message = "Không tìm thấy admin!" });
            /*var lstAdmin = db.TUsers.AsNoTracking().Where(x => (x.RoleId == 1 || x.RoleId == 3)).ToList();
            var model = new Email_Password_forAdmin
            {
                listAdmin = lstAdmin
            };
            return RedirectToAction("ManageAdmin", model);*/
        }
    }



}
