document.addEventListener('DOMContentLoaded', function () {
    initializePasswordValidation();
    initializeConfirmPasswordValidation();
    initializeFormValidation();
});

function initializePasswordValidation() {
    const passwordField = document.getElementById("passwordField");
    if (!passwordField) return;

    passwordField.addEventListener("input", function () {
        const password = this.value;
        validatePassword(password);
    });
}

function validatePassword(password) {
    const requirements = {
        length: password.length >= 6,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        number: /[0-9]/.test(password),
        special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
    };

    updateRequirementIcon("length-req", requirements.length, "לפחות 6 תווים");
    updateRequirementIcon("uppercase-req", requirements.uppercase, "אות גדולה אחת לפחות");
    updateRequirementIcon("lowercase-req", requirements.lowercase, "אות קטנה אחת לפחות");
    updateRequirementIcon("number-req", requirements.number, "מספר אחד לפחות");
    updateRequirementIcon("special-req", requirements.special, "תו מיוחד אחד לפחות");

    const passwordValidationIcon = document.getElementById("password-validation");
    if (passwordValidationIcon) {
        const allRequirementsMet = Object.values(requirements).every(req => req);
        updateValidationIcon(passwordValidationIcon, allRequirementsMet);
    }

    return requirements;
}

function initializeConfirmPasswordValidation() {
    const rePasswordField = document.getElementById("rePasswordField");
    if (!rePasswordField) return;

    rePasswordField.addEventListener("input", function () {
        const password = document.getElementById("passwordField").value;
        const rePassword = this.value;
        validateConfirmPassword(password, rePassword);
    });
}

function validateConfirmPassword(password, rePassword) {
    const validationIcon = document.getElementById("repassword-validation");
    if (validationIcon) {
        const isValid = password === rePassword && password !== "";
        updateValidationIcon(validationIcon, isValid);
    }
    return password === rePassword;
}

function updateRequirementIcon(elementId, isValid, text) {
    const element = document.getElementById(elementId);
    if (element) {
        const icon = isValid ?
            '<i class="fas fa-check text-success"></i>' :
            '<i class="fas fa-times text-danger"></i>';
        element.innerHTML = `${icon} ${text}`;
    }
}

function updateValidationIcon(element, isValid) {
    element.innerHTML = isValid ?
        '<i class="fas fa-check text-success"></i>' :
        '<i class="fas fa-times text-danger"></i>';
}

function initializeFormValidation() {
    const form = document.querySelector('form');
    if (!form) return;

    form.addEventListener('submit', function (e) {
        const password = document.getElementById("passwordField").value;
        const rePassword = document.getElementById("rePasswordField").value;

        const requirements = validatePassword(password);
        const allRequirementsMet = Object.values(requirements).every(req => req);

        if (!allRequirementsMet) {
            e.preventDefault();
            alert('אנא ודא שהסיסמה עומדת בכל הדרישות');
            return;
        }

        if (!validateConfirmPassword(password, rePassword)) {
            e.preventDefault();
            alert('הסיסמאות אינן תואמות');
            return;
        }
    });
}