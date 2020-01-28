function AddSubscriptionHandler(xhr)
{
    if (xhr.responseJSON != null && xhr.responseJSON.length > 0 && xhr.responseJSON[0].Value)
    {
        $('#Get_Subscriptions_Form').submit();
    } else handleError(xhr, 'ErrorMessageDiv');
}



function DeleteSubscription(subscription_to_delete) {
    if (subscription_to_delete != null) {
        $("input[name='subscription_to_delete'").val(subscription_to_delete);
        $("#Delete_Subscriptions_Form").submit();
    }
}






















function NotificationHandler(notificationItem) {
    $("#accordian_notifications").prepend(notificationItem);
}


function WireUpSubcriptionFieldClick() {
    $(".subscribing_field a").click(function () {
        var selText = $(this).text();
        var checkbox = $('input[id="SubscriptionField.' + selText + '"]');
        if (checkbox != null) {
            if (checkbox.prop("checked")) {
                checkbox.prop("checked", false);
                $(this).removeClass("active");
            }
            else {
                checkbox.prop("checked", true);
                $(this).addClass("active");
            }
        }
    });
}














function WireUpSubscriptionDropDownClick() {
    $(".dropdown-menu li a").click(function () {
        var selText = $(this).text();
        $(this).parents('.btn-group').find('.dropdown-toggle').html(selText +
            ' <span class="caret"></span>');

        if (this.attributes['data-subscription'] != null &&
            this.attributes['data-subscription'].value != null) {
            $("input[name='subscription']").val(this.attributes['data-subscription'].value);

            if (this.attributes['data-subscription'].value == "user") {
                $("#SubscriptionFieldListTitle").html("Select a User fields for Notification");
                $("#SubscriptionFieldList").html($("#UserSubscriptionFieldList").html());

            }
            else if (this.attributes['data-subscription'].value == "page") {
                $("#SubscriptionFieldListTitle").html("Select a Page fields for Notification");
                $("#SubscriptionFieldList").html($("#PageSubscriptionFieldList").html());
            }
            else if (this.attributes['data-subscription'].value == "permissions") {
                $("#SubscriptionFieldListTitle").
                    html("Select a Permission fields for Notification");
                $("#SubscriptionFieldList").html($("#PermissionSubscriptionFieldList").html());
            }
            else {
                $("#SubscriptionFieldListTitle").empty();
                $("#SubscriptionFieldList").empty();
            }


            WireUpSubcriptionFieldClick();

        }
    });
}