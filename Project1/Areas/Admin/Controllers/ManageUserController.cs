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
            var lstAdmin = db.TUsers.AsNoTracking().Where(x => (x.RoleId == 1 || x.RoleId==3) ).ToList();
            return View("ManageAdmin", lstAdmin);
        }


        [Route("AddAdmin")]
        [HttpGet]
        public IActionResult AddAdmin()
        {
           return View();
        }

        [HttpPost]
        public IActionResult GrantSuperAdmin(string email)
        {
            if (int.Parse(HttpContext.Session.GetString("RoleId")) == 3)
            {
                var admin = db.TUsers.FirstOrDefault(a => a.Email == email);
                if (admin != null)
                {
                    admin.RoleId = 3; // Cập nhật thành Super Admin
                    db.TUsers.Update(admin);
                    db.SaveChanges();
                }
                var superadmin = db.TUsers.FirstOrDefault(a => a.RoleId == 3);
                if (superadmin != null)
                {
                    superadmin.RoleId = 1;
                    db.TUsers.Update(superadmin);
                    db.SaveChanges();
                }
                TempData["SuccessMessage"] = "Cấp quyền Super Admin thành công!";
                return RedirectToAction("ManageAdmin");
            }
            else
            {
                TempData["WrongRole"]= "Không có quyền hạn để thực hiện thao tác này";
                return RedirectToAction("Index", "HomeAdmin");
            }
        }

    }



}
