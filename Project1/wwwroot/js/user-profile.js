$(document).ready(function () {
    // Load user profile khi modal mở
    $('#openProfileModal').click(function () {
        loadUserProfile();
    });

    function loadUserProfile() {
        $.ajax({
            url: '/User/GetUserProfile',
            method: 'GET',
            success: function (response) {

                if (response.success) {
                    var user = response.data;
                    // Điền dữ liệu vào form
                    $('#UserId').val(user.userId);
                    $('#FirstName').val(user.firstName);
                    $('#LastName').val(user.lastName);
                    $('#Nickname').val(user.nickname);
                    $('#Email').val(user.email);
                    $('#PhoneNumber').val(user.phoneNumber);
                    $('#Address').val(user.address);

                    // Mở modal sau khi load dữ liệu thành công
                    $('#userProfileModal').modal('show');
                } else {
                    toastr.error(response.message || 'Không thể tải thông tin người dùng');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('Đã xảy ra lỗi khi tải thông tin người dùng');
            }
        });
    }

    // Xử lý sự kiện lưu thông tin
    $('#saveProfileBtn').click(function () {
        var formData = {
            userId: $('#UserId').val(),
            firstName: $('#FirstName').val(),
            lastName: $('#LastName').val(),
            nickname: $('#Nickname').val(),
            email: $('#Email').val(),
            phoneNumber: $('#PhoneNumber').val(),
            address: $('#Address').val()
        };

        $.ajax({
            url: '/User/UpdateProfile',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message || 'Có lỗi xảy ra khi cập nhật');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                console.error('Response:', xhr.responseText);
                toastr.error('Đã xảy ra lỗi khi cập nhật thông tin');
            }
        });
    });

    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "timeOut": "3000"
    };
    $('#changePasswordBtn').click(function () {
        var formData = {
            currentPassword: $('#CurrentPassword').val(),
            newPassword: $('#NewPassword').val(),
            confirmPassword: $('#ConfirmPassword').val()
        };

        $.ajax({
            url: '/User/ChangePassword',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    Swal.fire('Thành công', response.message, 'success');
                    $('#changePasswordModal').modal('hide');
                    // Reset form
                    $('#changePasswordForm')[0].reset();
                } else {
                    Swal.fire('Lỗi', response.message, 'error');
                }
            }
        });
    });
});

