let currentCartId = null;

$(document).ready(function() {
    // טיפול בכפתורי סגירת המודל
    $('.close-modal').click(function() {
        closeModal();
    });

    // סגירת המודל בלחיצה מחוץ לו
    window.onclick = function(event) {
        const modal = document.getElementById('confirmDeleteModal');
        if (event.target === modal) {
            closeModal();
        }
    }

    // טיפול בכפתור אישור מחיקה
    $('#confirmDelete').click(function() {
        if (!currentCartId) return;

        var token = $('input[name="__RequestVerificationToken"]').val();
        console.log('Attempting to remove cart item:', currentCartId);

        $.ajax({
            url: '/ShoppingCart/RemoveFromCart',
            type: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            data: {
                cartId: currentCartId,
                __RequestVerificationToken: token
            },
            success: function(response) {
                console.log('Server response:', response);

                if (response && response.success === true) {
                    $(`#cart-row-${currentCartId}`).fadeOut(300, function() {
                        $(this).remove();
                        updateTotalAmount();
                        updateItemCount();

                        if ($('.cart-items').children().length === 0) {
                            location.reload();
                        }
                    });
                    showMessage(true);
                } else {
                    console.error('Remove failed:', response.message);
                    showMessage(false);
                }
                closeModal();
            },
            error: function(xhr, status, error) {
                console.error('Ajax error:', {xhr: xhr, status: status, error: error});
                showMessage(false);
                closeModal();
            },
            complete: function() {
                currentCartId = null;
            }
        });
    });
});

function closeModal() {
    const modal = document.getElementById('confirmDeleteModal');
    modal.style.display = 'none';
    document.body.classList.remove('modal-open');
    currentCartId = null;
}

function removeFromCart(cartId) {
    currentCartId = cartId;
    const modal = document.getElementById('confirmDeleteModal');
    modal.style.display = 'block';
    document.body.classList.add('modal-open');
}

function updateQuantity(cartId, newQuantity) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    var maxStock = parseInt($(`#quantity-${cartId}`).siblings('button')
        .filter((_, el) => $(el).text() === '+')
        .attr('onclick')
        .match(/\d+(?=\)$)/)[0]);

    $.ajax({
        url: '/ShoppingCart/UpdateQuantity',
        type: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        data: {
            cartId: cartId,
            quantity: newQuantity,
            __RequestVerificationToken: token
        },
        success: function(response) {
            if (response.success) {
                $(`#quantity-${cartId}`).text(newQuantity);

                const quantityControls = $(`#quantity-${cartId}`).closest('.quantity-controls');
                quantityControls.find('button:first').attr('onclick', `decreaseQuantity(${cartId}, ${newQuantity})`);
                quantityControls.find('button:last').attr('onclick', `increaseQuantity(${cartId}, ${newQuantity}, ${maxStock})`);

                updateItemPrice(cartId);
                updateTotalAmount();
                updateItemCount();

                showMessage(true);
            } else {
                showMessage(false);
                console.error('Failed to update quantity:', response.message);
            }
        },
        error: function(xhr, status, error) {
            showMessage(false);
            console.error('Error:', error);
        }
    });
}

function decreaseQuantity(cartId, currentQuantity) {
    if (currentQuantity > 1) {
        updateQuantity(cartId, currentQuantity - 1);
    }
}

function increaseQuantity(cartId, currentQuantity, maxStock) {
    if (currentQuantity < maxStock) {
        updateQuantity(cartId, currentQuantity + 1);
    }
}

function updateItemPrice(cartId) {
    const item = $(`#cart-row-${cartId}`);
    const quantity = parseInt($(`#quantity-${cartId}`).text());
    const priceText = item.find('.price').text();
    const price = parseFloat(priceText.replace('₪', '').replace(/,/g, ''));

    if (!isNaN(price) && !isNaN(quantity)) {
        const total = price * quantity;
        item.find('.total').text(`סה"כ: ₪${total.toLocaleString('he-IL', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        })}`);
    }
}

function updateTotalAmount() {
    var total = 0;
    $('.cart-item').each(function() {
        var priceText = $(this).find('.total').text();
        var price = parseFloat(priceText.replace('סה"כ: ₪', '').replace(/,/g, ''));
        if (!isNaN(price)) {
            total += price;
        }
    });

    $('.summary-row.total span:last-child').text('₪' + total.toLocaleString('he-IL', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }));
}

function updateItemCount() {
    var totalItems = 0;
    $('.cart-item').each(function() {
        var quantity = parseInt($(this).find('.quantity-controls span').text());
        if (!isNaN(quantity)) {
            totalItems += quantity;
        }
    });

    $('.items-count').text(totalItems + ' פריטים');
    $('.summary-row:not(.total) span:last-child').text(totalItems);
}

function showMessage(isSuccess) {
    const successMsg = $('#successMessage');
    const errorMsg = $('#errorMessage');

    const messageToShow = isSuccess ? successMsg : errorMsg;
    messageToShow.fadeIn(300);

    setTimeout(function() {
        messageToShow.fadeOut(300);
    }, 3000);
}

function completePurchase() {
    var token = $('input[name="__RequestVerificationToken"]').val();

    // הצג אנימציית טעינה או הודעה
    $('.checkout-btn').prop('disabled', true).text('מבצע רכישה...');

    // שלח את הטופס באופן רגיל (לא Ajax)
    var form = $('<form>')
        .attr('method', 'POST')
        .attr('action', '/Order/CompletePurchase')
        .append($('<input>')
            .attr('type', 'hidden')
            .attr('name', '__RequestVerificationToken')
            .val(token));

    $('body').append(form);
    form.submit();
}