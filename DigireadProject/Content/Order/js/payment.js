$(document).ready(function() {
    function isValidCreditCard(number) {
        number = number.replace(/\D/g, '');
        if (number.length !== 16) return false;

        let sum = 0;
        let isEven = false;

        for (let i = number.length - 1; i >= 0; i--) {
            let digit = parseInt(number[i]);

            if (isEven) {
                digit *= 2;
                if (digit > 9) {
                    digit -= 9;
                }
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 === 0;
    }

    function isValidExpiryDate(value) {
        if (!value.match(/^(0[1-9]|1[0-2])\/([0-9]{2})$/)) return false;

        const [month, year] = value.split('/');
        const currentDate = new Date();
        const currentYear = currentDate.getFullYear() % 100;
        const currentMonth = currentDate.getMonth() + 1;

        const expYear = parseInt(year);
        const expMonth = parseInt(month);

        if (expYear < currentYear || (expYear === currentYear && expMonth < currentMonth)) {
            return false;
        }

        if (expYear > currentYear + 10) {
            return false;
        }

        return true;
    }

  
    $('#CardNumber').on('input', function() {
        let value = $(this).val().replace(/\D/g, '');
        let formattedValue = '';
        
        if (value.length === 16) {
            if (!isValidCreditCard(value)) {
                $(this).addClass('is-invalid');
                $(this).next('.text-danger').text('מספר כרטיס לא תקין');
            } else {
                $(this).removeClass('is-invalid').addClass('is-valid');
                $(this).next('.text-danger').text('');
            }
        } else {
            $(this).removeClass('is-valid is-invalid');
            $(this).next('.text-danger').text('');
        }
    });

    $('#ExpiryDate').on('input', function() {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length >= 2) {
            const month = value.substring(0, 2);
            if (parseInt(month) > 12) {
                value = '12' + value.substring(2);
            } else if (parseInt(month) < 1) {
                value = '01' + value.substring(2);
            }
            value = value.substring(0, 2) + '/' + value.substring(2);
        }
        $(this).val(value.substring(0, 5));

        if (value.length >= 4) {
            if (!isValidExpiryDate($(this).val())) {
                $(this).addClass('is-invalid');
                $(this).next('.text-danger').text('תאריך תפוגה לא תקין או פג תוקף');
            } else {
                $(this).removeClass('is-invalid').addClass('is-valid');
                $(this).next('.text-danger').text('');
            }
        } else {
            $(this).removeClass('is-valid is-invalid');
            $(this).next('.text-danger').text('');
        }
    });

    $('#CVV').on('input', function() {
        let value = $(this).val().replace(/\D/g, '').substring(0, 3);
        $(this).val(value);

        if (value.length === 3) {
            $(this).removeClass('is-invalid').addClass('is-valid');
            $(this).next('.text-danger').text('');
        } else if (value.length > 0) {
            $(this).addClass('is-invalid');
            $(this).next('.text-danger').text('קוד CVV חייב להכיל 3 ספרות');
        } else {
            $(this).removeClass('is-valid is-invalid');
            $(this).next('.text-danger').text('');
        }
    });

    $('#CardHolderName').on('input', function() {
        let value = $(this).val();
        if (value.length > 0) {
            if (!/^[\u0590-\u05FF\s]+$/.test(value)) {
                $(this).addClass('is-invalid');
                $(this).next('.text-danger').text('נא להזין שם בעברית בלבד');
            } else if (value.length < 2) {
                $(this).addClass('is-invalid');
                $(this).next('.text-danger').text('נא להזין שם מלא');
            } else {
                $(this).removeClass('is-invalid').addClass('is-valid');
                $(this).next('.text-danger').text('');
            }
        } else {
            $(this).removeClass('is-valid is-invalid');
            $(this).next('.text-danger').text('');
        }
    });

    $('.payment-form').on('submit', function(e) {
        let isValid = true;
        let errorMessage = '';
        
        const expiryDate = $('#ExpiryDate').val();
        if (!isValidExpiryDate(expiryDate)) {
            if (isValid) {
                $('#ExpiryDate').addClass('is-invalid').focus();
            }
            isValid = false;
            errorMessage = errorMessage || 'נא להזין תאריך תפוגה תקין';
        }

        const cvv = $('#CVV').val();
        if (cvv.length !== 3) {
            if (isValid) {
                $('#CVV').addClass('is-invalid').focus();
            }
            isValid = false;
            errorMessage = errorMessage || 'נא להזין קוד CVV תקין';
        }

        const cardHolderName = $('#CardHolderName').val().trim();
        if (cardHolderName.length < 2 || !/^[\u0590-\u05FF\s]+$/.test(cardHolderName)) {
            if (isValid) {
                $('#CardHolderName').addClass('is-invalid').focus();
            }
            isValid = false;
            errorMessage = errorMessage || 'נא להזין שם מלא בעברית';
        }

        if (!isValid) {
            e.preventDefault();
            if ($('.alert-danger').length) {
                $('.alert-danger').text(errorMessage);
            } else {
                $('<div class="alert alert-danger mt-3">' + errorMessage + '</div>')
                    .insertBefore('.form-actions');
            }

            $('.is-invalid').closest('.form-group').addClass('shake');
            setTimeout(() => {
                $('.form-group').removeClass('shake');
            }, 500);

            return false;
        }
    });

    $('.form-control').on('focus', function() {
        $(this).removeClass('is-invalid');
        $(this).next('.text-danger').text('');
        $('.alert-danger').remove();
    });
});
