function filterBooks() {
    const searchInput = document.getElementById('searchInput').value.toLowerCase();
    const saleOnly = document.getElementById('saleOnly').checked;
    const books = document.querySelectorAll('.book-item');

    books.forEach(book => {
        const title = book.getAttribute('data-title').toLowerCase();
        const author = book.getAttribute('data-author').toLowerCase();
        const publisher = book.getAttribute('data-publisher').toLowerCase();
        
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
        return; 
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

    container.innerHTML = '';
    books.forEach(book => container.appendChild(book));
}
function sortBooks() {
    const sortBy = document.getElementById('sortSelect').value;
    const booksContainer = document.getElementById('booksContainer');
    const books = Array.from(booksContainer.getElementsByClassName('book-item'));

    if (!sortBy) {
        books.sort((a, b) => {
            return parseInt(a.getAttribute('data-db-order')) - parseInt(b.getAttribute('data-db-order'));
        });
    } else {
        books.sort((a, b) => {
            switch (sortBy) {
                case 'title':
                    const titleA = a.getAttribute('data-title').trim().replace(/^['"](.*?)['"]$/, '$1');
                    const titleB = b.getAttribute('data-title').trim().replace(/^['"](.*?)['"]$/, '$1');

                    const numA = titleA.match(/^\d+/);
                    const numB = titleB.match(/^\d+/);

                    if (numA && numB) {
                        return parseInt(numA[0]) - parseInt(numB[0]);
                    } else if (numA) {
                        return -1;
                    } else if (numB) {
                        return 1;
                    }

                    return titleA.localeCompare(titleB, 'he', {
                        sensitivity: 'base',
                        ignorePunctuation: true
                    });

                case 'popularity':
                    const ratingA = parseFloat(a.getAttribute('data-rating')) || 0;
                    const ratingB = parseFloat(b.getAttribute('data-rating')) || 0;
                    if (ratingA !== ratingB) return ratingB - ratingA;

                    const reviewCountA = parseInt(a.getAttribute('data-review-count')) || 0;
                    const reviewCountB = parseInt(b.getAttribute('data-review-count')) || 0;
                    return reviewCountB - reviewCountA;

                case 'priceAsc':
                    return parseFloat(a.getAttribute('data-price')) - parseFloat(b.getAttribute('data-price'));

                case 'priceDesc':
                    return parseFloat(b.getAttribute('data-price')) - parseFloat(a.getAttribute('data-price'));

                case 'year':
                    return parseInt(b.getAttribute('data-year')) - parseInt(a.getAttribute('data-year'));

                case 'author':
                    return a.getAttribute('data-author')
                        .localeCompare(b.getAttribute('data-author'), 'he', {
                            sensitivity: 'base',
                            ignorePunctuation: true
                        });

                default:
                    return 0;
            }
        });
    }

    while (booksContainer.firstChild) {
        booksContainer.removeChild(booksContainer.firstChild);
    }
    books.forEach(book => booksContainer.appendChild(book));
}

document.addEventListener('DOMContentLoaded', function() {
    const sortSelect = document.getElementById('sortSelect');
    sortSelect.value = '';
});


