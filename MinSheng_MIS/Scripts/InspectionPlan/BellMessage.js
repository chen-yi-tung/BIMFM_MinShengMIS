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
        btn.on("click", (event) => {
            collapse.toggleClass("show")
            if (collapse.hasClass("show")) {
                console.log("bell show")
                postHaveReadMessage(list.children().toArray().map((item) => item.dataset.wmsn))
                badge.text("")
            }
        })
        updater()
    }
    function BellMessage(data, hrd) {
        list.empty()
        let count = 0;
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
            item.attr("data-wmsn", d.WMSN)
            list.append(item)

            if (!hrd.includes(d.WMSN)) {
                count++;
            }
        });

        badge.text(count > 99 ? "99+" : count || "")
    }
    function getBellMessageInfo() {
        return new Promise((success, error) => {
            $.ajax({
                url: `/WarningMessage_Management/BellMessageInfo`,
                type: "GET",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success,
                error,
            })
        })

    }
    function getHaveReadMessage() {
        return new Promise((success, error) => {
            $.ajax({
                url: `/WarningMessage_Management/GetHaveReadMessage`,
                type: "GET",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success(res) {
                    if (res.Error) { error(res) }
                    success(res.Data)
                },
                error,
            })
        })
    }
    function postHaveReadMessage(data) {
        console.log(data)
        return new Promise((success, error) => {
            $.ajax({
                url: `/WarningMessage_Management/PostHaveReadMessage`,
                type: "POST",
                data: JSON.stringify(data),
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success,
                error,
            })
        })
    }
    async function updater() {
        clearTimeout(updateTimer)
        const [
            BellMessageInfo,
            HaveReadData,
        ] = await Promise.all([
            getBellMessageInfo(),
            getHaveReadMessage(),
        ])
        BellMessage(BellMessageInfo, HaveReadData)
        updateTimer = setTimeout(updater, UPDATE_TIME)
    }
})