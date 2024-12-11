document.addEventListener("DOMContentLoaded", function () {
    const totalPriceElement = document.getElementById("total-price");
    const voucherCodeElement = document.getElementById('voucher-code');
    const discountValueElement = document.getElementById('discount-value');
    const finalPriceElement = document.getElementById('final-price');

    // Lắng nghe sự kiện click vào nút "Áp dụng"
    $(document).on('click', '.apply-discount', function (event) {
        event.preventDefault();

        // Lấy voucherId từ thuộc tính voucher-id của nút Áp dụng
        const voucherId = $(this).attr('voucher-id');
        console.log('voucher id: ' + voucherId);

        if (!voucherId) {
            alert("Không tìm thấy mã voucher");
            return;
        }

        // Gửi yêu cầu tới API 
        fetch(`/api/CartAPI/ApplyVoucher?id=${voucherId}`)
            .then(response => response.json())
            .then(data => {
                if (!data.success) {
                    toastr.error(data.message, 'Lỗi');
                    return;
                }

                const voucher = data.voucher;
                const voucherCode = voucher.code;
                const discountAmount = voucher.discountAmount;
                const finalPrice = voucher.totalPrice;

                $('#voucher-code').text(voucherCode);
                $('#discount-value').text(formatCurrency(discountAmount));
                $('#final-price').text(formatCurrency(finalPrice));

                toastr.success(data.message, 'Thành công');
            })
            .catch(error => {
                console.error("Error: ", error);
                toastr.error("Có lỗi xảy ra khi kết nối máy chủ", "Lỗi");
            });
    });

    const checkoutButtonElement = document.getElementById('checkout');
    checkoutButtonElement.addEventListener("click", function (event) {
        event.preventDefault();

        // Gửi yêu cầu đến API CreateOrder
        fetch('/api/CartAPI/CartValidation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    window.location.href = "/Cart/Checkout";
                }
                else {                   
                    toastr.warning(data.message, "Cảnh báo");
                }
                
            })
            .catch(error => {
                console.error("Error: ", error);
                toastr.error("Có lỗi xảy ra khi kết nối máy chủ", "Lỗi");
            });
    });

    // Lắng nghe sự kiện click vào nút "Xóa sản phẩm"
    $(document).on('click', '.remove-cart', function (event) {
        event.preventDefault();

        const productDetailId = this.getAttribute('data-product-detail-id');
        console.log('product detail id: ' + productDetailId);

        $.ajax({
            url: '/api/CartAPI/RemoveCart',
            type: 'GET',
            data: { productDetailId: productDetailId },
            success: function (response) {
                // Cập nhật bảng giỏ hàng
                let applyVoucher = response.applyVoucher;
                let cartItems = response.cartItems;
                let totalPrice = response.totalPrice;
                let finalPrice = response.finalPrice;

                console.log('cart items: ' + cartItems);
                console.log('total price: ' + totalPrice);

                let tableBody = $('table tbody');
                tableBody.empty(); // Xóa dữ liệu cũ

                // Thêm dữ liệu mới
                cartItems.forEach(item => {
                    const row = `<tr>
                        <td class="p-4">
                            <div class="d-flex align-items-center">
                                <img src="../ImagesProduct/${item.image}" class="rounded me-4" alt="" style="width: 60px; height: auto;">
                                <div>
                                    <a class="text-dark">${item.name}</a>
                                    ${item.isPizza ? `
                                    <small class="d-block">
                                        <span>Kích cỡ: ${item.sizeId == 1 ? "Nhỏ" : item.sizeId == 2 ? "Vừa" : "Lớn"}</span> |
                                        <span>Đế viền: ${item.crustId == 1 ? "Mỏng" : item.crustId == 2 ? "Dày" : "Phô mai"}</span>
                                    </small>
                                    ` : ''}
                                </div>
                            </div>
                        </td>
                        <td class="text-center align-middle p-4">${formatCurrency(item.price)}</td>
                        <td class="text-center align-middle p-4">${item.quantity}</td>
                        <td class="text-center align-middle p-4">${formatCurrency(item.thanhTien)}</td>
                        <td class="text-center align-middle px-0">
                            <a class="remove-cart shop-tooltip close float-none text-danger" title="Xóa" data-product-detail-id="${item.productDetailId}" style="cursor:pointer">×</a>
                        </td>
                    </tr>`;
                    tableBody.append(row);
                });

                // Cập nhật tổng tiền
                totalPriceElement.textContent = formatCurrency(totalPrice);

                if (!applyVoucher) {
                    voucherCodeElement.textContent = "";
                    discountValueElement.textContent = formatCurrency(0);
                    toastr.warning(response.message, "Cảnh báo");
                }

                finalPriceElement.textContent = formatCurrency(finalPrice);

                // Cập nhật số lượng sản phẩm trong giỏ
                updateCartQuantity();

                // Cập nhật danh sách voucher
                updateVoucherList(totalPrice);
            },
            error: function () {
                toastr.error("Không thể xóa sản phẩm", "Lỗi");
            }
        });
    });

    function updateVoucherList(totalPrice) {
        $.ajax({
            url: '/api/CartAPI/GetUpdatedVouchers',
            type: 'GET',
            data: { totalPrice: totalPrice },
            success: function (response) {
                const voucherContainer = $('#voucher-list');
                voucherContainer.empty();

                response.forEach(item => {
                    const opacity = item.isValid ? "1" : "0.6";
                    const voucherHTML = `<div class="discount-item d-flex justify-content-between align-items-center mb-3 p-3" style="background-color: #ffefe6; border-radius: 10px; opacity:${opacity}">
                        <div class="d-flex align-items-center">
                            <div>
                                <strong>${item.code}</strong><br>
                                ${item.discountValue > 0 ? `<span>Giảm ${item.isPercentDiscountType ? item.discountValue + "%" + ` tối đa ${formatCurrency(item.maxDiscountValue)}` : formatCurrency(item.discountValue)}</span><br>` : ""}
                                <span>Đơn tối thiểu ${formatCurrency(item.minOrderValue)}</span><br>
                                <span>Số voucher hiện có ${item.number}</span><br>
                                <span>${formatDate(item.startDate)} - ${formatDate(item.expirationDate)}</span>
                            </div>
                        </div>
                        <a href="#" class="apply-discount" data-bs-dismiss="modal" voucher-id="${item.voucherId}" style="pointer-events: ${item.isValid ? "auto" : "none"}">${item.type}</a>
                    </div>`;
                    voucherContainer.append(voucherHTML);
                });
            },
            error: function () {
                console.error("Có lỗi xảy ra khi cập nhật danh sách voucher");
            }
        });
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
    }

    function formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }
});