window.addEventListener('load', async () => {
    let updateTimer = null;
    const UPDATE_TIME = 5000;
    const badge = $("#AlertCollapse").prev(".badge")
    const list = $("#AlertCollapse .collapse-body")
    updater()
    function BellMessage(data) {
        list.empty()
        badge.text(data.length)
        data.forEach(d => {
            const item = $(`
                <a class="alert-item" target="_blank" data-level="${d.WMType}" data-process-state="${d.WMState}">
                    <i class="icon-alert"></i>
                    <span class="road">${d.Location}</span>
                    <span class="time">${d.TimeOfOccurrence}</span>
                    <span class="title">${d.Message}</span>
                    <span class="process-state"></span>
                </a>`) 
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