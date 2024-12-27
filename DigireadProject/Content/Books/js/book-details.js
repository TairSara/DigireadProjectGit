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

    showNoStockMessage() {
        document.getElementById('errorModal').style.display = 'block';
    }

    closeErrorModal() {
        document.getElementById('errorModal').style.display = 'none';
    }

    async quickPurchase(bookId, isRental) {
        try {
            // מצא את הכפתור ושמור את הטקסט המקורי
            const button = document.querySelector('.btn-quick-purchase');
            const originalText = button.innerHTML;

            // הצג מצב טעינה
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> מעבד...';

            // קבל את טוקן האבטחה
            const token = document.querySelector('[name="__RequestVerificationToken"]').value;

            // קודם נוסיף לעגלה
            const addToCartResponse = await fetch('/ShoppingCart/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams({
                    bookId: bookId,
                    isRental: isRental,
                    __RequestVerificationToken: token
                })
            });

            const addToCartResult = await addToCartResponse.json();

            if (addToCartResult.success) {
                // אם ההוספה לעגלה הצליחה, נעבור לדף הקופה
                window.location.href = '/Order/QuickPurchase';
            } else {
                // הצג הודעת שגיאה
                Swal.fire({
                    title: 'שגיאה',
                    text: addToCartResult.message || 'אירעה שגיאה בתהליך הרכישה',
                    icon: 'error',
                    confirmButtonText: 'אישור'
                });
                // החזר את הכפתור למצב הרגיל
                button.disabled = false;
                button.innerHTML = originalText;
            }
        } catch (error) {
            console.error('Error:', error);
            Swal.fire({
                title: 'שגיאה',
                text: 'אירעה שגיאה בתהליך הרכישה',
                icon: 'error',
                confirmButtonText: 'אישור'
            });
            // החזר את הכפתור למצב הרגיל
            const button = document.querySelector('.btn-quick-purchase');
            button.disabled = false;
            button.innerHTML = '<i class="fas fa-bolt"></i> קנייה מהירה';
        }
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

// Create global instance of BookPurchaseManager
let purchaseManager = null;

// Function to initialize manager and attach methods
function initializeBookManager() {
    try {
        // Create manager instance first
        purchaseManager = new BookPurchaseManager();

        // Then attach methods to global bookManager object
        bookManager.showPurchaseOptions = function() { purchaseManager.showPurchaseOptions(); };
        bookManager.closePurchaseDialog = function() { purchaseManager.closePurchaseDialog(); };
        bookManager.showNoStockMessage = function() { purchaseManager.showNoStockMessage(); };
        bookManager.closeErrorModal = function() { purchaseManager.closeErrorModal(); };
        bookManager.quickPurchase = function(bookId, isRental) { purchaseManager.quickPurchase(bookId, isRental); };

        console.log('BookManager initialized successfully');
    } catch (error) {
        console.error('Error initializing BookManager:', error);
    }
}

// Call initialize function when DOM is loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeBookManager);
} else {
    initializeBookManager();
}