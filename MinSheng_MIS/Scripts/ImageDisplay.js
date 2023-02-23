function ImageDisplay(selector) {
    $(document.body).on("click", ".img-display .btn-close", function () {
        $(".img-display").remove();
    })
    $(document.body).on("click", ".img-display", function () {
        $(".img-display").remove();
    })
    $(selector).click(function () {
        let img = this.src;
        $(document.body).append(`
            <div class="img-display">
                <div class="img-display-div">
                    <button class="btn-close btn-close-white"></button>
                    <img src="${img}">
                </div>
            </div>
            `);
    })
}