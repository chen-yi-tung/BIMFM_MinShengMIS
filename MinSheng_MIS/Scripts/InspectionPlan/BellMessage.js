window.addEventListener('load', async () => {
    let updateTimer = null;
    const UPDATE_TIME = 5000;
    const badge = $("#AlertBadge")
    const collapse = $("#AlertCollapse")
    const list = collapse.find(".simplebar-content")
    const btn = $(".btn-alert")
    init()
    function init() {
        btn.on("click", ".alert-item", (event) => {
            event.stopPropagation()
            event.preventDefault()
            window.open(event.currentTarget.href, "_blank");
        })
        btn.on("click", (event) => { collapse.toggleClass("show") })
        updater()
    }
    function BellMessage(data) {
        list.empty()
        badge.text(data.length)
        data.forEach(d => {
            const item = $(`
                <a class="alert-item" target="_blank" data-level="${d.WMType}" data-process-state="${d.WMState ?? 1}">
                    <i class="icon-alert"></i>
                    <span class="road">${d.Location}</span>
                    <span class="time">${d.TimeOfOccurrence ?? "-"}</span>
                    <span class="title">${d.Message}</span>
                    <span class="process-state"></span>
                </a>`)
            item.attr("href", `/WarningMessage_Management/Read/${d.WMSN}`)
            list.append(item)
        });
    }
    function readData() {
        return new Promise((resolve) => {
            $.ajax({
                url: `/WarningMessage_Management/BellMessageInfo`,
                type: "GET",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success(res) {
                    BellMessage(res)
                    resolve()
                },
                error(res) {
                    BellMessage([])
                    resolve()
                }
            })
        })

    }
    async function updater() {
        clearTimeout(updateTimer)
        await readData()
        updateTimer = setTimeout(updater, UPDATE_TIME)
    }
})