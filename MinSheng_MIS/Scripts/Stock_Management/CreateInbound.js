async function init_CreateInbound() {
    const MAX_ROW_SIZE = 1; //只能有一筆RFID資料

    document.getElementById("back").onclick = () => history.back();
    document.getElementById("submit").onclick = () => checkSave();

    const RFIDScanBtn = await init_RFIDScanBtn();
    const formTab = DT.setupFormTab();
    const fileUploader = new FileUploader({
        container: "#FileUploader",
        className: "form-group col-3fr",
        required: false,
        accept: [".jpg", ".jpeg", ".png", ".pdf"],
        label: "採購單據",
        id: "PurchaseOrder",
    });

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
        await formDropdown.StockTypeSN({ id: modal.querySelector("#StockTypeSN"), sisnId: modal.querySelector("#SISN") });

        //隱藏時移除元素
        modal.addEventListener("hidden.bs.modal", () => {
            modal.remove();
        });

        bsModal.data = null;
        //編輯時設定資料
        bsModal.setData = (data = {}) => {
            modal.querySelector("#isEdit").value = data.isEdit ?? false;
            modal.querySelector("#InternalCode").value = data.InternalCode ?? "";
            modal.querySelector("#ExternalCode").value = data.ExternalCode ?? "";
            modal.querySelector("#StockTypeSN").value = data.StockTypeSN ?? "";
            formDropdown.StockTypeSN({
                id: modal.querySelector("#StockTypeSN"),
                sisnId: modal.querySelector("#SISN"),
                value: data.StockTypeSN,
                sisnValue: data.SISN,
            });
            modal.querySelector("#NumberOfChanges").value = data.NumberOfChanges ?? 1;
            bsModal.data = data;
        };

        //儲存時取得資料

        bsModal.getData = () => {
            const isEdit = modal.querySelector("#isEdit").value ?? false;
            const InternalCode = modal.querySelector("#InternalCode").value ?? null;
            const ExternalCode = modal.querySelector("#ExternalCode").value ?? null;
            const StockTypeSN = modal.querySelector("#StockTypeSN").value ?? null;
            const SISN = modal.querySelector("#SISN").value ?? null;
            const NumberOfChanges = modal.querySelector("#NumberOfChanges").value ?? null;
            bsModal.data = { InternalCode, ExternalCode, StockTypeSN, SISN, NumberOfChanges, isEdit };
            return bsModal.data;
        };

        //儲存
        modal.querySelector("#add-row").addEventListener("click", async () => {
            const form = modal.querySelector("form");
            if (!form.reportValidity()) {
                return;
            }
            bsModal.hide();
            const originData = bsModal.getData();
            if (originData.isEdit === "true") {
                RFIDGrid.edit(originData);
                return;
            }
            const res = await $.getJSON(`/Stock_Management/GetComputationalStockDetail?id=${originData.SISN}`).then((res) => res.Datas);
            const data = Object.assign(res, originData);
            data.isEdit = true;
            RFIDGrid.add(data);
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
        function add(row) {
            datas.push(row);
            document.getElementById(options.id)?.remove();
            DT.createTable(`[data-tab-content="RFID"]`, options);
            checkMaxRowSize();
        }
        function edit(row) {
            const rowData = getRowData(row.InternalCode);
            Object.assign(rowData, row);
            const rowDOM = getRow(row.InternalCode);
            for (const [k, v] of Object.entries(row)) {
                const td = rowDOM.querySelector("#d-" + k);
                if (td) {
                    td.textContent = v;
                }
            }
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
            add,
            edit,
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

        //放入資料，顯示表單
        RFIDModal.setData({
            InternalCode: RFID,
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
                const d = RFIDGrid.datas[0];
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
