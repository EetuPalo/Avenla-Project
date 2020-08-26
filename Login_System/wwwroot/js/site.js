// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    if (document.getElementsByName("culture")[0].selectedOptions[0].value == "fi-FI") {
        $('.table').DataTable({
            stateSave: true,
            language: {

                "url": "//cdn.datatables.net/plug-ins/1.10.21/i18n/Finnish.json"
            }

        });
    }
    else if (document.getElementsByName("culture")[0].selectedOptions[0].value == "en-GB") {
        $('.table').DataTable({
            stateSave: true,
            language: {

                "url": "//cdn.datatables.net/plug-ins/1.10.21/i18n/English.json"
            }

        });
    }

    $('.disable_arrow').css("background", "none");
});


function goBack() {
    window.history.back();
}
