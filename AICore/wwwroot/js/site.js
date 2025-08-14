// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

    $(document).ready(function()
    {
        console.log("ready!");
    $("#btn-create-conversation")
    .click(function()
    {
                var lurl = String($(this).data('id'));
    $(".modal-body").load(lurl);
                }
    );
        }
    );
