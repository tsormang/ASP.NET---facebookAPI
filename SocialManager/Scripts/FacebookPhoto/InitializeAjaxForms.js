var NewAlbumOptions = {
    beforeSend: function () {
        $("#divLoadingAlbums").show();
    },
    success: function () {
        $("#NewAlbumName").val("");
        $("#formGetAlbums").submit();
    },
    error: function (xhr) {
        handleError(xhr, 'Error_Results');
    },
    complete: function () {
        $("#divLoadingAlbums").hide();
    }
};
$("#formNewAlbum").ajaxForm(NewAlbumOptions);

var GetAlbumOptions = {
    beforeSend: function () {
        $("#divLoadingAlbums").show();
    },
    success: function (response) {
        $("#Albums_Result").html(response);
        CompleteLoadingAlbumData();
    },
    error: function (xhr) {
        handleError(xhr, 'Error_Results');
    },
    complete: function () {
        $("#divLoadingAlbums").hide();
    }
};
$("#formGetAlbums").ajaxForm(GetAlbumOptions);

var UploadPhotoOptions = {
    beforeSend: function () {
        $("#divLoadingPhotos").show();
    },
    success: function (response) {
        if (response != null) {
            if (response.status != "failed") {
                $("#PhotoDescription").val("");
                $("#PhotosToUpload").val("");
            }
            if (response.status == "posted") {
                var currentAlbumId = $("#AlbumId").val();
                if (currentAlbumId != null) {
                    ReloadAlbum(currentAlbumId,1);
                }
            }
            else if (response.status == "pending") {
                $('#PhotoPendingPostDialog').modal();
            }
        }
    },
    error: function (xhr) {
        handleError(xhr, 'Error_Results');
    },
    complete: function () {
        $("#divLoadingPhotos").hide();
    }
};
$("#formPhotoUpload").ajaxForm(UploadPhotoOptions);

var GetAlbumPhotoOptions = {
    beforeSend: function () {
        $("#divLoadingPhotos").show();
    },
    success: function (response) {
        $("#Photo_Results").html(response);
        CompleteLoadingPagedData();
    },
    error: function (xhr) {
        handleError(xhr, 'Error_Results');
    },
    complete: function () {
        $("#divLoadingPhotos").hide();
    }
};
$("#Photo_Results_Form").ajaxForm(GetAlbumPhotoOptions);

var ReloadAlbumOptions = {
    beforeSend: function () {
        $("#divLoadingAlbums").show();
    },
    success: function (response) {
        var reloadAlbumId = $("#ReloadAlbumId").val();
        $("#AlbumDiv_" + reloadAlbumId).html(response);
    },
    error: function (xhr) {
        debugger;
        handleError(xhr, 'Error_Results');
    },
    complete: function () {
        $("#divLoadingAlbums").hide();
    }
};
$("#formReloadAlbum").ajaxForm(ReloadAlbumOptions);