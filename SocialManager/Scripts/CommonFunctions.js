//reference the auto-generated proxy for the hub
//var notification = $.connection.notificationHub;


function handleError(ajaxContext, UpdateTargetId) {
    if (ajaxContext != null &&
        ajaxContext.responseText != null) {
        $("#" + UpdateTargetId).html(ajaxContext.responseText);
    }
}
function wireupPrivacyDropDownMenu() {
    //Allow dropdown as a select
    $(".dropdown-menu li a").click(function () {
        var selText = $(this).text();
        $(this).parents('.btn-group').find('.dropdown-toggle').html(selText +
            ' <span class="caret"></span>');
        if (this.attributes['data-privacy'] != null &&
            this.attributes['data-privacy'].value != null)
            $("input[name='Privacy']").val(this.attributes['data-privacy'].value);
    });
}
function wireUpContinousScroll(LoadingImageDiv, Url, ResultDiv) {
    //Continous Paging Scroll
    $("#FB_Scrollable_List").scroll(function () {
        var ScrollingDiv = $("#FB_Scrollable_List");
        buffer = 60 //# of pixels from bottom of scroll to fire get more data function
        if (ScrollingDiv
            .prop('scrollHeight') - ScrollingDiv.scrollTop()
            <= ScrollingDiv.height() + buffer) {
            var nextpageUrl = $("#FB_Next_PageLink").val();
            if (nextpageUrl != null &&
                nextpageUrl != "") {
                GetMoreData(nextpageUrl, LoadingImageDiv, Url, ResultDiv);
            }
            else {
                //At the end of paged data
                AllowGetMoreDataCall = false;
            }
        };

    });
}

function GetMoreData(NextPageUri,LoadingImageDiv, Url, ResultDiv) {
    if (AllowGetMoreDataCall) {
        AllowGetMoreDataCall = false
        //ajax call to get more data
        $("#" + LoadingImageDiv).show();
        $.ajax({
            type: "POST",
            url: Url,
            data: { NextPageUri: NextPageUri },
            cache: false,
            success: function (result) {
                if (result != null &&
                    result != "") {
                    $("#" + LoadingImageDiv).hide();
                    $("#FB_Next_PageLink").remove();
                    $("#FB_CurrentDataCount").remove();
                    $("#" + ResultDiv).append(result);
                    CompleteLoadingPagedData();

                }
                AllowGetMoreDataCall = true;
            },
            error: function (result) {
                AllowGetMoreDataCall = false;
            }
        })

    }
}

function wireUpFileUploadButton() {
    //created by Cory LaViska
    //Article: Whipping File Inputs Into Shape with Bootstrap 3
    //reference: http://www.abeautifulsite.net/whipping-file-inputs-into-shape-with-bootstrap-3/
        $('.btn-file :file').on('fileselect', function (event, numFiles, label) {
            var input = $(this).parents('.input-group').find(':text'),
                log = numFiles > 1 ? numFiles + ' files selected' : label;

            if (input.length) {
                input.val(log);
            } else {
                if (log) alert(log);
            }

        });

        $(document).on('change', '.btn-file :file', function () {
            var input = $(this),
                numFiles = input.get(0).files ? input.get(0).files.length : 1,
                label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
            input.trigger('fileselect', [numFiles, label]);
        });
}

