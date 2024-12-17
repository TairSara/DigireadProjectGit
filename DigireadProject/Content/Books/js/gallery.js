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

    // עדכון הכפתור הפעיל
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