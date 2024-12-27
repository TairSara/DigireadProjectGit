document.addEventListener("DOMContentLoaded", () => {
    console.log("DigiRead page loaded!");
    const welcomeSection = document.querySelector('.welcome-section');
    if (welcomeSection) {
        welcomeSection.style.opacity = '0';
        welcomeSection.style.transition = 'opacity 0.5s ease-in';
        setTimeout(() => {
            welcomeSection.style.opacity = '1';
        }, 500);
    }
});