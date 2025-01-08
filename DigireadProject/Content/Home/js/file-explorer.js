document.addEventListener("DOMContentLoaded", () => {
    console.log("DigiRead page loaded!");

    const createBookModal = () => {
        const modal = document.createElement('div');
        modal.className = 'book-preview-modal';
        modal.innerHTML = `
        <div class="preview-container">
            <div class="preview-content">
                <button class="close-preview" aria-label="סגור">&times;</button>
                <div class="preview-image">
                    <img src="" alt="תמונת ספר" />
                </div>
                <div class="preview-details">
                    <h3 class="preview-title"></h3>
                    <p class="preview-author"></p>
                    <div class="preview-rating">
                        <div class="stars"></div>
                        <span class="average-rating"></span>
                    </div>
                    <div class="preview-description scrollable-description"></div>
                    <a href="#" class="book-details-btn">לדף הספר</a>
                </div>
            </div>
        </div>
        `;

        const style = document.createElement('style');
        style.textContent = `
        .book-preview-modal {
            display: none;
            position: fixed;
            z-index: 1000;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            overflow: auto;
            background-color: rgba(0,0,0,0.6);
            opacity: 0;
            transition: opacity 0.3s ease;
            direction: rtl;
        }

        .preview-container {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100%;
            padding: 20px;
        }

        .preview-content {
            background-color: white;
            border-radius: 10px;
            max-width: 800px;
            width: 90%;
            max-height: 80vh;
            display: flex;
            position: relative;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            transform: translateY(30px);
            transition: transform 0.3s ease;
        }

        .preview-image {
            width: 40%;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .preview-image img {
            max-width: 100%;
            max-height: 500px;
            object-fit: contain;
        }

        .preview-details {
            width: 60%;
            padding: 20px;
            overflow-y: auto;
        }

        .close-preview {
            position: absolute;
            top: 10px;
            left: 10px;
            background: none;
            border: none;
            font-size: 30px;
            cursor: pointer;
            color: #666;
            transition: color 0.3s ease;
        }

        .close-preview:hover {
            color: #000;
        }

        .scrollable-description {
            max-height: 200px;
            overflow-y: auto;
            padding-right: 50px;
            margin-bottom: 15px;
        }

        .book-details-btn {
            display: inline-block;
            background-color:  #8154a8;
            color: white;
            text-decoration: none;
            padding: 10px 20px;
            border-radius: 5px;
            transition: background-color 0.3s ease;
        }

        .book-details-btn:hover {
            background-color: #8154a8;
        }

        .preview-rating {
            display: flex;
            align-items: center;
            margin-bottom: 15px;
        }

        .stars {
            margin-left: 10px;
        }
        `;
        document.head.appendChild(style);

        document.body.appendChild(modal);
        return modal;
    };

    const modal = createBookModal();

    const closeModal = () => {
        modal.style.opacity = '0';
        modal.querySelector('.preview-content').style.transform = 'translateY(30px)';
        setTimeout(() => {
            modal.style.display = 'none';
        }, 300);
    };

    modal.querySelector('.close-preview').addEventListener('click', closeModal);
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeModal();
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.style.display === 'flex') {
            closeModal();
        }
    });

    function showBookDetails(card) {
        const bookData = {
            image: card.querySelector('.book-image img').src,
            title: card.querySelector('.book-title').textContent,
            author: card.querySelector('.book-author').textContent,
            description: card.querySelector('.tooltip-text').textContent,
            rating: card.querySelector('.average-rating')?.textContent,
            stars: card.querySelector('.stars')?.innerHTML,
            link: card.querySelector('.book-details-link')?.href || '#'
        };

        modal.querySelector('.preview-image img').src = bookData.image;
        modal.querySelector('.preview-title').textContent = bookData.title;
        modal.querySelector('.preview-author').textContent = bookData.author;
        modal.querySelector('.scrollable-description').textContent = bookData.description;

        const previewRating = modal.querySelector('.preview-rating');
        if (bookData.rating && bookData.stars) {
            previewRating.style.display = 'flex';
            modal.querySelector('.stars').innerHTML = bookData.stars;
            modal.querySelector('.average-rating').textContent = bookData.rating;
        } else {
            previewRating.style.display = 'none';
        }

        modal.querySelector('.book-details-btn').href = bookData.link;

        modal.style.display = 'flex';
        setTimeout(() => {
            modal.style.opacity = '1';
            modal.querySelector('.preview-content').style.transform = 'translateY(0)';
        }, 10);
    }

    document.querySelectorAll('.book-card').forEach(card => {
        card.addEventListener('click', (e) => {
            e.preventDefault();
            showBookDetails(card);
        });
    });
    class EnhancedBookCarousel {
        constructor(element) {
            this.originalContainer = element;
            this.setupContainer();
            this.books = Array.from(this.container.querySelectorAll('.book-card'));
            this.currentIndex = 0;
            this.booksToShow = this.calculateBooksToShow();
            this.isAnimating = false;
            this.autoplayInterval = null;

            this.books.reverse();
            this.books.forEach(book => {
                this.container.appendChild(book);
            });

            this.initializeCarousel();
            this.setupEventListeners();
            this.setupIntersectionObserver();
            this.startAutoplay();
        }

        setupContainer() {
            const carouselContainer = document.createElement('div');
            carouselContainer.className = 'books-carousel-container';
            carouselContainer.style.direction = 'rtl';

            this.container = document.createElement('div');
            this.container.className = 'books-carousel';
            this.container.style.direction = 'ltr';

            while (this.originalContainer.firstChild) {
                this.container.appendChild(this.originalContainer.firstChild);
            }

            carouselContainer.appendChild(this.container);
            this.originalContainer.parentNode.replaceChild(carouselContainer, this.originalContainer);
        }

        calculateBooksToShow() {
            const containerWidth = this.container.parentElement.offsetWidth - 160;
            return Math.floor(containerWidth / 360);
        }

        initializeCarousel() {
            const nav = document.createElement('div');
            nav.className = 'carousel-nav';
            nav.innerHTML = `
                <button class="prev" aria-label="ספר קודם">❮</button>
                <button class="next" aria-label="ספר הבא">❯</button>
            `;
            this.container.parentElement.appendChild(nav);

            this.books.forEach((book, index) => {
                book.style.opacity = '0';
                book.style.transform = 'translateY(20px) scale(0.95)';
                setTimeout(() => {
                    book.style.opacity = '1';
                    book.style.transform = 'translateY(0) scale(1)';
                }, index * 150);
            });

            this.container.style.transform = 'translateX(0)';
        }

        startAutoplay() {
            this.autoplayInterval = setInterval(() => {
                if (!this.isAnimating && !document.querySelector('.book-preview-modal').style.display === 'flex') {
                    this.navigate(1);
                }
            }, 5000);
        }

        stopAutoplay() {
            if (this.autoplayInterval) {
                clearInterval(this.autoplayInterval);
                this.autoplayInterval = null;
            }
        }

        setupEventListeners() {
            const nextBtn = this.container.parentElement.querySelector('.carousel-nav .next');
            const prevBtn = this.container.parentElement.querySelector('.carousel-nav .prev');

            if (nextBtn) {
                nextBtn.addEventListener('click', () => {
                    this.stopAutoplay();
                    this.navigate(1);
                });
            }

            if (prevBtn) {
                prevBtn.remove();
            }
        }

        async navigate(direction) {
            if (this.isAnimating) return;

            this.isAnimating = true;
            const containerWidth = this.container.parentElement.offsetWidth - 160;
            const scrollAmount = 360;

            const currentTransform = getComputedStyle(this.container).transform;
            const currentX = new WebKitCSSMatrix(currentTransform).m41;

            const totalWidth = this.books.length * 360;
            const maxScroll = -(totalWidth - containerWidth);

            let targetX;

            if (currentX <= maxScroll) {
                targetX = 0;
            } else {
                targetX = currentX - scrollAmount;
                if (targetX < maxScroll) {
                    targetX = maxScroll;
                }
            }

            await this.animateCarousel(currentX, targetX);
            this.isAnimating = false;
        }

        async animateCarousel(startX, endX) {
            const duration = startX === 0 || endX === 0 ? 1000 : 800;
            const startTime = performance.now();

            return new Promise(resolve => {
                const animate = (currentTime) => {
                    const elapsed = currentTime - startTime;
                    const progress = Math.min(elapsed / duration, 1);

                    const easeProgress = this.easeInOutCubic(progress);
                    const currentX = startX + (endX - startX) * easeProgress;

                    this.container.style.transform = `translateX(${currentX}px)`;

                    if (progress < 1) {
                        requestAnimationFrame(animate);
                    } else {
                        resolve();
                    }
                };

                requestAnimationFrame(animate);
            });
        }

        easeInOutCubic(t) {
            return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
        }

        setupIntersectionObserver() {
            const options = {
                root: null,
                threshold: 0.1
            };

            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('visible');
                        if (entry.target.classList.contains('book-card')) {
                            const img = entry.target.querySelector('img');
                            if (img) img.style.transform = 'scale(1.02)';
                        }
                    }
                });
            }, options);

            this.books.forEach(book => observer.observe(book));
        }
    }

    const popularBooksGrid = document.querySelector('.popular-books-section .books-grid');
    const saleBooksGrid = document.querySelector('.sale-books-section .books-grid');

    if (popularBooksGrid) {
        const popularCarousel = new EnhancedBookCarousel(popularBooksGrid);

        popularBooksGrid.querySelectorAll('.book-card').forEach(card => {
            card.addEventListener('click', (e) => {
                e.preventDefault();
                showBookDetails(card);
            });
        });
    }

    if (saleBooksGrid) {
        const saleCarousel = new EnhancedBookCarousel(saleBooksGrid);

        saleBooksGrid.querySelectorAll('.book-card').forEach(card => {
            card.addEventListener('click', (e) => {
                e.preventDefault();
                showBookDetails(card);
            });
        });
    }

});

