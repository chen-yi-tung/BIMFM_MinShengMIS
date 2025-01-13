const DEBUG_TEST = false;
async function init_CreateOutbound() {
    document.getElementById("back").onclick = () => history.back();
    document.getElementById("submit").onclick = () => checkSave();
    const RFIDScanBtn = new RFID_ScanButton({
        id: "rfid",
        fake: DEBUG_TEST,
        onScanEnd: async (RFID) => {
            //檢查有無重複
            const exist = RFIDGrid.grid.getRowIndex(RFID);
            if (exist !== -1) {
                DT.createDialogModal("此RFID已存在！");
                return;
            }

            if (DEBUG_TEST) {
                //放入資料，顯示資料
                const data = {
                    RFIDInternalCode: RFID,
                    RFIDExternalCode: "RFIDExternalCode",
                    StockType: "StockType",
                    StockName: "StockName",
                    Unit: "Unit",
                }
                RFIDModal.setData({
                    InternalCode: data.RFIDInternalCode,
                    ExternalCode: data.RFIDExternalCode,
                    StockType: data.StockType,
                    StockName: data.StockName,
                    NumberOfChanges: 1,
                    Unit: data.Unit,
                });
                RFIDModal.show();
                return;
            }

            //後端取得RFID對應設備
            const data = await $.getJSON(`/StockRFID_Management/StockOutDetail?RFIDInternalCode=${RFID}`)
                .then((res) => {
                    if (res.ErrorMessage) {
                        DT.createDialogModal("取得RFID對應設備失敗！<br>" + res.ErrorMessage);
                        return null;
                    }
                    return res.Datas;
                })
                .catch((ex) => {
                    DT.createDialogModal("取得RFID對應設備失敗！" + ex.responseText);
                    return null;
                });
            if (!data) return;

            //放入資料，顯示資料
            RFIDModal.setData({
                InternalCode: data.RFIDInternalCode,
                ExternalCode: data.RFIDExternalCode,
                StockType: data.StockType,
                StockName: data.StockName,
                NumberOfChanges: 1,
                Unit: data.Unit,
            });
            RFIDModal.show();
        }
    });
    const RFIDModal = new RFID_Modal({
        template: document.getElementById("RFIDModal"),
        submitButtonSelector: "#add-row",
        setData(data = {}) {
            const table = this.modal.querySelector("#RFID-DataTable");
            table.replaceChildren();
            DT.createTableInner(table, data, [
                { id: "InternalCode", title: "RFID內碼" },
                { id: "ExternalCode", title: "RFID外碼" },
                { id: "StockType", title: "類別" },
                { id: "StockName", title: "品項名稱" },
                {
                    id: "NumberOfChanges",
                    title: "領取數量",
                    formatter(v, row) {
                        return `${v}${row.Unit}`;
                    },
                },
            ]);
            this.data = data;
        },
        getData() {
            return this.data;
        },
        async submit() {
            RFIDGrid.add(this.getData());
            this.hide();
        },
        reset() {
            this.setData();
        }
    });
    const RFIDGrid = new RFID_Grid({
        container: document.querySelector(`[data-tab-content="RFID"]`),
        maxRowSize: 100,
        grid: {
            id: "RFID_Gird",
            type: "grid",
            className: "datatable-grid form-group col-3fr mt-2 mt-md-0",
            bodyClassName: "h-100 overflow-auto",
            items: {
                thead: true,
                metaKey: "InternalCode",
                columns: [
                    { id: "InternalCode", title: "RFID內碼", width: 130 },
                    { id: "ExternalCode", title: "RFID外碼", width: 130 },
                    { id: "StockType", title: "類別", width: 130 },
                    { id: "StockName", title: "品項名稱", width: 200 },
                    { id: "NumberOfChanges", title: "領取數量", width: 100 },
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

    await formDropdown.StockTypeSN({ unitId: document.querySelector("#NumberOfChanges+.form-unit") });
    const formTab = DT.setupFormTab();

    function checkSave() {
        //檢查required
        const form = document.querySelector("form.form");
        if (!form.reportValidity()) {
            return;
        }
        if (!RFIDGrid.data.length === 0) {
            DT.createDialogModal("請先掃描RFID！");
            return;
        }

        DT.createDialogModal({
            id: "DialogModal-CreateStock",
            inner: `<div class="d-flex justify-content-center align-items-center gap-2">
                <i class="fa-solid fa-triangle-exclamation" style="font-size: 24px;"></i>
                新增後無法更改，確認是否新增出庫？
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
                submitUrl = "/Stock_Management/CreateNormalComputationalStockOut";
                fd.append("SISN", document.getElementById("SISN").value);
                fd.append("NumberOfChanges", document.getElementById("NumberOfChanges").value);
                break;
            case "RFID":
                submitUrl = "/StockRFID_Management/RFIDStockOut";
                RFIDGrid.data.forEach((d, i) => {
                    fd.append(`RFIDInternalCodes[${i}]`, d.InternalCode);
                });
                break;
        }
        fd.append("Recipient", document.getElementById("Recipient").value);
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
                    DT.createDialogModal(`新增出庫失敗！${res?.ErrorMessage || ""}`);
                    return;
                }

                DT.createDialogModal({
                    id: "DialogModal-Success",
                    inner: "新增出庫成功！",
                    onHide: () => {
                        window.location.href = "/Stock_Management/Index";
                    },
                });
            },
            error(res) {
                console.log(res);
                DT.createDialogModal(`新增出庫失敗！${res?.ErrorMessage || ""}`);
            },
        });
    }
}
