function removeFromWaitList(waitListId) {
    if (confirm('האם אתה בטוח שברצונך להסיר ספר זה מרשימת ההמתנה?')) {
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: '/BookManagement/RemoveFromWaitList',
            type: 'POST',
            data: {
                waitListId: waitListId,
                __RequestVerificationToken: token
            },
            success: function (result) {
                if (result.success) {
                    location.reload();
                } else {
                    alert(result.message || 'אירעה שגיאה בהסרת הספר מרשימת ההמתנה');
                }
            },
            error: function (xhr, status, error) {
                console.error('שגיאה:', error);
                console.error('סטטוס:', status);
                console.error('תגובה:', xhr.responseText);
                alert('אירעה שגיאה בהסרת הספר מרשימת ההמתנה');
            }
        });
    }
}