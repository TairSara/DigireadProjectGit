class AdminDashboard {
    constructor() {
        this.initializeEventListeners();
        this.initializeAnimations();
        this.initializeStatistics();
    }

    initializeEventListeners() {
        // Add click tracking for cards
        document.querySelectorAll('.dashboard-card').forEach(card => {
            card.addEventListener('click', (e) => this.trackCardClick(e));
            card.addEventListener('mouseenter', (e) => this.animateCardEnter(e));
            card.addEventListener('mouseleave', (e) => this.animateCardLeave(e));
        });

        // Add hover statistics update
        document.querySelectorAll('.stat-card').forEach(stat => {
            stat.addEventListener('mouseenter', () => this.updateStatisticValue(stat));
        });
    }

    initializeAnimations() {
        // Add entrance animations for cards
        const cards = document.querySelectorAll('.dashboard-card, .stat-card');
        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            setTimeout(() => {
                card.style.transition = 'all 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 100 * index);
        });
    }

    animateCardEnter(event) {
        const card = event.currentTarget;
        const icon = card.querySelector('.card-icon');
        if (icon) {
            icon.style.transform = 'scale(1.1) rotate(5deg)';
        }
    }

    animateCardLeave(event) {
        const card = event.currentTarget;
        const icon = card.querySelector('.card-icon');
        if (icon) {
            icon.style.transform = 'scale(1) rotate(0)';
        }
    }

    initializeStatistics() {
        // Initialize real-time statistics updates
        this.startStatisticsRefresh();
        this.animateStatistics();
    }

    animateStatistics() {
        document.querySelectorAll('.stat-value').forEach(statValue => {
            const finalValue = parseInt(statValue.textContent);
            let currentValue = 0;
            const duration = 2000; // 2 seconds
            const steps = 60;
            const increment = finalValue / steps;
            const stepTime = duration / steps;

            const updateValue = () => {
                currentValue = Math.min(currentValue + increment, finalValue);
                statValue.textContent = Math.round(currentValue);
                if (currentValue < finalValue) {
                    setTimeout(updateValue, stepTime);
                }
            };

            updateValue();
        });
    }

    trackCardClick(event) {
        const cardTitle = event.currentTarget.querySelector('.card-title').textContent;
        console.log(`Card clicked: ${cardTitle}`);
        // Add analytics tracking here
    }

    updateStatisticValue(statCard) {
        const valueElement = statCard.querySelector('.stat-value');
        if (valueElement) {
            this.fetchLatestStatistic(valueElement.dataset.statType)
                .then(newValue => {
                    if (newValue !== null) {
                        this.animateValueChange(valueElement, newValue);
                    }
                });
        }
    }

    animateValueChange(element, newValue) {
        const oldValue = parseInt(element.textContent);
        const difference = newValue - oldValue;
        const steps = 30;
        const increment = difference / steps;
        let currentStep = 0;

        const animation = setInterval(() => {
            currentStep++;
            const currentValue = oldValue + (increment * currentStep);
            element.textContent = Math.round(currentValue);

            if (currentStep >= steps) {
                clearInterval(animation);
                element.textContent = newValue;
            }
        }, 20);
    }

    startStatisticsRefresh() {
        // Refresh statistics every 5 minutes
        setInterval(() => {
            document.querySelectorAll('.stat-value').forEach(stat => {
                this.updateStatisticValue(stat.closest('.stat-card'));
            });
        }, 300000); // 5 minutes
    }

    async fetchLatestStatistic(statType) {
        try {
            const response = await fetch(`/api/statistics/${statType}`);
            if (response.ok) {
                const data = await response.json();
                return data.value;
            }
        } catch (error) {
            console.error('Error fetching statistic:', error);
        }
        return null;
    }
}

// Initialize dashboard when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new AdminDashboard();
});