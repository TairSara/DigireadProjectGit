class AdminDashboard {
    constructor() {
        this.initializeEventListeners();
        this.initializeStatistics();
    }

    initializeEventListeners() {
        // Add click tracking for cards
        document.querySelectorAll('.dashboard-card').forEach(card => {
            card.addEventListener('click', (e) => this.trackCardClick(e));
        });

        // Add hover statistics update
        document.querySelectorAll('.stat-card').forEach(stat => {
            stat.addEventListener('mouseenter', () => this.updateStatisticValue(stat));
        });
    }

    initializeStatistics() {
        // Initialize real-time statistics updates
        this.startStatisticsRefresh();
    }

    trackCardClick(event) {
        const cardTitle = event.currentTarget.querySelector('.card-title').textContent;
        console.log(`Card clicked: ${cardTitle}`);
        // Add analytics tracking here
    }

    updateStatisticValue(statCard) {
        // Add real-time statistics update logic here
        const valueElement = statCard.querySelector('.stat-value');
        if (valueElement) {
            // Example of how to update values in real-time
            // You can replace this with actual API calls
            this.fetchLatestStatistic(valueElement.dataset.statType)
                .then(newValue => {
                    valueElement.textContent = newValue;
                });
        }
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
        // Placeholder for API call
        // Replace with actual API endpoint
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