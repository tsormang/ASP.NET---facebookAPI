var AllowGetMoreDataCall = true;
var ItemShowingCount = 0;

function CompleteLoadingAlbumData() {
    var currentAlbumId = $("#FirstAlbumId").val();
    $("#AlbumId").val(currentAlbumId);
    GetAlbumPhotos(currentAlbumId);
    $("#AlbumCarousel").on("slide.bs.carousel", function (event) {
        var next = $(event.relatedTarget);
        if (next != null) {
            var currentAlbumId = next.find("[name='AlbumId']").val()
            GetAlbumPhotos(currentAlbumId);
        }
    });
}

function GetAlbumPhotos(currentAlbumId) {
    $("#AlbumId").val(currentAlbumId);
    ItemShowingCount = 0;
    $("#FB_Scrollable_List").scrollTop(0);
    $("#More_FB_Photos").empty();
    $("#Photo_Results_Form").submit();
}

function CompleteLoadingPagedData() {
    AllowGetMoreDataCall = true;
    ItemShowingCount = ItemShowingCount + parseInt($("#FB_CurrentDataCount").val());

    if (ItemShowingCount > 1)
        PhotoCountLabel = "Showing " + ItemShowingCount + " photos";
    else
        PhotoCountLabel = "Showing " + ItemShowingCount + " photo";

    $("#ShowingCountLabel").html(PhotoCountLabel);
    $("#TargetAlbumId").val($("#AlbumId").val());
    $('[data-toggle="tooltip"]').tooltip();
}


















function ReloadAlbum(currentAlbumId, PhotoCountChange)
{
    var currentAlbumCount = Number($("#AlbumCount_" + currentAlbumId).val()) + PhotoCountChange;
    $("#AlbumCount_" + currentAlbumId).val(currentAlbumCount);
    if (currentAlbumCount > 1)
        $("#AlbumCaption_" + currentAlbumId).html(currentAlbumCount + " photos");
    else {
        {
            $("#AlbumCaption_" + currentAlbumId).html(currentAlbumCount + " photo");
            $("#ReloadAlbumId").val(currentAlbumId);
            $("#formReloadAlbum").submit();
        }
    }
    GetAlbumPhotos(currentAlbumId);
}















function DeletePhoto(PhotoId)
{
    $.ajax({
        type: "Post",
        url: "/FacebookPhoto/DeletePhoto",
        async: false,
        data: {PhotoId: PhotoId},
        cache: false,
        success: function (result) {
            if (result != null &&
                result.length > 0)
                if (result[0].Key == "success" &&
                    result[0].Value == true)
                    {
                        var currentAlbumId = $("#AlbumId").val();
                        if (currentAlbumId != null) {
                            ReloadAlbum(currentAlbumId,-1);
                        }
                }
                else {
                    $("#Error_Results").html(result);
                }
        },
        error: function (xhr) {
            handleError(xhr, 'Error_Results');
        }
    });
}