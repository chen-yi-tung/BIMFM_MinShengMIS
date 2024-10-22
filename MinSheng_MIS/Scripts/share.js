function addButtonEvent() {
    $("#back").click(function () {
        history.back();
    })

    $("#submit").click(function () {
        save();
    })
}