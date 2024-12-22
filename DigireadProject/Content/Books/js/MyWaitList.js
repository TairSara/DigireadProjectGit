function removeFromWaitList(waitListId) {
    if (confirm('האם אתה בטוח שברצונך להסיר ספר זה מרשימת ההמתנה?')) {
        $.ajax({
            url: '@Url.Action("RemoveFromWaitList", "BookManagement")',
            type: 'POST',
            data: {
                waitListId: waitListId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    location.reload();
                } else {
                    alert(result.message || 'אירעה שגיאה בהסרת הספר מרשימת ההמתנה');
                }
            },
            error: function () {
                alert('אירעה שגיאה בהסרת הספר מרשימת ההמתנה');
            }
        });
    }
}