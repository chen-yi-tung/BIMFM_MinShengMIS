const DEBUG_TEST = false;
async function init_CreateInbound() {
    document.getElementById("back").onclick = () => history.back();
    document.getElementById("submit").onclick = () => checkSave();

    const RFIDScanBtn = new RFID_ScanButton({
        id: "rfid",
        type: "2",
        fake: DEBUG_TEST,
        onScanEnd: (RFID) => {
            //檢查有無重複
            const exist = RFIDGrid.grid.getRowIndex(RFID);
            if (exist !== -1) {
                DT.createDialogModal("此RFID已存在！");
                return;
            }

            //放入資料，顯示表單
            RFIDModal.setData({
                InternalCode: RFID,
            });
            RFIDModal.show();
        }
    });
    const RFIDModal = new RFID_Modal({
        template: document.getElementById("RFIDModal"),
        submitButtonSelector: "#add-row",
        init() {
            formDropdown.StockTypeSN({
                id: this.modal.querySelector("#StockTypeSN"),
                sisnId: this.modal.querySelector("#SISN"),
                unitId: this.modal.querySelector("#NumberOfChanges+.form-unit"),
            });
        },
        setData(data = {}) {
            this.modal.querySelector("#isEdit").value = data.isEdit ?? false;
            this.modal.querySelector("#InternalCode").value = data.InternalCode ?? "";
            this.modal.querySelector("#ExternalCode").value = data.ExternalCode ?? "";
            this.modal.querySelector("#StockTypeSN").value = data.StockTypeSN ?? "";
            formDropdown.StockTypeSN({
                id: this.modal.querySelector("#StockTypeSN"),
                sisnId: this.modal.querySelector("#SISN"),
                value: data.StockTypeSN,
                sisnValue: data.SISN,
            });
            this.modal.querySelector("#NumberOfChanges").value = data.NumberOfChanges ?? 1;
        },
        getData() {
            const isEdit = this.modal.querySelector("#isEdit").value ?? false;
            const InternalCode = this.modal.querySelector("#InternalCode").value ?? null;
            const ExternalCode = this.modal.querySelector("#ExternalCode").value ?? null;
            const StockTypeSN = this.modal.querySelector("#StockTypeSN").value ?? null;
            const StockType = this.modal.querySelector(`#StockTypeSN option[value="${StockTypeSN}"]`).textContent ?? null;
            const SISN = this.modal.querySelector("#SISN").value ?? null;
            const StockName = this.modal.querySelector(`#SISN option[value="${SISN}"]`).textContent ?? null;
            const NumberOfChanges = this.modal.querySelector("#NumberOfChanges").value ?? null;
            const data = { InternalCode, ExternalCode, StockTypeSN, StockType, SISN, StockName, NumberOfChanges, isEdit };
            return data;
        },
        async submit() {
            const form = this.modal.querySelector("form");
            if (!form.reportValidity()) {
                return;
            }
            const originData = this.getData();
            const res = await $.getJSON(`/Stock_Management/GetComputationalStockDetail?id=${originData.SISN}`).then((res) => res.Datas);
            const data = Object.assign(res, originData);

            if (data.isEdit === "true") {
                RFIDGrid.edit(data);
                this.hide();
                return;
            }
            data.isEdit = true;
            RFIDGrid.add(data);
            this.hide();
        },
        reset() {
            this.setData();
        }
    });
    const RFIDGrid = new RFID_Grid({
        container: document.querySelector(`[data-tab-content="RFID"]`),
        maxRowSize: 1, //只能有一筆RFID資料
        grid: {
            id: "RFID_Gird",
            type: "grid",
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
                            onClick(e, v, row) {
                                row.isEdit = true;
                                RFIDModal.setData(row);
                                RFIDModal.show();
                            },
                        },
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
                            onClick(e, v, row) {
                                RFIDGrid.remove(row);
                            },
                        },
                    },
                ],
            },
        },
        onAdd() {
            RFIDScanBtn.disabled = RFIDGrid.checkMaxRowSize()
        },
        onRemove() {
            RFIDScanBtn.disabled = RFIDGrid.checkMaxRowSize()
        }
    });
    RFIDModal.init();
    
    const formTab = DT.setupFormTab();
    const fileUploader = new FileUploader({
        container: "#FileUploader",
        className: "form-group col-3fr",
        required: false,
        accept: [".jpg", ".jpeg", ".png", ".pdf"],
        label: "採購單據",
        id: "PurchaseOrder",
    });

    await formDropdown.StockTypeSN({ unitId: document.querySelector("#NumberOfChanges+.form-unit") });

    function checkSave() {
        //檢查required
        const form = document.querySelector("form.form");
        if (!form.reportValidity()) {
            return;
        }
        if (!RFIDGrid.data?.length) {
            DT.createDialogModal("請先掃描RFID！");
            return;
        }

        DT.createDialogModal({
            id: "DialogModal-CreateStock",
            inner: `<div class="d-flex justify-content-center align-items-center gap-2">
                <i class="fa-solid fa-triangle-exclamation" style="font-size: 24px;"></i>
                新增後無法更改，確認是否新增入庫？
            </div>`,
            button: [
                { className: "btn btn-cancel", text: "取消", cancel: true },
                { className: "btn btn-delete", text: "確定新增", cancel: true, onClick: save },
            ],
        });
    }
    function save() {
        const fd = new FormData();
        let submitUrl = "";
        switch (formTab.currentTab) {
            case "Normal":
                submitUrl = "/Stock_Management/CreateNormalComputationalStockIn";
                fd.append("SISN", document.getElementById("SISN").value);
                fd.append("NumberOfChanges", document.getElementById("NumberOfChanges").value);
                break;
            case "RFID":
                submitUrl = "/StockRFID_Management/CreateRFIDStockIn";
                const d = RFIDGrid.data[0];
                fd.append("RFIDInternalCode", d.InternalCode);
                fd.append("RFIDExternalCode", d.ExternalCode);
                fd.append("StockName", d.SISN);
                break;
        }

        if (fileUploader.hasFile()) {
            fd.append("PurchaseOrder", fileUploader.getFile());
        }
        fd.append("Memo", document.getElementById("Memo").value);
        fd.append("__RequestVerificationToken", document.querySelector('[name="__RequestVerificationToken"]').value);

        console.log(Object.fromEntries(fd.entries()));
        if (DEBUG_TEST) return;
        $.ajax({
            url: submitUrl,
            data: fd,
            type: "POST",
            contentType: false,
            processData: false,
            success(res) {
                console.log(res);
                if (res.ErrorMessage) {
                    DT.createDialogModal(`新增入庫失敗！${res?.ErrorMessage || ""}`);
                    return;
                }

                DT.createDialogModal({
                    id: "DialogModal-Success",
                    inner: "新增入庫成功！",
                    onHide: () => {
                        window.location.href = "/Stock_Management/Index";
                    },
                });
            },
            error(res) {
                console.log(res);
                DT.createDialogModal(`新增入庫失敗！${res?.responseText || ""}`);
            },
        });
    }
}
