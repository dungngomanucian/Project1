using Microsoft.AspNetCore.Mvc;
using Project1.Models;
using Project1.ViewModels;
using Project1.Helpers;
using Project1.Services;
using Microsoft.AspNetCore.Authorization;
using Project1.Models.Authentication;
using System.Net.WebSockets;
using MailKit.Search;
using System.Text.RegularExpressions;

namespace Project1.Controllers
{
    public class CartController : Controller
    {
        private readonly PizzaOnlineContext db;
        private readonly IVnPayService _vnPayservice;
        private readonly PaypalClient _paypalClient;
        private readonly ExchangeRateService _exchangeRateService;

        public CartController(PizzaOnlineContext context, IVnPayService vnPayservice, PaypalClient paypalClient,
            ExchangeRateService exchangeRateService)
        {
            db = context;
            _vnPayservice = vnPayservice;
            _paypalClient = paypalClient;
            _exchangeRateService = exchangeRateService;
        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

		public IActionResult Index()
        {
            // Lấy địa chỉ từ Session
            var selectedAddress = HttpContext.Session.GetString("SelectedAddress");

            // Truyền giá trị vào ViewBag để hiển thị trong view nếu cần
            ViewBag.SelectedAddress = selectedAddress;

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

            return View(Cart);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
            {
                return Redirect("/Menu/Index");
            }
            long customerId = GetCustomerId();
            PrepareCheckoutViewBag(customerId);

            ViewBag.PaypalClientId = _paypalClient.ClientId;

            double? totalPrice = Cart.Sum(p => p.ThanhTien);
            
            var (voucherId, voucherCode, discountAmount, finalPrice) = HandleVoucher(totalPrice);

            ViewBag.TotalPrice = totalPrice;
            ViewBag.VoucherCode = voucherCode;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.FinalPrice = finalPrice;

            return View(Cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        {
            long customerId = GetCustomerId();
            PrepareCheckoutViewBag(customerId);

            if (ModelState.IsValid)
            {
                if (payment == "VNPay")
                {
                    return ProcessVNPay(model);
                }

                var khachHang = new TUser();
                if (ViewBag.CanEdit == false)
                {
                    khachHang = db.TUsers.SingleOrDefault(kh => kh.UserId == customerId);
                }

                string code = GenerateOrderId().ToString();

                SaveOrderAndDetails(
                    customerId,
                    code,
                    model.GhiChu,
                    1, // paymentmethod, can using this code: payment == "COD" ? 1 : 2
                    Cart,
                    model.HoTen ?? khachHang?.Nickname ?? ""
                );

                HttpContext.Session.Remove(MySetting.CART_KEY);
                HttpContext.Session.Remove("VoucherId");

                TempData["Message"] = "Đơn hàng được đặt thành công, bạn sẽ thanh toán khi nhận hàng";

                return RedirectToAction("Index", "Order");
            }

            //hien thi span validate
            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.HoTen))
                {
                    ViewBag.HoTenError = "Họ và tên không được để trống";
                }
                if (string.IsNullOrEmpty(model.DiaChi))
                {
                    ViewBag.DiaChiError = "Địa chỉ không được để trống";
                }
                if (string.IsNullOrEmpty(model.DienThoai) || !Regex.IsMatch(model.DienThoai, @"^\d{10}$"))
                {
                    ViewBag.DienThoaiError = "Số điện thoại phải có đủ 10 chữ số";
                }
            }

            return View(Cart);
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response?.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            // Lấy thông tin từ session
            string hoTen = HttpContext.Session.GetString("HoTen");
            string diaChi = HttpContext.Session.GetString("DiaChi");
            string dienThoai = HttpContext.Session.GetString("DienThoai");
            string ghiChu = HttpContext.Session.GetString("GhiChu");

            // Luu vao DB Order, OrderDetail
            // lay thong tin user
            long customerId = GetCustomerId();

            //PrepareCheckoutViewBag(customerId);

            var khachHang = db.TUsers.SingleOrDefault(kh => kh.UserId == customerId);
            string code = GenerateOrderId().ToString();

            // Gọi phương thức SaveOrderAndDetails để lưu hóa đơn và chi tiết hóa đơn
            SaveOrderAndDetails(
                customerId,
                code,
                $"{response.OrderDescription}. {ghiChu ?? "VNPay"}", // Đưa ghi chú từ session vào
                2, // VNPay payment method
                Cart,
                string.IsNullOrEmpty(hoTen) ? khachHang?.Nickname : hoTen // Nếu không có tên trong session, dùng nickname
            );

            // Xóa giỏ hàng khỏi session
            HttpContext.Session.Remove(MySetting.CART_KEY);
            HttpContext.Session.Remove("VoucherId");

            TempData["Message"] = "Thanh toán VNPay thành công.";
            return RedirectToAction("Index", "Order");
        }

        #region Paypal payment
        
        [HttpPost("/Cart/SaveToSession")]
        public IActionResult SaveToSession([FromBody] CheckoutVM model)
        {
            HttpContext.Session.SetString("HoTen", model.HoTen ?? "");
            HttpContext.Session.SetString("DiaChi", model.DiaChi ?? "");
            HttpContext.Session.SetString("DienThoai", model.DienThoai ?? "");
            HttpContext.Session.SetString("GhiChu", model.GhiChu ?? "");

            return Ok(new { Message = "Session updated successfully" });
        }
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {

            // Thông tin đơn hàng gửi qua Paypal
            double? totalPrice = Cart.Sum(p => p.ThanhTien);
            var (voucherId, voucherCode, discountAmount, finalPrice) = HandleVoucher(totalPrice);

            double tongTienVND = finalPrice ?? 0;
            if (tongTienVND <= 0)
            {
                return BadRequest(new { Message = "Tổng tiền không hợp lệ" });
            }

            // Lấy tỷ giá từ EUR sang VND
            //double tyGiaEURtoVND = await _exchangeRateService.GetExchangeRateAsync();
            //if (tyGiaEURtoVND <= 0)
            //{
            //    return BadRequest(new { Message = "Không thể lấy tỷ giá từ EUR sang VND" });
            //}

            //double tyGiaEURtoUSD = await _exchangeRateService.GetExchangeRateAsync("USD");
            //if (tyGiaEURtoUSD <= 0)
            //{
            //    return BadRequest(new { Message = "Không thể lấy tỷ giá từ EUR sang USD" });
            //}

            //double tyGiaUSDToVND = tyGiaEURtoVND / tyGiaEURtoUSD;

            var tongTienUSD = Math.Round(tongTienVND / 24000 /*tyGiaEURtoVND*/, 2).ToString("F2");
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTienUSD, donViTienTe, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);
                if (response.status != "COMPLETED")
                {
                    return BadRequest(new { Message = "Thanh toán không thành công từ PayPal." });
                }

                // Lấy thông tin từ session
                string hoTen = HttpContext.Session.GetString("HoTen");
                string diaChi = HttpContext.Session.GetString("DiaChi");
                string dienThoai = HttpContext.Session.GetString("DienThoai");
                string ghiChu = HttpContext.Session.GetString("GhiChu");

                // Luu vao DB Order, OrderDetail
                long customerId = GetCustomerId();
                var khachHang = db.TUsers.SingleOrDefault(kh => kh.UserId == customerId);
                string orderDescription = $"Thanh toán đơn hàng #{orderID} qua PayPal";

                SaveOrderAndDetails(
                    customerId,
                    orderID,
                    $"{orderDescription}. {ghiChu ?? ""}",
                    3, // PaymentMethodId cho PayPal
                    Cart,
                    string.IsNullOrEmpty(hoTen) ? khachHang?.Nickname : hoTen // Nếu không có tên từ session, dùng tên nickname
                );

                HttpContext.Session.Remove(MySetting.CART_KEY);
                HttpContext.Session.Remove("VoucherId");
                TempData["Message"] = "Thanh toán PayPal thành công.";
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        #endregion

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return RedirectToAction("Index", "Order");
        }

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }
        private long GenerateOrderId()
        {
            var lastOrder = db.TOrders
                              .OrderByDescending(x => x.OrderId)
                              .FirstOrDefault();
            if (lastOrder != null)
            {
                return lastOrder.OrderId + 1;
            }
            return 1;
        }
        private void PrepareCheckoutViewBag(long userId)
        {
            var user = db.TUsers.SingleOrDefault(u => u.UserId == userId);

            ViewBag.CanEdit = user == null ? true : false; // CanEdit sẽ là true nếu không có thông tin user.
            //ViewBag.HoTen = user?.Nickname ?? string.Empty;
            ViewBag.HoTen = $"{user?.LastName} {user?.FirstName}".Trim();
            ViewBag.DiaChi = HttpContext.Session.GetString("SelectedAddress");
            ViewBag.DienThoai = user?.PhoneNumber ?? string.Empty;
        }
        private long GetCustomerId()
        {
            var customerIdString = HttpContext.Session.GetString("UserId");
            if (!long.TryParse(customerIdString, out long customerId))
            {
                throw new Exception("Customer ID không hợp lệ.");
            }
            return customerId;
        }

        private IActionResult ProcessVNPay(CheckoutVM model)
        {
            HttpContext.Session.SetString("HoTen", model.HoTen ?? "");
            HttpContext.Session.SetString("DiaChi", model.DiaChi ?? "");
            HttpContext.Session.SetString("DienThoai", model.DienThoai ?? "");
            HttpContext.Session.SetString("GhiChu", model.GhiChu ?? "");  // neu null -> ""

            double? totalPrice = Cart.Sum(p => p.ThanhTien);
            var (voucherId, voucherCode, discountAmount, finalPrice) = HandleVoucher(totalPrice);

            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = finalPrice,
                CreatedDate = DateTime.Now,
                Description = $"{model.HoTen} {model.DienThoai}",
                FullName = model.HoTen,
                OrderId = (int)GenerateOrderId()
            };
            return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
        }
        private void SaveOrderAndDetails(long customerId,string code, string note, int paymentMethodId, List<CartItem> cartItems, string customerName)
        {
            double? totalPrice = cartItems.Sum(p => p.ThanhTien);
            var (voucherId, voucherCode, discountAmount, finalPrice) = HandleVoucher(totalPrice);
            totalPrice = finalPrice;

            // Cập nhật lại số lượng voucher
            if (voucherId != 0)
            {
                var voucher = db.TVouchers.SingleOrDefault(v => v.VoucherId == voucherId);
                if (voucher != null)
                {
                    voucher.Number -= 1; // Giảm số lượng voucher đi 1
                    db.TVouchers.Update(voucher);
                    db.SaveChanges();
                }
            }

            var address = HttpContext.Session.GetString("SelectedAddress");
            if (address != null)
            {
                long orderId = GenerateOrderId();
                var hoadon = new TOrder
                {
                    OrderId = orderId,
                    CustomerId = customerId,
                    Code = code,
                    Date = DateTime.Now,
                    Note = note,
                    TotalPrice = totalPrice,
                    PaymentMethodId = paymentMethodId,
                    StatusId = 1,
                    Address = address,
                    VoucherId = voucherId != 0 ? voucherId : null
                };

                db.Database.BeginTransaction();
                try
                {
                    db.Add(hoadon);
                    db.SaveChanges();

                    var cthds = cartItems.Select(item => new TOrderDetail
                    {
                        OrderId = hoadon.OrderId,
                        ProductDetailId = item.ProductDetailId,
                        Number = item.Quantity,
                        Name = customerName // Dùng tên từ session hoặc nickname của khách hàng
                    }).ToList();

                    foreach(var item in cartItems)
                    {
                        var productDetail = db.TProductDetails.SingleOrDefault(pd => pd.ProductDetailId == item.ProductDetailId);
                        if(productDetail != null)
                        {
                            productDetail.Number -= item.Quantity;
                            if(productDetail.Number < 0)
                            {
                                db.Database.RollbackTransaction();
                                throw new Exception("Không đủ số lượng chi tiết sản phẩm");
                            }
                            db.TProductDetails.Update(productDetail);
                        }                       
                    }

                    db.AddRange(cthds);
                    db.SaveChanges();
                    db.Database.CommitTransaction();
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                    TempData["Message"] = $"Đã xảy ra lỗi khi lưu hóa đơn: {ex.Message}";
                    throw;
                }
            }          
        }

        private (long VoucherId, string VoucherCode, double? DiscountAmount, double? FinalPrice) HandleVoucher(double? totalPrice)
        {
            string voucherCode = "";
            double? discountAmount = 0;

            if (long.TryParse(HttpContext.Session.GetString("VoucherId"), out long voucherId))
            {
                var voucher = db.TVouchers.SingleOrDefault(v => v.VoucherId == voucherId);
                if (voucher != null)
                {
                    discountAmount = voucher.IsPercentDiscountType == true ? totalPrice * (voucher.DiscountValue / 100) : voucher.DiscountValue;
                    discountAmount = discountAmount <= voucher.MaxDiscountValue ? discountAmount : voucher.MaxDiscountValue;
                    totalPrice -= discountAmount;

                    if (voucher.Number > 0)
                    {
                        voucher.Number -= 1;
                        db.TVouchers.Update(voucher);
                        db.SaveChanges();
                    }

                    voucherCode = voucher.Code;
                }
            }

            return (voucherId, voucherCode, discountAmount, totalPrice);
        }

    }
}
