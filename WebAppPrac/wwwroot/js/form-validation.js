document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const emailInput = document.getElementById("username");

    form.addEventListener("submit", function (e) {
        const email = emailInput.value.trim();
        const isValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

        if (!isValid) {
            e.preventDefault();
            const message = emailInput.dataset.error;
            showError(message);
            emailInput.focus();
        }
    });

    function showError(text) {
        let box = document.getElementById("error-box");
        if (!box) {
            box = document.createElement("div");
            box.id = "error-box";
            box.className = "alert alert-danger mt-2";
            emailInput.parentNode.appendChild(box);
        }
        box.textContent = text;
    }
});
