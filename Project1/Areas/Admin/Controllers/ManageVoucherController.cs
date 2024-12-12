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
using Mono.TextTemplating;

namespace Project1.Areas.Admin.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Area("admin")]
    [Route("admin")]
    [Route("admin/managevoucher")]
    public class ManageVoucherController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ManageVoucherController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        PizzaOnlineContext db = new PizzaOnlineContext();


        //từ đây trở xuống là các action xử lý voucher
        [Route("ListVoucher")]
        public IActionResult ListVoucher(int state)
        {
            if (state == 1)
            {
                var listVoucher = db.TVouchers.AsNoTracking().Where(x => x.Deleted == false && x.Number > 0).ToList();
                ViewBag.Null = "List thành công";
                return View("ListVoucher", listVoucher);
            }
            else if (state == 2)
            {

                var listVoucher = db.TVouchers.AsNoTracking().Where(x => x.Deleted == true || x.Number == 0).ToList();
                if (listVoucher.Count() == 0) { ViewBag.Null = "Không có voucher nào bị hết hạn hoặc hết số lượng"; }
                return View("ListVoucher", listVoucher);
            }
            else
            {
                return View();
            }
        }

        [Route("AddVoucher")]
        [HttpGet]
        public IActionResult AddVoucher()
        {

            return View("AddVoucher");
        }

        [Route("AddVoucher")]
        [HttpPost]
        public IActionResult AddVoucher(TVoucher voucher)
        {
            long change = long.Parse(HttpContext.Session.GetString("UserId"));
            var user = db.TUsers.FirstOrDefault(x => x.UserId == change);
            int lastVoucherId = (int)db.TVouchers.DefaultIfEmpty().Max(p => (int?)p.VoucherId ?? 0);
            voucher.VoucherId = lastVoucherId + 1;
            voucher.CreatedBy = user.Nickname;
            voucher.CreatedDate = DateTime.UtcNow;
            voucher.LastModifiedBy = user.Nickname;
            voucher.LastModifiedDate = DateTime.UtcNow;
            voucher.Deleted = false;
            if (voucher != null)
            {
                db.TVouchers.Add(voucher);
                db.SaveChanges();
                return RedirectToAction("ListVoucher","ManageVoucher", new { state = 1 });
            }
            return View("AddVoucher", voucher);
        }
        //kết thúc các action xử lý voucher
        [Route("DeActiveVoucher")]
        public IActionResult DeActiveVoucher(int voucherId)
        {
            var voucher = db.TVouchers.Where(x => x.VoucherId == voucherId).FirstOrDefault();
            long change = long.Parse(HttpContext.Session.GetString("UserId"));
            var user = db.TUsers.FirstOrDefault(x => x.UserId == change);

            voucher.Deleted = true;
            voucher.LastModifiedBy = user.Nickname;
            voucher.LastModifiedDate = DateTime.UtcNow;

            db.TVouchers.Update(voucher);
            db.SaveChanges();
            return RedirectToAction("ListVoucher", "ManageVoucher", new { state = 1 });
        }

        [Route("GetVoucherIn4")]
        public IActionResult GetVoucherIn4(int voucherId)
        {
            /*var voucherIn4=from vou in db.TVouchers where vou.VoucherId == voucherId
                           select new ComebackVoucher_ViewModel {  };*/
            var voucherIn4 = db.TVouchers
                .Where(v => v.VoucherId == voucherId)
                .Select(v => new ComebackVoucher_ViewModel
                {
                    voucherId=v.VoucherId,
                    Quantity = v.Number,
                    StartDate = v.StartDate,
                    EndDate = v.ExpirationDate
                    // Ánh xạ các trường cần thiết ở đây
                })
                .FirstOrDefault(); // Lấy đối tượng duy nhất
            return PartialView("../PartialView/_VoucherIn4", voucherIn4);
        }

        [Route("ActiveVoucher")]
        [HttpPost]
        public IActionResult ActiveVoucher(ComebackVoucher_ViewModel voucherIn4)
        {
            var voucher = db.TVouchers.Where(x => x.VoucherId == voucherIn4.voucherId).FirstOrDefault();
            voucher.Number=voucherIn4.Quantity;
            voucher.StartDate = voucherIn4.StartDate;
            voucher.ExpirationDate=voucherIn4.EndDate;
            voucher.Deleted = false;
            db.TVouchers.Update(voucher);
            db.SaveChanges();
            return RedirectToAction("ListVoucher", "ManageVoucher", new { state = 1 });
        }

    }
}
