// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function ok() {
    console.log("OK");
}


document.addEventListener("DOMContentLoaded", function () {
    const toggle = document.getElementById("notification-toggle");
    const box = document.getElementById("notification-box");
    const close = document.getElementById("notification-close");

    toggle.addEventListener("click", function (e) {
        e.preventDefault();
        box.classList.toggle("hidden");
    });

    close.addEventListener("click", function () {
        box.classList.add("hidden");
    });

    document.addEventListener("click", function (e) {
        if (!box.contains(e.target) && !toggle.contains(e.target)) {
            box.classList.add("hidden");
        }
    });

    if (window.lucide) lucide.createIcons();
});