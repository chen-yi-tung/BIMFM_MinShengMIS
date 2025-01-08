async function init_CreateInbound() {
    const MAX_ROW_SIZE = 1; //只能有一筆RFID資料
    document.getElementById("rfid").onclick = () => checkRFID()
    document.getElementById("back").onclick = () => history.back()
    document.getElementById("submit").onclick = () => checkSave()

    const formTab = DT.setupFormTab()
    const fileUploader = new FileUploader({
        container: "#FileUploader",
        className: "form-group col-3fr",
        required: false,
        accept: [".jpg", ".jpeg", ".png", ".pdf"],
        label: "採購單據",
        id: "PurchaseOrder"
    })

    const StockTypeSN = await formDropdown.pushSelect({ id: "StockTypeSN", url: '/DropDownList/StockType' });
    StockTypeSN.addEventListener('change', async function (e) {
        const v = e.currentTarget.value;
        await formDropdown.pushSelect({ id: "SISN", url: `/DropDownList/StockName?StockTypeSN=${v}`, placeholder: v ? "請選擇" : "請先選擇類別" });
    });
    StockTypeSN.dispatchEvent(new Event('change'))

    const RFIDModal = await init_RFIDModal()

    function checkRFID() {
        const fakeRFID = Math.random().toString(36).slice(2, 10 + 2);
        console.log('checkRFID', fakeRFID)

        //TODO: 後端檢查RFID內碼是否已使用
        if (false) {
            DT.createDialogModal(`取得RFID資訊失敗！`)
        }

        RFIDModal.setData({
            InternalCode: fakeRFID,
        });
        RFIDModal.show()

    }

    async function init_RFIDModal() {
        const modalTemplate = document.getElementById("RFIDModal");
        const modal = modalTemplate.content.cloneNode(true).firstElementChild;
        const bsModal = bootstrap.Modal.getOrCreateInstance(modal);
        const StockTypeSN = await formDropdown.pushSelect({ id: modal.querySelector("#StockTypeSN"), url: '/DropDownList/StockType' });
        StockTypeSN.addEventListener('change', async function (e) {
            const v = e.currentTarget.value;
            await formDropdown.pushSelect({ id: modal.querySelector("#SISN"), url: `/DropDownList/StockName?StockTypeSN=${v}`, placeholder: v ? "請選擇" : "請先選擇類別" });
        });
        StockTypeSN.dispatchEvent(new Event('change'))

        //隱藏時移除元素
        modal.addEventListener("hidden.bs.modal", () => {
            modal.remove();
        })

        //編輯時設定資料
        bsModal.setData = (data = {}) => {
            modal.querySelector("#isEdit").value = data.isEdit ?? false;
            modal.querySelector("#InternalCode").value = data.InternalCode ?? "";
            modal.querySelector("#ExternalCode").value = data.ExternalCode ?? "";
            modal.querySelector("#StockTypeSN").value = data.StockTypeSN ?? "";
            const v = data.StockTypeSN;
            formDropdown.pushSelect({ id: modal.querySelector("#SISN"), url: `/DropDownList/StockName?StockTypeSN=${v}`, placeholder: v ? "請選擇" : "請先選擇類別" }).then(() => {
                modal.querySelector("#SISN").value = data.SISN ?? "";
            })
            modal.querySelector("#NumberOfChanges").value = data.NumberOfChanges ?? 1;
        }

        //儲存時取得資料
        bsModal.getData = () => {
            const isEdit = modal.querySelector("#isEdit").value ?? false;
            const InternalCode = modal.querySelector("#InternalCode").value ?? null;
            const ExternalCode = modal.querySelector("#ExternalCode").value ?? null;
            const StockTypeSN = modal.querySelector("#StockTypeSN").value ?? null;
            const SISN = modal.querySelector("#SISN").value ?? null;
            const NumberOfChanges = modal.querySelector("#NumberOfChanges").value ?? null;
            return { InternalCode, ExternalCode, StockTypeSN, SISN, NumberOfChanges, isEdit };
        }

        //儲存
        const datas = [];
        bsModal.datas = datas;
        modal.querySelector('#add-row').addEventListener('click', async () => {
            const form = modal.querySelector('form');
            if (!form.reportValidity()) {
                return;
            }
            addRow();
        })

        return bsModal;

        async function addRow() {
            bsModal.hide();
            const originData = bsModal.getData();
            const res = await $.getJSON(`/Stock_Management/GetComputationalStockDetail?id=${originData.SISN}`).then(res => res.Datas)
            const data = Object.assign(res, originData)
            if (data.isEdit === 'true') {
                const index = datas.findIndex(x => x.InternalCode === data.InternalCode)
                if (index === -1) {
                    console.error("Not found RFID");
                    return;
                }
                Object.assign(datas[index], data)
                const rowDOM = document.querySelector(`#RFID_Gird [data-internal-code="${data.InternalCode}"]`);
                for (const [k, v] of Object.entries(data)) {
                    const td = rowDOM.querySelector("#d-" + k)
                    if (td) {
                        td.textContent = v;
                    }
                }
                return;
            }
            data.isEdit = true;

            datas.push(data);

            const options = {
                id: "RFID_Gird",
                type: "grid",
                data: datas,
                className: "datatable-grid form-group col-3fr mt-2 mt-md-0",
                bodyClassName: "h-100 overflow-auto",
                items: {
                    thead: true,
                    metaKey: "InternalCode",
                    columns: [
                        {
                            id: "_Edit",
                            type: "btn",
                            className: "p-2 d-flex justify-content-center gap-2",
                            width: 80,
                            button: {
                                text: "編輯",
                                className: "btn btn-datagrid",
                                onClick: (e, v, row) => {
                                    bsModal.setData(row);
                                    bsModal.show();
                                }
                            }
                        },
                        { id: "InternalCode", title: "RFID內碼", width: 130 },
                        { id: "ExternalCode", title: "RFID外碼", width: 130 },
                        { id: "StockType", title: "類別", width: 130 },
                        { id: "StockName", title: "品項名稱", width: 200 },
                        { id: "NumberOfChanges", title: "入庫數量", width: 100 },
                        { id: "Unit", title: "單位", width: 80 },
                        {
                            id: "_Delete",
                            type: "btn",
                            className: "p-1 pt-2",
                            width: 48,
                            button: {
                                className: "btn-delete-item",
                                onClick: (e, v, row) => {
                                    const rowDOM = document.querySelector(`#RFID_Gird [data-internal-code="${row.InternalCode}"]`);
                                    rowDOM.remove();

                                    const index = datas.findIndex(x => x.InternalCode === row.InternalCode)
                                    if (index !== -1) {
                                        datas.splice(index, 1);
                                    }

                                    if (datas.length === 0) {
                                        document.getElementById(options.id).remove();
                                    }

                                    checkMaxRowSize()
                                }
                            }
                        },
                    ]
                }
            }

            document.getElementById(options.id)?.remove();
            DT.createTable(`[data-tab-content="RFID"]`, options)
            checkMaxRowSize()
        }

        function checkMaxRowSize(n = MAX_ROW_SIZE) {
            const btn = document.getElementById("rfid")
            if (datas.length >= n) {
                btn.disabled = true;
            }
            else {
                btn.disabled = false;
            }
        }
    }

    function checkSave() {
        //檢查required
        const form = document.querySelector("form.form");
        if (!form.reportValidity()) {
            return;
        }
        if (!RFIDModal.datas.length === 0) {
            DT.createDialogModal("請先掃描RFID！")
            return;
        }

        DT.createDialogModal({
            id: "DialogModal-CreateStock",
            inner: `<div class="d-flex justify-content-center align-items-center gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                    class="bi bi-exclamation-triangle-fill" viewBox="0 0 16 16">
                    <path
                        d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z" />
                </svg>
                新增後無法更改，確認是否新增入庫?？
            </div>`,
            button: [
                { className: "btn btn-cancel", text: "取消", cancel: true, },
                { className: "btn btn-delete", text: "確定新增", cancel: true, onClick: save, },
            ]
        })
    }

    function save() {
        const fd = new FormData();
        let submitUrl = '';
        switch (formTab.currentTab) {
            case 'Normal':
                submitUrl = '/Stock_Management/CreateNormalComputationalStockIn';
                fd.append("SISN", document.getElementById('SISN').value)
                fd.append("NumberOfChanges", document.getElementById('NumberOfChanges').value)
                break;
            case 'RFID':
                submitUrl = '/StockRFID_Management/CreateRFIDStockIn';
                const d = RFIDModal.datas[0];
                fd.append("RFIDInternalCode", d.InternalCode)
                fd.append("RFIDExternalCode", d.ExternalCode)
                fd.append("StockName", d.SISN)
                break;
        }

        if (fileUploader.hasFile()) {
            fd.append("PurchaseOrder", fileUploader.getFile())
        }
        fd.append("Memo", document.getElementById('Memo').value)
        fd.append("__RequestVerificationToken", document.querySelector('[name="__RequestVerificationToken"]').value)

        console.log(Object.fromEntries(fd.entries()));
        $.ajax({
            url: submitUrl,
            data: fd,
            type: "POST",
            contentType: false,
            processData: false,
            success(res) {
                console.log(res);
                if (res.ErrorMessage) {
                    DT.createDialogModal(`新增失敗！${res?.ErrorMessage || ""}`)
                    return;
                }

                DT.createDialogModal({
                    id: "DialogModal-Success", inner: "新增成功！",
                    onHide: () => { window.location.href = "/Stock_Management/Index"; }
                });
            },
            error(res) {
                console.log(res);
                DT.createDialogModal(`新增失敗！${res?.ErrorMessage || ""}`)
            }
        })
    }
}
