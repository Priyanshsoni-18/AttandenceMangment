// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showLoadingEvent() {
    // Display the loading event spinner or message
    document.getElementById('loadingSpinner').style.display = 'block';
}
window.addEventListener("load", () => {
    const loader = document.querySelector(".loader");

    loader.classList.add("loader--hidden");

    loader.addEventListener("transitionend", () => {
        document.body.removeChild(loader);
    });
});
document.getElementById("myForm").addEventListener("submit", function (event) {
    var fileInput = document.getElementById("fileInput");
    if (fileInput.files.length === 0) {
        event.preventDefault(); // Prevent form submission
        alert("Please select a file.");
    }
});

