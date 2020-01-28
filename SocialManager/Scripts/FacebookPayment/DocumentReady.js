window.fbAsyncInit = function () {
    FB.init({
        appId: _CurrentFB_AppId,
        status: true,
        cookie: true,
        xfbml: true
    });
    FB.getLoginStatus(function (response) {
        // Check login status on load, and if the user is
        // already logged in, go directly to the welcome message.
        if (response.status == 'connected') {
            onLogin(response);
        } else {
            // Otherwise, show Login dialog first.
            FB.login(function (response) {
                onLogin(response);
            }, { scope: 'user_friends, email' });
        }
    });

    function onLogin(response) {
        if (response.status == 'connected') {
            FB.api('/me?fields=first_name', function (data) {
                var welcomeBlock = document.getElementById('fb-welcome');
                welcomeBlock.innerHTML = 'Hello, ' + data.first_name + '!';
            });
        }
    }

    document.getElementById('pay').onclick = function () { buy() };

    function buy() {
        var obj = {
            method: 'pay',
            action: 'purchaseitem',
            product: 'http://socialmanager.azurewebsites.net/FacebookPayment/Product'
        };

        FB.ui(obj, function (data) {
            console.log(data);
        });
    }

   
};

// Load the SDK Asynchronously
(function (d) {
    var js, id = 'facebook-jssdk', ref = d.getElementsByTagName('script')[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement('script'); js.id = id; js.async = true;
    js.src = "//connect.facebook.net/en_US/all.js";
    ref.parentNode.insertBefore(js, ref);
}(document));
