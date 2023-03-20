//import "datatable.js"

function DataDetailModal(url) {
    let ModalJQ, ModalBs;
    const sn = [
        { text: "系統別", value: "System" },
        { text: "子系統別", value: "SubSystem" },
        { text: "設備名稱", value: "EName" },
        { text: "區域", value: "Area" },
        { text: "樓層", value: "Floor" },
        { text: "空間名稱", value: "RoomName" },
        { text: "國有財產編碼", value: "PropertyCode" },
        { text: "廠牌", value: "Brand" },
        { text: "DBID", value: "DBID" },
        { text: "RFID", value: "RFID" },
        { text: "設備狀態", value: "EState" },
    ]
    if ($(".data-detail-modal")[0]) {
        console.log("show data-detail-modal")
        bootstrap.Modal.getInstance($(".data-detail-modal")[0]).show();
    }
    else {
        $.getJSON(url, readData);
        //readData({
        //    EName: "AAA"
        //})
    }
    function readData(data) {
        console.log("data-detail-modal readData: ",data);
        const html = `
        <div class="modal fade data-detail-modal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${data.EName ?? "設備資料"}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <table class="datatable-table">${createTableInner(data, sn)}</table>
                    </div>
                    <div class="modal-footer justify-content-center">
                        <a type="button" class="btn btn-search" href="" target="_blank">定位</a>
                    </div>
                </div>
            </div>
        </div>
        `;
        ModalJQ = $(html);
        $(document.body).append(ModalJQ);
        ModalBs = new bootstrap.Modal(ModalJQ[0]);

        ModalBs.show();
    }

}