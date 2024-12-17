class PasswordValidator {
    constructor() {
        this.passwordInput = document.getElementById("newPassword");
        this.confirmInput = document.getElementById("confirmPassword");
        this.form = document.querySelector('form');
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        this.passwordInput.addEventListener("input", () => this.validatePassword());
        this.confirmInput.addEventListener("input", () => this.checkPasswordMatch());
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));
    }

    validatePassword() {
        const password = this.passwordInput.value;
        const requirements = {
            length: password.length >= 6,
            uppercase: /[A-Z]/.test(password),
            lowercase: /[a-z]/.test(password),
            number: /[0-9]/.test(password),
            special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
        };

        this.updateRequirementUI('length-req', requirements.length, 'לפחות 6 תווים');
        this.updateRequirementUI('uppercase-req', requirements.uppercase, 'אות גדולה אחת לפחות');
        this.updateRequirementUI('lowercase-req', requirements.lowercase, 'אות קטנה אחת לפחות');
        this.updateRequirementUI('number-req', requirements.number, 'מספר אחד לפחות');
        this.updateRequirementUI('special-req', requirements.special, 'תו מיוחד אחד לפחות');

        const isValid = Object.values(requirements).every(req => req);
        this.updateValidationIcon('password-validation', isValid);

        if (this.confirmInput.value !== "") {
            this.checkPasswordMatch();
        }

        return isValid;
    }

    updateRequirementUI(elementId, isValid, text) {
        const icon = isValid ? 'check' : 'times';
        const colorClass = isValid ? 'text-success' : 'text-danger';
        document.getElementById(elementId).innerHTML =
            `<i class="fas fa-${icon} ${colorClass}"></i> ${text}`;
    }

    updateValidationIcon(elementId, isValid) {
        const icon = isValid ? 'check' : 'times';
        const colorClass = isValid ? 'text-success' : 'text-danger';
        document.getElementById(elementId).innerHTML =
            `<i class="fas fa-${icon} ${colorClass}"></i>`;
    }

    checkPasswordMatch() {
        const isMatch = this.passwordInput.value === this.confirmInput.value
            && this.passwordInput.value !== "";
        this.updateValidationIcon('repassword-validation', isMatch);
        return isMatch;
    }

    handleSubmit(e) {
        if (!this.validatePassword()) {
            e.preventDefault();
            alert('אנא ודא שהסיסמה עומדת בכל הדרישות');
            return;
        }

        if (!this.checkPasswordMatch()) {
            e.preventDefault();
            alert('הסיסמאות אינן תואמות');
            return;
        }
    }
}

// Initialize validator when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new PasswordValidator();
});