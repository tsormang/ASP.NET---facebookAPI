$(document).ready(function () {
        WireUpSubscriptionDropDownClick();

        $("#SubscriptionFieldListTitle").html("Select User fields for Notification");
        $("#SubscriptionFieldList").html($("#UserSubscriptionFieldList").html());
        $("#SubscriptionFieldList").scrollTop(0);

        if ($.connection != null)
            var notification = $.connection.notificationHub;
        if (notification != null) {
            //create a function that the hub can call back to display notifications
            notification.client.addNewNotificationToPage = function (notificationItem) {
                NotificationHandler(notificationItem);
            };
            $.connection.hub.start(function () {
                notification.server.activate().done(function (response) {
                    NotificationHandler(response);
                });
            });
        }
        $("#Get_Subscriptions_Form").submit();
});
