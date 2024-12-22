$(document).ready(function () {
    // Image preview handler
    $('#ImageSrc').on('change', function () {
        var imgUrl = $(this).val();
        if (imgUrl) {
            var img = $('<img>', {
                src: imgUrl,
                alt: 'תצוגה מקדימה של התמונה',
                class: 'img-thumbnail mt-2',
                style: 'max-width: 200px'
            });
            img.on('error', function () {
                alert('לא ניתן לטעון את התמונה מה-URL שהוזן');
                $(this).remove();
            });
            var previewDiv = $('#imagePreview');
            if (previewDiv.length === 0) {
                previewDiv = $('<div>', { id: 'imagePreview' }).insertAfter('#currentImage');
            }
            previewDiv.html(img);
        }
    });

    // Form submission handler
    $('#editBookForm').on('submit', function (e) {
        e.preventDefault();
        var formData = {
            BookID: $('#BookID').val(),
            Title: $('#Title').val(),
            MainAuthor: $('#MainAuthor').val(),
            Publisher: $('#Publisher').val(),
            PublishYear: $('#PublishYear').val(),
            RentalPrice: $('#RentalPrice').val(),
            PurchasePrice: $('#PurchasePrice').val(),
            AgeRestriction: $('#AgeRestriction').val(),
            Genre: $('#Genre').val(),
            OriginalPrice: $('#OriginalPrice').val(),
            DiscountEndDate: $('#DiscountEndDate').val(),
            StockQuantity: $('#StockQuantity').val(),
            ImageSrc: $('#ImageSrc').val(),
            Description: $('#Description').val(),
            StockQuantityRent: $('#StockQuantityRent').val(),
            IsAvailable: $('#IsAvailable').is(':checked'),
            IsForRent: $('#IsForRent').is(':checked'),
            IsEPUBAvailable: $('#IsEPUBAvailable').is(':checked'),
            IsF2BAvailable: $('#IsF2BAvailable').is(':checked'),
            IsMobiAvailable: $('#IsMobiAvailable').is(':checked'),
            IsPDFAvailable: $('#IsPDFAvailable').is(':checked')
        };

        // Add anti-forgery token
        formData["__RequestVerificationToken"] = $('input[name="__RequestVerificationToken"]').val();

        // Show loading indicator
        var submitButton = $(this).find('button[type="submit"]');
        var originalButtonText = submitButton.text();
        submitButton.prop('disabled', true).text('מעדכן...');

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert('הספר עודכן בהצלחה!');
                    // שימוש בנתיב ישיר
                    window.location.href = '/BookManagement/ManageBooks';
                } else {
                    alert(response.message || 'אירעה שגיאה בעדכון הספר');
                    submitButton.prop('disabled', false).text(originalButtonText);
                }
            },
            error: function (xhr, status, error) {
                alert('אירעה שגיאה בשליחת הטופס: ' + error);
                submitButton.prop('disabled', false).text(originalButtonText);
            }
        });
    });

    // Toggle StockQuantity based on IsAvailable
    $('#IsAvailable').on('change', function () {
        var stockQuantityInput = $('#StockQuantity');
        if (!$(this).is(':checked')) {
            stockQuantityInput.val(0);
            stockQuantityInput.prop('readonly', true);
        } else {
            stockQuantityInput.prop('readonly', false);
        }
    });

    // Toggle StockQuantityRent based on IsForRent
    $('#IsForRent').on('change', function () {
        var stockQuantityRentInput = $('#StockQuantityRent');
        if (!$(this).is(':checked')) {
            stockQuantityRentInput.val(0);
            stockQuantityRentInput.prop('readonly', true);
        } else {
            stockQuantityRentInput.prop('readonly', false);
        }
    });

    // Initialize readonly states
    if (!$('#IsAvailable').is(':checked')) {
        $('#StockQuantity').prop('readonly', true);
    }
    if (!$('#IsForRent').is(':checked')) {
        $('#StockQuantityRent').prop('readonly', true);
    }
});