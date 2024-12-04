document.addEventListener("DOMContentLoaded", function () {

    const submitFeelingButton = document.getElementById('submit-feeling');
    submitFeelingButton.addEventListener("click", function (event) {
        event.preventDefault();

        const customerFeelingElement = document.getElementById('CustomerFeeling');
        const feeling = customerFeelingElement.value;
        if (!feeling.trim()) {
            customerFeelingElement.placeholder = 'Cảm nhận không được để trống';
            return;
        }

        const orderId = submitFeelingButton.getAttribute('data-order-id');

        fetch(`/api/OrderAPI/UpdateCustomerFeeling?id=${orderId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(feeling),
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {                  
                    toastr.success(data.message, "Thành công");
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

    const confirmCancelButton = document.getElementById('confirmCancel');
    confirmCancelButton.addEventListener("click", function (event) {
        event.preventDefault();

        const orderId = confirmCancelButton.getAttribute('data-order-id');

        fetch(`/api/OrderAPI/CancelOrder?id=${orderId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Cập nhật thanh progress
                    const progressBar = document.querySelector('.progress-bar');
                    progressBar.style.width = '100%';
                    progressBar.classList.remove('bg-info', 'bg-warning', 'bg-success');
                    progressBar.classList.add('bg-danger');
                    progressBar.textContent = 'Hủy';

                    toastr.success(data.message, "Thành công");
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
});