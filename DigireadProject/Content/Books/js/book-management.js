class BookManagement {
    constructor() {
        this.form = $('#addBookForm');
        this.imageInput = $('#ImageSrc');
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        this.form.on('submit', (e) => this.handleFormSubmit(e));
        this.imageInput.on('change', () => this.handleImagePreview());
    }

    handleFormSubmit(e) {
        e.preventDefault();
        const formData = new FormData(this.form[0]);

        $.ajax({
            url: this.form.attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: (response) => this.handleSubmitSuccess(response),
            error: () => this.handleSubmitError()
        });
    }

    handleSubmitSuccess(response) {
        if (response.success) {
            alert('הספר נוסף בהצלחה');
            window.location.href = '/BookManagement/ManageBooks';
        } else {
            alert('שגיאה: ' + response.message);
        }
    }

    handleSubmitError() {
        alert('אירעה שגיאה בשמירת הספר');
    }

    handleImagePreview() {
        const imgUrl = this.imageInput.val();
        if (!imgUrl) return;

        let previewDiv = $('#imagePreview');
        if (previewDiv.length === 0) {
            previewDiv = $('<div>', { id: 'imagePreview' }).insertAfter(this.imageInput);
        }

        const img = $('<img>', {
            src: imgUrl,
            alt: 'תצוגה מקדימה של התמונה',
            class: 'img-thumbnail mt-2',
            style: 'max-width: 200px'
        });

        img.on('error', function () {
            alert('לא ניתן לטעון את התמונה מה-URL שהוזן');
            $(this).remove();
        });

        previewDiv.html(img);
    }
}

// Initialize when document is ready
$(document).ready(() => {
    new BookManagement();
});