using Microsoft.AspNetCore.Mvc;
using Project1.Models;
using Project1.ViewModels;
using Project1.Helpers;
using Project1.Services;
using Microsoft.AspNetCore.Authorization;
using Project1.Models.Authentication;
using System.Net.WebSockets;

namespace Project1.Controllers
{
    public class CartController : Controller
    {
        private readonly PizzaOnlineContext db;
        private readonly IVnPayService _vnPayservice;

        public CartController(PizzaOnlineContext context, IVnPayService vnPayservice)
        {
            db = context;
            _vnPayservice = vnPayservice;
        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

		public IActionResult Index()
        {
            ViewBag.Vouchers = db.TVouchers.ToList();
            ViewBag.DiscountAmount = 0;
            ViewBag.VoucherCode = "";
            ViewBag.FinalPrice = Cart.Sum(p => p.ThanhTien);
            if (!Cart.Any())
            {
                HttpContext.Session.Remove("VoucherId");
                return View(Cart);
            }
           
            double? totalPrice = Cart.Sum(p => p.ThanhTien);
            if (long.TryParse(HttpContext.Session.GetString("VoucherId"), out long voucherId))
            {
                var voucher = db.TVouchers.SingleOrDefault(p => p.VoucherId == voucherId);
                // Tính tổng tiền sau khi giảm khuyến mại
                if (voucher != null)
                {
                    if (totalPrice < voucher.MinOrderValue)
                    {
                        HttpContext.Session.Remove("VoucherId");
                        return View(Cart);
                    }
                    var discountAmount = voucher.IsPercentDiscountType == true ? totalPrice * (voucher.DiscountValue / 100) : voucher.DiscountValue;
                    discountAmount = discountAmount <= voucher.MaxDiscountValue ? discountAmount : voucher.MaxDiscountValue;
                    totalPrice -= discountAmount;
                    ViewBag.DiscountAmount = discountAmount;
                    ViewBag.VoucherCode = voucher.Code;
                }
            }
            ViewBag.FinalPrice = totalPrice;  
            

            // Lấy địa chỉ từ Session
            var selectedAddress = HttpContext.Session.GetString("SelectedAddress");

            // Truyền giá trị vào ViewBag để hiển thị trong view nếu cần
            ViewBag.SelectedAddress = selectedAddress;

            return View(Cart);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            //if (Cart.Count == 0)
            //{
            //    return Redirect("/");
            //}

            return View(Cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(p => p.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.HoTen} {model.DienThoai}",
                        FullName = model.HoTen,
                        OrderId = new Random().Next(1000, 100000)
                    };
                    return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
                }

                //var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                //var khachHang = new KhachHang();
                //if (model.GiongKhachHang)
                //{
                //    khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                //}

                //var hoadon = new HoaDon
                //{
                //    MaKh = customerId,
                //    HoTen = model.HoTen ?? khachHang.HoTen,
                //    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                //    DienThoai = model.DienThoai ?? khachHang.DienThoai,
                //    NgayDat = DateTime.Now,
                //    CachThanhToan = "COD",
                //    CachVanChuyen = "GRAB",
                //    MaTrangThai = 0,
                //    GhiChu = model.GhiChu
                //};

                //db.Database.BeginTransaction();
                //try
                //{

                //    db.Add(hoadon);
                //    db.SaveChanges();

                //    var cthds = new List<ChiTietHd>();
                //    foreach (var item in Cart)
                //    {
                //        cthds.Add(new ChiTietHd
                //        {
                //            MaHd = hoadon.MaHd,
                //            SoLuong = item.SoLuong,
                //            DonGia = item.DonGia,
                //            MaHh = item.MaHh,
                //            GiamGia = 0
                //        });
                //    }
                //    db.AddRange(cthds);
                //    db.SaveChanges();
                //    db.Database.CommitTransaction();

                //    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                //    return View("Success");
                //}
                //catch
                //{
                //    db.Database.RollbackTransaction();
                //}
            }

            return View(Cart);
        }

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }


            // Lưu đơn hàng vô database

            TempData["Message"] = $"Thanh toán VNPay thành công";
            return RedirectToAction("PaymentSuccess");
        }
    }
}
