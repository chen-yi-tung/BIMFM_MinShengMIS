async function init_CreateOutbound() {
    const MAX_ROW_SIZE = 100;
    document.getElementById("rfid").onclick = () => checkRFID();
    document.getElementById("back").onclick = () => history.back();
    document.getElementById("submit").onclick = () => checkSave();
    const RFIDScanBtn = await init_RFIDScanBtn();
    const formTab = DT.setupFormTab();

    await formDropdown.StockTypeSN();

    const RFIDModal = await init_RFIDModal();
    const RFIDGrid = await init_RFIDGrid();
    async function init_RFIDScanBtn() {
        const btn = document.getElementById("rfid");
        const icon = btn.querySelector(".scan-icon");
        btn.onclick = async () => {
            setLoading(true);
            await checkRFID();
            setLoading(false);
        };
        function setLoading(b) {
            if (b) {
                btn.disabled = true;
                icon.className = "spinner-border spinner-border-sm";
            } else {
                btn.disabled = false;
                icon.className = "scan-icon";
            }
        }
        return {
            setLoading,
        };
    }
    async function init_RFIDModal() {
        const modalTemplate = document.getElementById("RFIDModal");
        const modal = modalTemplate.content.cloneNode(true).firstElementChild;
        const bsModal = bootstrap.Modal.getOrCreateInstance(modal);
        modalTemplate.remove();

        //隱藏時移除元素
        modal.addEventListener("hidden.bs.modal", () => {
            modal.remove();
        });

        //顯示時設定資料
        bsModal.data = null;
        bsModal.setData = (data = {}) => {
            const table = modal.querySelector("#RFID-DataTable");
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
            bsModal.data = data;
        };

        //儲存時取得資料
        bsModal.getData = () => {
            return JSON.parse(JSON.stringify(bsModal.data));
        };

        //儲存
        modal.querySelector("#add-row").addEventListener("click", async () => {
            bsModal.hide();
            RFIDGrid.add(bsModal.getData());
        });

        return bsModal;
    }
    async function init_RFIDGrid() {
        const datas = [];
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
                                remove(row);
                            },
                        },
                    },
                ],
            },
        };
        function getRow(key) {
            return document.querySelector(`#${options.id} tr[data-meta-key="${key}"]`);
        }
        function getRowData(key) {
            const index = getRowIndex(key);
            return datas[index];
        }
        function getRowIndex(key) {
            const index = datas.findIndex((x) => x[options.items.metaKey] === key);
            if (index === -1) {
                console.error("[RFIDGird.getRowData] Not found row");
                return;
            }
            return index;
        }
        function add(d) {
            datas.push(d);
            document.getElementById(options.id)?.remove();
            DT.createTable(`[data-tab-content="RFID"]`, options);
            checkMaxRowSize();
        }
        function remove(row) {
            const rowDOM = getRow(row.InternalCode);
            rowDOM.remove();

            const index = getRowIndex(row.InternalCode);
            if (index !== -1) {
                datas.splice(index, 1);
            }

            if (datas.length === 0) {
                document.getElementById(options.id).remove();
            }
            checkMaxRowSize();
        }
        function checkMaxRowSize(n = MAX_ROW_SIZE) {
            const btn = document.getElementById("rfid");
            if (datas.length >= n) {
                btn.disabled = true;
            } else {
                btn.disabled = false;
            }
        }
        return {
            datas,
            options,
            add,
            remove,
            getRow,
            getRowData,
            getRowIndex,
        };
    }
    async function checkRFID() {
        //後端取得RFID
        const RFID = await $.getJSON(`/RFID/CheckRFID`)
            .then((res) => {
                if (res.ErrorMessage) {
                    DT.createDialogModal("掃描失敗！" + res.ErrorMessage);
                    return null;
                }
                return res.Datas.trim();
            })
            .catch((ex) => {
                DT.createDialogModal("掃描失敗！" + ex.responseText);
                return null;
            });
        if (!RFID) return;

        console.log("checkRFID", RFID);

        //檢查有無重複
        const exist = RFIDGrid.datas.findIndex((d) => d.InternalCode === RFID);
        if (exist !== -1) {
            DT.createDialogModal("此RFID已存在！");
            return;
        }

        //後端取得RFID對應設備
        const data = await $.getJSON(`/StockRFID_Management/StockOutDetail?RFIDInternalCode=${RFID}`)
            .then((res) => {
                if (res.ErrorMessage) {
                    DT.createDialogModal("取得RFID對應設備失敗！" + res.ErrorMessage);
                    return null;
                }
                return res.Datas;
            })
            .catch((ex) => {
                DT.createDialogModal("取得RFID對應設備失敗！" + ex.responseText);
                return null;
            });

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
    function checkSave() {
        //檢查required
        const form = document.querySelector("form.form");
        if (!form.reportValidity()) {
            return;
        }
        if (!RFIDGrid.datas.length === 0) {
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
                RFIDGrid.datas.forEach((d, i) => {
                    fd.append(`RFIDInternalCodes[${i}]`, d.InternalCode);
                });
                break;
        }
        fd.append("Recipient", document.getElementById("Recipient").value);
        fd.append("Memo", document.getElementById("Memo").value);
        fd.append("__RequestVerificationToken", document.querySelector('[name="__RequestVerificationToken"]').value);

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
