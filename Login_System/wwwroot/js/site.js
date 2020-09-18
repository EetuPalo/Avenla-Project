// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    if (document.getElementsByName("culture")[0].selectedOptions[0].value == "fi-FI") {
        var table = $('.table').DataTable({

            stateSave: true,
        
            language: {

                "url": "//cdn.datatables.net/plug-ins/1.10.21/i18n/Finnish.json"
            }

        });
    }
    else if (document.getElementsByName("culture")[0].selectedOptions[0].value == "en-GB") {
        var table = $('.table').DataTable({
            stateSave: true,
           
            language: {

                "url": "//cdn.datatables.net/plug-ins/1.10.21/i18n/English.json"
            }

        });
        
    }
    //adds clear icon inside datatables search bar. since mozilla doesn't support one out of the gate, we have to create our own
    $(document).ready(function () { $('div.dataTables_filter input').addClass('clearable');})
    $('.disable_arrow').css("background", "none");
  
    /**
 * Clearable text inputs
 */
    function tog(v) { return v ? 'addClass' : 'removeClass'; }
    $(document).on('input', '.clearable', function () {
        $(this)[tog(this.value)]('x');
    }).on('mousemove', '.x', function (e) {
        $(this)[tog(this.offsetWidth - 18 < e.clientX - this.getBoundingClientRect().left)]('onX');
    }).on('touchstart click', '.onX', function (ev) {
        ev.preventDefault();
        $(this).removeClass('x onX').val('').change();
    });

// $('.clearable').trigger("input");
// Uncomment the line above if you pre-fill values from LS or server

});
$(":input[type='search']")
function goBack() {
    window.history.back();
}
/**
 * 
 * Delete-actions uses Sweetalert popUp-confirmation
 * if you need to redirect page back to /{controller}/index
 * call function with 4th parameter 'true'
 */
function DeletePopUp(urlController, id, lang, redirect) {   
    //Have to manually set the warning text by current language
    var title = "";
    var text = "";
    if (lang.match("en-GB")) {
        title = "Delete";
        text = "Are you sure you want to delete item?"
    }
    else {
        title = "Poistaminen";
        text = "Haluatko varmasti poistaa kohteen?"
    }
    var urlString = "/fi_FI/" + urlController + "/Delete/" + id;      
    swal({
        title: title,
        text: text,
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: urlString,
                beforeSend: function (request) {
                    request.setRequestHeader("RequestVerificationToken", $("[name='__RequestVerificationToken']").val());
                },
                success: function (data) {
                    if (data.success) {
                        if (redirect) { window.location = '/fi_FI/' + urlController; }
                        else { location.reload(); }                                        
                    }
                    else {
                        swal({
                            title: "Error message",
                            text: "Delete failed",
                            icon: "warning",
                            buttons: true
                        })                       
                    }
                }
            });
        }
    });
}
