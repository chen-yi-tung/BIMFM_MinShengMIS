function RFIDResultModal(data, Options, tableZoneID) {
    console.log("Data為: ", data);
    const sn = [,
        { text: "RFID內碼", value: "InterCode" },
        { text: "RFID外碼", value: "ExterCode" },
        { text: "類別", value: "Type" },
        { text: "品項名稱", value: "ItemName" },
        { text: "領取數量", value: "Num" },
        { text: "單位", value: "Init" },
    ]
    
    let ModalJQ, ModalBs;

    /*$.getJSON(url, readData);*/

    /*function readData(data) {*/

    console.log("data-detail-modal readData: ", data);
    
    if (data.result) {
        const html = `
        <div class="modal fade data-detail-modal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title w-100 text-center">RFID掃描資訊</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <table class="datatable-table">${createTableInner(data.info, sn)}</table>
                    </div>

                    <div class="modal-footer justify-content-center">
                        <button type="button" class="btn btn-cancel" data-bs-dismiss="modal">返回</button>
                        <button type="button" class="btn btn-export" id="add_row">確定</button>
                    </div>
                </div>
            </div>
        </div>
        `;
        
        ModalJQ = $(html);
        ModalBs = new bootstrap.Modal(ModalJQ[0]);

        //點擊確定新增此RFID按鈕
        ModalJQ.find("#add_row").on('click', function () {
            const datatable = document.getElementById(tableZoneID);
            console.log("確認這裡tableZoneID", tableZoneID);

            if (datatable) {
                const style = window.getComputedStyle(datatable);
                const isHidden = style.display === 'none';
                if (isHidden) {
                    datatable.style.display = 'block';
                }
                const appendOnly = true;
                createRFIDForm(data.info, Options, tableZoneID, appendOnly);
            }
            ModalBs.hide();
        });

        ModalJQ[0].addEventListener("hidden.bs.modal", () => {
            ModalBs.dispose();
            ModalJQ.remove();
        })
        ModalBs.show();
    } else {
        let modal = createDialogModal({
            id: "DialogModal-Delete",
            inner: `
            <div class="d-flex justify-content-center align-items-center gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                    class="bi bi-exclamation-triangle-fill" viewBox="0 0 16 16">
                    <path
                        d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z" />
                </svg>
                查無此RFID
            </div>
        `,
            button: [
                { className: "btn btn-export", cancel: true, text: "確定" },
            ]
        })
    }
        
        
    /*}*/
}

function createRFIDForm(data, Options, tableZoneID, appendOnly) {
    const modalTableBody = $(`#RFIDForm tbody`);
    if (!modalTableBody.length) {
        console.error("Modal table not found!");
        return;
    }
    const newRow = createTableGrid(data, Options, tableZoneID, appendOnly);
    modalTableBody.append(newRow);
}