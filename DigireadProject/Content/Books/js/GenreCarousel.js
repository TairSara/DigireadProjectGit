function scrollCarousel(direction) {
    const carousel = document.getElementById('genresCarousel');
    const containerWidth = carousel.clientWidth;
    const itemWidth = carousel.querySelector('.genre-container').offsetWidth + 10; // רוחב הפריט + מרווח
    const scrollAmount = containerWidth; // גלילה בגודל הקונטיינר

    if (direction === 'right') {
        carousel.scrollLeft -= scrollAmount;
    } else {
        carousel.scrollLeft += scrollAmount;
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const carousel = document.getElementById('genresCarousel');
    carousel.scrollLeft = 0;
});