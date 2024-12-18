$(document).ready(function () {
    $("#updateProfileForm").submit(function (e) {
        e.preventDefault();
        const form = $(this);
        const url = form.attr("action");
        const data = form.serialize();

        $.post(url, data)
            .done(function (response) {
                if (response.success) {
                    Swal.fire({
                        title: "הצלחה!",
                        text: response.message,
                        icon: "success",
                        confirmButtonText: "אישור"
                    });
                } else {
                    Swal.fire({
                        title: "שגיאה!",
                        text: response.message,
                        icon: "error",
                        confirmButtonText: "סגור"
                    });
                }
            })
            .fail(function () {
                Swal.fire({
                    title: "שגיאה!",
                    text: "אירעה שגיאה במערכת. אנא נסה שנית מאוחר יותר.",
                    icon: "error",
                    confirmButtonText: "סגור"
                });
            });
    });
});
