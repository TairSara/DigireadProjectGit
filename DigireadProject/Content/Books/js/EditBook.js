$(document).ready(function () {
    // Image preview handler
    $('#ImageSrc').on('change', function() {
        var imgUrl = $(this).val();
        if (imgUrl) {
            var img = $('<img>', {
                src: imgUrl,
                alt: 'תצוגה מקדימה של התמונה',
                class: 'img-thumbnail mt-2',
                style: 'max-width: 200px'
            });

            img.on('error', function() {
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
            Description: $('#Description').val(), // הוספתי את התיאור לטופס
            StockQuantityRent: $('#StockQuantityRent').val(),

            // Boolean fields
            IsAvailable: $('#IsAvailable').is(':checked'),
            IsForRent: $('#IsForRent').is(':checked'),
            IsEPUBAvailable: $('#IsEPUBAvailable').is(':checked'),
            IsF2BAvailable: $('#IsF2BAvailable').is(':checked'),
            IsMobiAvailable: $('#IsMobiAvailable').is(':checked'),
            IsPDFAvailable: $('#IsPDFAvailable').is(':checked')
        };

        // Add anti-forgery token
        formData["__RequestVerificationToken"] = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert('הספר עודכן בהצלחה!');
                    window.location.href = '@Url.Action("ManageBooks", "BookManagement")';
                } else {
                    alert(response.message || 'אירעה שגיאה בעדכון הספר');
                }
            },
            error: function () {
                alert('אירעה שגיאה בשליחת הטופס');
            }
        });
    });
});