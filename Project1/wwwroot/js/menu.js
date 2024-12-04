function showProductModal(productId, name, description, imageUrl, categoryId, isFilterPage) {

    // Cập nhật các thông tin cơ bản vào modal
    document.getElementById('modalProductName').innerText = name;
    document.getElementById('modalProductDescription').innerText = description;
    document.getElementById('modalProductImage').src = imageUrl;

    console.log('IsFilterPage: ' + isFilterPage);

    window.categoryId = categoryId;
    window.isFilterPage = isFilterPage;

    // Đặt lại các giá trị mặc định
    document.getElementById('sizeMedium').checked = true;
    document.getElementById('crustThick').checked = true;
    document.getElementById('quantityInput').value = 1;

    console.log('Intital Size: ' + $('input[name="size"]:checked').val());
    console.log('Initial Crust: ' + $('input[name="crust"]:checked').val());
    console.log('Initial Quantity: ' + document.getElementById('quantityInput').value);

    // Lấy Product Details bằng AJAX
    $.ajax({
        url: `/api/MenuAPI/${productId}/details`,
        method: 'GET',
        success: function (productDetails) {
            // Lưu productDetails vào biến để sử dụng khi cập nhật giá
            window.currentProductDetails = productDetails;

            // Ẩn các tùy chọn kích cỡ và đế viền nếu sản phẩm không phải Pizza và hiện nếu sản phẩm là Pizza
            toogleSizeAndCrustOption();

            // Cập nhật giá mặc định dựa vào lựa chọn đầu tiên
            updatePriceBasedOnSelection();

            // Gắn sự kiện onchange vào các radio button sau khi dữ liệu đã được load
            $('input[name="size"], input[name="crust"]').on('change', function () {
                updatePriceBasedOnSelection();
            });
        },
        error: function () {
            console.error('Không thể lấy chi tiết sản phẩm');
        }
    });

    // Hiển thị modal
    var productModal = new bootstrap.Modal(document.getElementById('productModal'));
    productModal.show();
}

function updatePriceBasedOnSelection() {

    console.log('Current ProductDetails: ' + window.currentProductDetails);

    const selectedSize = $('input[name="size"]:checked').val();
    const selectedCrust = $('input[name="crust"]:checked').val();
    const quantity = parseInt($('#quantityInput').val());

    console.log('Selected Size: ' + selectedSize);
    console.log('Selected Crust: ' + selectedCrust);

    // Tìm chi tiết sản phẩm tương ứng với size và crust
    const selectedDetail = window.currentProductDetails.find(detail => detail.sizeId == selectedSize && detail.crustId == selectedCrust);

    console.log('Selected ProductDetail: ' + selectedDetail);

    if (selectedDetail) {
        const totalPrice = selectedDetail.price * quantity;
        $('#modalProductPrice').text(totalPrice.toLocaleString('vi-VN') + ' \u20AB');
    } else {
        $('#modalProductPrice').text('Giá không khả dụng');
    }
}

// Hàm để tăng/giảm số lượng
function updateQuantity(change) {
    var quantityInput = document.getElementById('quantityInput');
    var quantity = parseInt(quantityInput.value);
    quantity += change;
    if (quantity < 1) quantity = 1;
    quantityInput.value = quantity;

    console.log('Updated Quantity: ' + quantityInput.value);

    // Cập nhật giá sau khi thay đổi số lượng
    updatePriceBasedOnSelection();
}

document.addEventListener("DOMContentLoaded", function () {
    $('#addToCartButton').on('click', function () {
        const selectedSize = $('input[name="size"]:checked').val();
        const selectedCrust = $('input[name="crust"]:checked').val();

        let isPizza = true;
        if (window.currentProductDetails && window.currentProductDetails.length == 1) {
            isPizza = false;
        }

        const selectedDetail = window.currentProductDetails.find(detail => detail.sizeId == selectedSize && detail.crustId == selectedCrust);
        if (selectedDetail) {
            const productDetailId = selectedDetail.productDetailId;
            const quantity = parseInt($('#quantityInput').val());

            console.log('product detail id: ' + productDetailId);
            console.log('quantity: ' + quantity);
            console.log('is pizza: ' + isPizza);

            $.ajax({
                url: '/api/CartAPI/AddToCart?productDetailId=' + productDetailId,
                type: 'POST',
                data: { quantity: quantity, isPizza: isPizza},
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message, "Thành công");
                        updateCartQuantity();
                    }
                    else {
                        toastr.warning(response.message, "Cảnh báo");
                    }
                },
                error: function () {
                    toastr.error("Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng", "Lỗi");
                }
            });
        } else {
            toastr.error('Không thể thêm sản phẩm vào giỏ hàng, vui lòng chọn lại', "Lỗi");
        }
    });
});

function toogleSizeAndCrustOption() {
    if (window.currentProductDetails && window.currentProductDetails.length === 1) {
        $('#sizeOptions').hide();
        $('#crustOptions').hide();
    }
    else {
        $('#sizeOptions').show();
        $('#crustOptions').show();
    }
}