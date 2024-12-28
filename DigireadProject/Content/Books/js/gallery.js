function filterBooks() {
    const searchInput = document.getElementById('searchInput').value.toLowerCase();
    const saleOnly = document.getElementById('saleOnly').checked;
    const books = document.querySelectorAll('.book-item');

    books.forEach(book => {
        const title = book.getAttribute('data-title').toLowerCase();
        const author = book.getAttribute('data-author').toLowerCase();
        const publisher = book.getAttribute('data-publisher').toLowerCase();

        // בדוק אם יש תג מבצע בספר הספציפי
        const hasSaleBadge = book.querySelector('.sale-badge') !== null;

        const matchesSearch = searchInput === '' ||
            title.includes(searchInput) ||
            author.includes(searchInput) ||
            publisher.includes(searchInput);

        const matchesSale = !saleOnly || hasSaleBadge;

        book.style.display = (matchesSearch && matchesSale) ? '' : 'none';
    });
}

function filterByGenre(genre) {
    const books = document.querySelectorAll('.book-item');
    const buttons = document.querySelectorAll('.genre-button');

    // עדכון הכפתור הפעילש
    buttons.forEach(button => {
        if (button.textContent === genre || (button.textContent === 'הכל' && genre === 'all')) {
            button.classList.add('active');
        } else {
            button.classList.remove('active');
        }
    });

    books.forEach(book => {
        const bookGenre = book.getAttribute('data-genre');
        book.style.display = (genre === 'all' || bookGenre === genre) ? '' : 'none';
    });
}

function sortBooks() {
    const select = document.getElementById('sortSelect');
    const value = select.value;

    if (!value) {
        return; // אם לא נבחרה קטגוריה, לא עושים כלום
    }

    const books = Array.from(document.querySelectorAll('.book-item'));
    const container = document.getElementById('booksContainer');

    books.sort((a, b) => {
        switch (value) {
            case 'priceAsc':
                return parseFloat(a.getAttribute('data-price')) - parseFloat(b.getAttribute('data-price'));
            case 'priceDesc':
                return parseFloat(b.getAttribute('data-price')) - parseFloat(a.getAttribute('data-price'));
            case 'popularity':
                return parseFloat(b.getAttribute('data-popularity') || 0) - parseFloat(a.getAttribute('data-popularity') || 0);
            case 'year':
                return parseInt(b.getAttribute('data-year')) - parseInt(a.getAttribute('data-year'));
            default:
                const aValue = a.getAttribute('data-' + value).toLowerCase();
                const bValue = b.getAttribute('data-' + value).toLowerCase();
                return aValue.localeCompare(bValue);
        }
    });

    // נקה את הקונטיינר וסדר מחדש
    container.innerHTML = '';
    books.forEach(book => container.appendChild(book));
}
function sortBooks() {
    const sortBy = document.getElementById('sortSelect').value;
    const booksContainer = document.getElementById('booksContainer');
    const books = Array.from(booksContainer.getElementsByClassName('book-item'));

    books.sort((a, b) => {
        switch (sortBy) {
            case 'popularity':
                // מיון לפי דירוג וכמות ביקורות
                const ratingA = parseFloat(a.dataset.rating) || 0;
                const ratingB = parseFloat(b.dataset.rating) || 0;
                const reviewsA = parseInt(a.dataset.reviews) || 0;
                const reviewsB = parseInt(b.dataset.reviews) || 0;

                // נוסחה המשקללת דירוג וכמות ביקורות
                const popularityA = (ratingA * 0.7) + ((reviewsA / Math.max(reviewsA, reviewsB)) * 0.3 * 5);
                const popularityB = (ratingB * 0.7) + ((reviewsB / Math.max(reviewsA, reviewsB)) * 0.3 * 5);
                return popularityB - popularityA;

            case 'title':
                return a.dataset.title.localeCompare(b.dataset.title, 'he');

            case 'priceAsc':
                return parseFloat(a.dataset.price) - parseFloat(b.dataset.price);

            case 'priceDesc':
                return parseFloat(b.dataset.price) - parseFloat(a.dataset.price);

            case 'year':
                return parseInt(b.dataset.year) - parseInt(a.dataset.year);

            case 'author':
                return a.dataset.author.localeCompare(b.dataset.author, 'he');

            default:
                return 0;
        }
    });

    // סידור מחדש של האלמנטים
    books.forEach(book => booksContainer.appendChild(book));
}

// הוספת טעינת ברירת מחדל למיון לפי פופולריות
document.addEventListener('DOMContentLoaded', function() {
    const sortSelect = document.getElementById('sortSelect');
    // קבע את ברירת המחדל לפופולריות
    sortSelect.value = 'popularity';
    // הפעל את המיון
    sortBooks();
});


