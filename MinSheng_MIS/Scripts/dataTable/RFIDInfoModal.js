//import "datatable.js"

function RFIDLocationModal(data) {
    console.log("Data為: ", data);
    const sn = [,
        { text: "名稱", value: "RFIDName" },
        { text: "棟別", value: "Area" },
        { text: "樓層", value: "Floor" },
        { text: "定位", value: "Location" },
    ]
    
    let ModalJQ, ModalBs;

    /*$.getJSON(url, readData);*/
    
    /*function readData(data) {*/
        console.log("data-detail-modal readData: ", data);
        const html = `
        <div class="modal fade data-detail-modal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title w-100 text-center">定位資訊</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <table class="datatable-table">${createTableInner(data, sn)}</table>
                    </div>
                </div>
            </div>
        </div>
        `;
        ModalJQ = $(html);
        ModalBs = new bootstrap.Modal(ModalJQ[0]);

        ModalBs.show();

        ModalJQ[0].addEventListener("hidden.bs.modal", () => {
            ModalBs.dispose();
            ModalJQ.remove();
        })
    /*}*/
}