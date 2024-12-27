function confirmDelete(bookId, type, title) {
    Swal.fire({
        title: 'האם אתה בטוח?',
        html: `האם אתה בטוח שברצונך למחוק את הספר <strong>${title}</strong> מהספרייה שלך?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'כן, מחק',
        cancelButtonText: 'ביטול',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            deleteBook(bookId, type);
        }
    });
}

function deleteBook(bookId, type) {
    console.log('מנסה למחוק ספר:', { bookId, type }); // הוספת לוג

    $.ajax({
        url: '/BookManagement/DeleteFromLibrary',
        type: 'POST',
        data: {
            bookId: bookId,
            type: type,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            console.log('תגובת השרת:', response); // הוספת לוג
            if (response.success) {
                Swal.fire({
                    title: 'נמחק!',
                    text: 'הספר נמחק בהצלחה מהספרייה שלך',
                    icon: 'success'
                }).then(() => {
                    location.reload();
                });
            } else {
                console.error('שגיאת שרת:', response.message); // הוספת לוג
                Swal.fire(
                    'שגיאה!',
                    response.message || 'אירעה שגיאה במחיקת הספר',
                    'error'
                );
            }
        },
        error: function(xhr, status, error) {
            console.error('שגיאת AJAX:', { status, error, responseText: xhr.responseText }); // הוספת לוג מפורט
            Swal.fire(
                'שגיאה!',
                'אירעה שגיאה במחיקת הספר',
                'error'
            );
        }
    });
}

function showFormatDialog(bookId) {
    Swal.fire({
        title: 'בחר פורמט להורדה',
        html: `
                    <div class="format-options" dir="rtl">
                        <button class="format-btn" onclick="downloadBook(${bookId}, 'PDF')">PDF</button>
                        <button class="format-btn" onclick="downloadBook(${bookId}, 'EPUB')">EPUB</button>
                        <button class="format-btn" onclick="downloadBook(${bookId}, 'MOBI')">MOBI</button>
                        <button class="format-btn" onclick="downloadBook(${bookId}, 'FB2')">FB2</button>
                    </div>
                `,
        showConfirmButton: false,
        showCloseButton: true,
        customClass: {
            popup: 'format-dialog'
        }
    });
}

function downloadBook(bookId, format) {
    $.ajax({
        url: '/BookManagement/DownloadBook',
        type: 'POST',
        data: {
            bookId: bookId,
            format: format,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            console.log('Download response:', response);
            if (response.success) {
                // Create a temporary link to download the file
                const link = document.createElement('a');
                link.href = response.downloadUrl;
                link.download = response.fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

                Swal.close();
            } else {
                Swal.fire({
                    title: 'שגיאה!',
                    text: response.message || 'אירעה שגיאה בהורדת הספר',
                    icon: 'error'
                });
            }
        },
        error: function() {
            console.error('Download error:', { status, error, response: xhr.responseText });
            Swal.fire({
                title: 'שגיאה!',
                text: 'אירעה שגיאה בהורדת הספר',
                icon: 'error'
            });
        }
    });
}


function showRatingDialog(bookId, title) {
    let currentRating = 0;

    Swal.fire({
        title: `דרג את הספר "${title}"`,
        html: `
            <div class="rating-stars" dir="ltr">
                <span class="star" data-rating="1">★</span>
                <span class="star" data-rating="2">★</span>
                <span class="star" data-rating="3">★</span>
                <span class="star" data-rating="4">★</span>
                <span class="star" data-rating="5">★</span>
            </div>
            <textarea id="reviewText" class="swal2-textarea" placeholder="הוסף ביקורת (אופציונלי)" dir="rtl"></textarea>
        `,
        showCancelButton: true,
        confirmButtonText: 'שלח דירוג',
        cancelButtonText: 'ביטול',
        reverseButtons: true,
        didOpen: () => {
            const stars = document.querySelectorAll('.rating-stars .star');
            stars.forEach(star => {
                star.addEventListener('click', function() {
                    const rating = this.dataset.rating;
                    currentRating = rating;
                    stars.forEach(s => {
                        s.classList.remove('active');
                        if (s.dataset.rating <= rating) {
                            s.classList.add('active');
                        }
                    });
                });
            });
        }
    }).then((result) => {
        if (result.isConfirmed) {
            if (currentRating === 0) {
                Swal.fire('שגיאה', 'אנא בחר דירוג', 'error');
                return;
            }
            submitRating(bookId, currentRating, document.getElementById('reviewText').value);
        }
    });
}

function submitRating(bookId, rating, reviewText) {
    $.ajax({
        url: '/Reviews/AddBookReview',
        type: 'POST',
        data: {
            bookId: bookId,
            rating: rating,
            reviewText: reviewText,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                Swal.fire({
                    title: 'תודה!',
                    text: 'הדירוג נשמר בהצלחה',
                    icon: 'success'
                });
            } else {
                Swal.fire(
                    'שגיאה!',
                    response.message || 'אירעה שגיאה בשמירת הדירוג',
                    'error'
                );
            }
        },
        error: function(xhr, status, error) {
            console.error('שגיאת AJAX:', { status, error, responseText: xhr.responseText });
            Swal.fire(
                'שגיאה!',
                'אירעה שגיאה בשמירת הדירוג',
                'error'
            );
        }
    });
}