$(document).ready(function () {
    var bookIdToDelete = null;

    // Attach event handlers
    window.confirmDelete = function (bookId) {
        bookIdToDelete = bookId;
        $('#deleteModal').modal('show');
    };

    $('#confirmDeleteBtn').on('click', function () {
        if (bookIdToDelete !== null) {
            $.ajax({
                type: 'POST',
                url: window.deleteBookUrl, // Define this in the view or a global script
                data: { id: bookIdToDelete },
                success: function (response) {
                    if (response.success) {
                        $('#book-' + bookIdToDelete).remove();
                        $('#deleteModal').modal('hide');
                    } else {
                        alert(response.message);
                    }
                },
                error: function () {
                    alert('שגיאה במחיקת הספר');
                }
            });
        }
    });

    $('#cancelDeleteBtn').on('click', function () {
        $('#deleteModal').modal('hide');
    });
});