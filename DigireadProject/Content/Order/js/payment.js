$(document).ready(function() {
    // פורמט אוטומטי למספר כרטיס אשראי
    $('#CardNumber').on('input', function() {
        $(this).val($(this).val().replace(/\D/g, ''));
    });

    // פורמט אוטומטי לתאריך תפוגה
    $('#ExpiryDate').on('input', function() {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length > 2) {
            value = value.substring(0, 2) + '/' + value.substring(2, 4);
        }
        $(this).val(value);
    });

    // פורמט אוטומטי ל-CVV
    $('#CVV').on('input', function() {
        $(this).val($(this).val().replace(/\D/g, '').substring(0, 3));
    });

    // ולידציה נוספת בצד לקוח
    $('form').on('submit', function(e) {
        const cardNumber = $('#CardNumber').val();
        const expiryDate = $('#ExpiryDate').val();
        const cvv = $('#CVV').val();

        if (cardNumber.length !== 16) {
            e.preventDefault();
            alert('נא להזין מספר כרטיס אשראי תקין');
            return false;
        }

        if (!expiryDate.match(/^(0[1-9]|1[0-2])\/([0-9]{2})$/)) {
            e.preventDefault();
            alert('נא להזין תאריך תפוגה תקין (MM/YY)');
            return false;
        }

        if (cvv.length !== 3) {
            e.preventDefault();
            alert('נא להזין קוד CVV תקין');
            return false;
        }
    });
});