$(document).ready(function () {
    wireupPrivacyDropDownMenu();

    $("#formGetAlbums").submit();

    wireUpContinousScroll(
    "divLoadingPhotos",
    "FacebookPhoto/GetMorePhotos",
    "More_FB_Photos");

    wireUpFileUploadButton();
});
