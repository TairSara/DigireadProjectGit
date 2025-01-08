function scrollCarousel(direction) {
    const carousel = document.getElementById('genresCarousel');
    const containerWidth = carousel.clientWidth;
    const itemWidth = carousel.querySelector('.genre-container').offsetWidth + 10; 
    const scrollAmount = containerWidth;

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