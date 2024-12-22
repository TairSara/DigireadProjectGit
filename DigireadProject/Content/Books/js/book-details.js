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

            const isWaitList = form.action.includes('AddToWaitList');

            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            });

            const result = await response.json();

            if (result.success) {
                this.closePurchaseDialog();
                Swal.fire({
                    title: 'הצלחה!',
                    text: isWaitList ? 'נוספת בהצלחה לרשימת ההמתנה' : 'הספר נוסף לסל הקניות בהצלחה',
                    icon: 'success',
                    confirmButtonText: 'אישור'
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = isWaitList ? '/BookManagement/MyWaitList' : '/BookManagement/Gallery';
                    }
                });
            } else {
                if (result.isRentalLimit) {
                    this.closePurchaseDialog();
                    Swal.fire({
                        title: 'לא ניתן להוסיף לסל הקניות',
                        html: `
                        <div class="text-center">
                            <div class="mb-4">
                                <i class="fas fa-exclamation-circle text-warning" style="font-size: 48px;"></i>
                            </div>
                            <div class="mb-3" style="font-size: 16px;">
                                <strong>שים לב!</strong>
                            </div>
                            <div class="mb-2" style="font-size: 16px;">
                                הגעת למגבלת ההשאלות המקסימלית.
                            </div>
                            <div style="font-size: 16px;">
                                ניתן להשאיל עד 3 ספרים במקביל.
                            </div>
                        </div>
                    `,
                        icon: 'warning',
                        confirmButtonText: 'הבנתי',
                        confirmButtonColor: '#3085d6'
                    });
                } else {
                    Swal.fire({
                        title: 'שגיאה',
                        text: result.message || 'אירעה שגיאה בהוספת הספר לסל',
                        icon: 'error',
                        confirmButtonText: 'אישור'
                    });
                }
            }
        } catch (error) {
            console.error('Error:', error);
            Swal.fire({
                title: 'שגיאה',
                text: 'אירעה שגיאה בפעולה',
                icon: 'error',
                confirmButtonText: 'אישור'
            });
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

var bookManager = {
    showPurchaseOptions: function() {
        document.getElementById('purchaseDialog').style.display = 'block';
    },

    closePurchaseDialog: function() {
        document.getElementById('purchaseDialog').style.display = 'none';
    },

    showNoStockMessage: function() {
        document.getElementById('errorModal').style.display = 'block';
    },

    closeErrorModal: function() {
        document.getElementById('errorModal').style.display = 'none';
    }
};