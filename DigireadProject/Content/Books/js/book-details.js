class BookPurchaseManager {
    constructor() {
        this.modal = document.getElementById('purchaseDialog');
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        // Close modal when clicking outside
        window.onclick = (event) => {
            if (event.target == this.modal) {
                this.closePurchaseDialog();
            }
        };

        // Add submit event listeners to forms
        const purchaseForms = document.querySelectorAll('.purchase-options form');
        purchaseForms.forEach(form => {
            form.addEventListener('submit', (e) => {
                e.preventDefault();
                const isRental = form.querySelector('[name="isRental"]').value === 'true';
                this.handleSubmit(form, isRental);
            });
        });
    }

    showPurchaseOptions() {
        this.modal.style.display = 'block';
    }

    closePurchaseDialog() {
        this.modal.style.display = 'none';
    }

    async handleSubmit(form, isRental) {
        try {
            const formData = new FormData(form);
            const token = form.querySelector('[name="__RequestVerificationToken"]').value;

            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            });

            if (response.ok) {
                this.closePurchaseDialog();
                this.showSuccessMessage();

                // Wait for 1 second before redirecting
                setTimeout(() => {
                    window.location.href = '/BookManagement/Gallery';
                }, 1000);
            } else {
                this.showErrorMessage();
            }
        } catch (error) {
            console.error('Error:', error);
            this.showErrorMessage();
        }
    }

    showSuccessMessage() {
        const successMessage = document.createElement('div');
        successMessage.className = 'alert alert-success';
        successMessage.innerHTML = `
            <i class="fas fa-check-circle"></i>
            הספר נוסף בהצלחה לסל הקניות
        `;

        document.body.appendChild(successMessage);

        setTimeout(() => {
            successMessage.remove();
        }, 3000);
    }

    showErrorMessage() {
        const errorMessage = document.createElement('div');
        errorMessage.className = 'alert alert-danger';
        errorMessage.innerHTML = `
            <i class="fas fa-exclamation-circle"></i>
            אירעה שגיאה בהוספת הספר לסל
        `;

        document.body.appendChild(errorMessage);

        setTimeout(() => {
            errorMessage.remove();
        }, 3000);
    }
}

// Initialize when document is ready
document.addEventListener('DOMContentLoaded', () => {
    window.bookManager = new BookPurchaseManager();
});