using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Project1.DTO;
using Project1.Helpers;
using Project1.Models;
using Project1.ViewModels;

namespace Project1.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private PizzaOnlineContext db = new PizzaOnlineContext();

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        [HttpPost("AddToCart")]
        public IActionResult AddToCart([FromQuery] int productDetailId, [FromForm] int quantity = 1, [FromForm] bool isPizza = true)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.ProductDetailId == productDetailId);
            var address = HttpContext.Session.GetString("SelectedAddress");
            if (address == null)
            {
                return Ok(new
                {
                    //success = false,
                    //message = "Hãy nhập địa chỉ giao hàng!"
                });
            }
            if (item == null)
            {
                var productDetail = db.TProductDetails.SingleOrDefault(p => p.ProductDetailId == productDetailId);
                if (productDetail == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Không tìm thấy sản phẩm có mã {productDetailId}"
                    });
                }
                var product = db.TProducts.SingleOrDefault(p => p.ProductId == productDetail.ProductId);
                if (product != null)
                {
                    item = new CartItem
                    {
                        ProductDetailId = productDetail.ProductDetailId,
                        Image = product.Image ?? string.Empty,
                        Name = product.Name,
                        Price = productDetail.Price ?? 0,
                        SizeId = productDetail.SizeId,
                        CrustId = productDetail.CrustId,
                        Quantity = quantity,
                        isPizza = isPizza
                    };
                    gioHang.Add(item);
                }
            }
            else
            {
                item.Quantity += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            return Ok(new
            {
                success = true,
                message = "Đưa sản phẩm vào giỏ hàng thành công"
            });
        }

        [HttpGet("GetCartQuantity")]
        public IActionResult GetCartQuantity()
        {
            var gioHang = Cart;
            var quantity = Cart.Sum(p => p.Quantity);
            return Ok(new {quantity});
        }

        [HttpGet("ApplyVoucher")]
        public async Task<IActionResult> ApplyVoucher(long id)
        {
            var voucher = await db.TVouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Voucher không tồn tại"
                });
            }

            double? totalPrice = Cart.Sum(p => p.ThanhTien);

            if (totalPrice < voucher.MinOrderValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Đơn hàng của bạn chưa đạt tối thiểu {String.Format("{0:0,0 \u20AB}", voucher.MinOrderValue).Replace(",", ".")} để áp dụng {voucher.Code}"
                });
            }

            var discountAmount = voucher.IsPercentDiscountType == true ? totalPrice * (voucher.DiscountValue / 100) : voucher.DiscountValue;
            discountAmount = discountAmount <= voucher.MaxDiscountValue ? discountAmount : voucher.MaxDiscountValue;
            totalPrice -= discountAmount;

            HttpContext.Session.SetString("VoucherId", voucher.VoucherId.ToString());

            return Ok(new
            {
                success = true,
                message = $"{voucher.Code} được áp dụng thành công",
                voucher = new
                {
                    voucher.Code,
                    discountAmount,
                    totalPrice
                }
            });
        }

        [HttpPost("CreateOrder")]
        public IActionResult CreateOrder()
        {
            var gioHang = Cart;
            if (!gioHang.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Giỏ hàng của bạn chưa có sản phẩm"
                });
            }

            // Lấy voucher id từ session
            long voucherId = 0;
            double? totalPrice = gioHang.Sum(p => p.ThanhTien);
            if (long.TryParse(HttpContext.Session.GetString("VoucherId"), out long id))
            {
                voucherId = id;
                var voucher = db.TVouchers.SingleOrDefault(p => p.VoucherId == voucherId);
                // Tính tổng tiền sau khi giảm khuyến mại
                if (voucher != null)
                {
                    var discountAmount = voucher.IsPercentDiscountType == true ? totalPrice * (voucher.DiscountValue / 100) : voucher.DiscountValue;
                    discountAmount = discountAmount <= voucher.MaxDiscountValue ? discountAmount : voucher.MaxDiscountValue;
                    totalPrice -= discountAmount;

                    // Giảm số lượng voucher trừ đi 1
                    if (voucher.Number > 0)
                    {
                        voucher.Number -= 1;
                        db.TVouchers.Update(voucher);
                        db.SaveChanges();
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Voucher {voucher.Code} đã hết số lượng"
                        });
                    }
                }
            }

            if (long.TryParse(HttpContext.Session.GetString("UserId"), out long userId))
            {
                var address = HttpContext.Session.GetString("SelectedAddress");
                if (address != null)
                {
                    var orderId = db.TOrders.OrderByDescending(o => o.OrderId).FirstOrDefault()?.OrderId + 1 ?? 1;
                    var orderCode = "ORDER" + orderId;

                    // Thêm mới một Order
                    var order = new TOrder
                    {
                        OrderId = orderId,
                        Code = orderCode,
                        CustomerId = userId,
                        TotalPrice = totalPrice,
                        Address = address,
                        Date = DateTime.Now,
                        VoucherId = voucherId != 0 ? voucherId : null,
                        StatusId = 1,
                        Deleted = false
                    };

                    // Thêm Order vào DB
                    db.TOrders.Add(order);
                    db.SaveChanges();

                    // Tạo OrderDetail cho từng ProductDetail
                    var orderDetails = Cart.Select(pd => new TOrderDetail
                    {
                        OrderId = orderId,
                        ProductDetailId = pd.ProductDetailId,
                        Number = pd.Quantity,
                    }).ToList();

                    db.TOrderDetails.AddRange(orderDetails);
                    db.SaveChanges();

                    HttpContext.Session.Remove(MySetting.CART_KEY);

                    HttpContext.Session.Remove("VoucherId");

                    return Ok(new
                    {
                        success = true,
                        message = "Bạn đã đặt hàng thành công"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bạn chưa nhập địa chỉ giao hàng"
                    });
                }
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập"
                });
            }
        }

        [HttpGet("RemoveCart")]
        public IActionResult RemoveCart(long productDetailId)
        {
            var gioHang = Cart;
            var productDetail = gioHang.SingleOrDefault(p => p.ProductDetailId == productDetailId);
            if (productDetail != null)
            {
                gioHang.Remove(productDetail);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }

            var CartItems = gioHang.Select(item => new
            {
                item.ProductDetailId,
                item.Image,
                item.Name,
                item.Price,
                item.SizeId,
                item.CrustId,
                item.Quantity,
                item.ThanhTien,
                item.isPizza
            });

            long voucherId = 0;
            double? totalPrice = gioHang.Sum(p => p.ThanhTien);
            if (long.TryParse(HttpContext.Session.GetString("VoucherId"), out long id))
            {
                voucherId = id;
                var voucher = db.TVouchers.SingleOrDefault(p => p.VoucherId == voucherId);
                if (voucher != null)
                {
                    if(totalPrice < voucher.MinOrderValue)
                    {
                        HttpContext.Session.Remove("VoucherId");
                        return Ok(new
                        {
                            Message = $"Đơn hàng của bạn chưa đạt tối thiểu {String.Format("{0:0,0 \u20AB}", voucher.MinOrderValue).Replace(",", ".")} để áp dụng {voucher.Code}",
                            ApplyVoucher = false,
                            CartItems,
                            TotalPrice = gioHang.Sum(p => p.Price * p.Quantity),
                            FinalPrice = totalPrice,                          
                        });
                    }
                    else
                    {
                        var discountAmount = voucher.IsPercentDiscountType == true ? totalPrice * (voucher.DiscountValue / 100) : voucher.DiscountValue;
                        discountAmount = discountAmount <= voucher.MaxDiscountValue ? discountAmount : voucher.MaxDiscountValue;
                        totalPrice -= discountAmount;
                    }              
                }
            }

            return Ok(new
            {
                ApplyVoucher = true,
                CartItems,
                TotalPrice = gioHang.Sum(p => p.Price * p.Quantity),
                FinalPrice = totalPrice,              
            });
        }

        [HttpGet("GetUpdatedVouchers")]
        public IActionResult GetUpdatedVouchers(double totalPrice)
        {
            var voucher = db.TVouchers;
            var updatedVoucher = voucher.Select(v => new
            {
                v.VoucherId,
                v.Code,
                v.DiscountValue,
                v.IsPercentDiscountType,
                v.MaxDiscountValue,
                v.MinOrderValue,
                v.Number,
                v.StartDate,
                v.ExpirationDate,
                isValid = totalPrice >= v.MinOrderValue && v.Number > 0 && v.StartDate <= DateTime.Now && v.ExpirationDate >= DateTime.Now,
                Type = DateTime.Now < v.StartDate ? "Sắp ra mắt" : (DateTime.Now > v.ExpirationDate ? "Hết hạn" : (totalPrice < v.MinOrderValue ? "Không đủ điều kiện" : (v.Number <= 0 ? "Hết số lượng" : "Áp dụng")))
            }).ToList();
            return Ok(updatedVoucher);
        }
    }
}


    




