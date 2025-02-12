class RFID_ScanButton {
    #loading = false;
    #disabled = false;
    constructor({ id = "rfid", type, fake = false, onScanEnd = null, disabled = false } = {}) {
        this.onScanEnd = onScanEnd.bind(this);
        this.#disabled = disabled;
        this.fake = fake;
        this.type = type;
        this.element = id instanceof HTMLElement ? id : document.getElementById(id) ?? document.querySelector(id);
        this.init();
    }
    get loading() {
        return this.#loading;
    }
    set loading(b) {
        this.#loading = b;
        if (b) {
            this.element.disabled = true;
            this.icon.className = "spinner-border spinner-border-sm";
        } else {
            this.element.disabled = this.#disabled;
            this.icon.className = "scan-icon";
        }
    }
    get disabled() {
        return this.#disabled;
    }
    set disabled(b) {
        this.#disabled = b;
        this.element.disabled = b;
    }
    init() {
        if (!this.element) {
            console.error("[RFID_ScanButton] No Found element.");
            return;
        }
        this.icon = this.element.querySelector(".scan-icon");
        if (!this.icon) {
            this.icon = document.createElement("i");
            this.element.appendChild(this.icon);
        }
        this.element.addEventListener("click", () => {
            this.loading = true;
            this.scan().finally(() => {
                this.loading = false;
            });
        });
        this.loading = false;
        this.element.disabled = this.#disabled;
    }
    async scan() {
        if (this.fake) {
            let RFID;
            if (typeof this.fake === "string") {
                RFID = this.fake;
            }
            RFID =
                "FAKERFID" +
                Math.random()
                    .toString(36)
                    .slice(2, 33 - 2);
            this.onScanEnd(RFID);
            return;
        }
        //後端取得RFID
        //RFID_ScanButton.type = 1:設備掃描、2:入庫掃描、3:出庫掃描
        const RFID = await $.get(`/RFID/CheckRFID`, {
            id: this.type
        })
            .then((res) => {
                if (res.ErrorMessage) {
                    DT.createDialogModal("掃描失敗！<br>" + res.ErrorMessage);
                    return null;
                }
                return res.Datas.trim();
            })
            .catch((ex) => {
                DT.createDialogModal("掃描失敗！" + ex.responseText);
                return null;
            });
        if (!RFID) return null;

        console.log("scan", RFID);

        if (typeof this.onScanEnd === "function") {
            this.onScanEnd(RFID);
        }
        return RFID;
    }
}

class RFID_Modal {
    #init = null;
    constructor({
        template,
        submitButtonSelector = "#add-row",
        init = () => { },
        setData = () => { },
        getData = () => { },
        submit = () => { },
        reset = () => { },
    } = {}) {
        const modalTemplate = template;
        this.modal = modalTemplate.content.cloneNode(true).firstElementChild;
        this.bsModal = bootstrap.Modal.getOrCreateInstance(this.modal);
        modalTemplate.remove();
        this.setData = setData.bind(this);
        this.getData = getData.bind(this);
        this.init = init.bind(this);
        this.submit = submit.bind(this);
        this.reset = reset.bind(this);
        this.modal.querySelector(submitButtonSelector).addEventListener("click", async () => {
            this.submit();
        });
        this.#init = init.bind(this);
    }
    init() {
        //隱藏時移除元素
        this.modal.addEventListener("hidden.bs.modal", () => {
            this.modal.remove();
            this.reset();
        });
        this.#init();
        return this;
    }
    show() {
        this.bsModal.show();
    }
    hide() {
        this.bsModal.hide();
    }
}

class RFID_Grid {
    constructor({ container, maxRowSize = 1, grid = {}, onAdd = () => { }, onEdit = () => { }, onRemove = () => { } } = {}) {
        this.data = [];
        this.maxRowSize = maxRowSize;
        grid.data = this.data;
        this.grid = DT.createTable(container, grid);
        this.grid.checkTableShow();
        this.onAdd = onAdd.bind(this);
        this.onEdit = onEdit.bind(this);
        this.onRemove = onRemove.bind(this);
    }
    add(row) {
        this.grid.add(row);
        this.onAdd(row);
    }
    edit(row) {
        this.grid.edit(row);
        this.onEdit(row);
    }
    remove(row) {
        this.grid.remove(row);
        this.onRemove(row);
    }
    checkMaxRowSize() {
        //true is over max size
        return this.data.length >= this.maxRowSize;
    }
}

class RFID_Location_Modal {
    constructor({
        template = null,
        columns = [
            { title: "名稱", id: "Name" },
            { title: "棟別", id: "Area" },
            { title: "樓層", id: "Floor" },
        ],
    } = {}) {
        const modalTemplate = template ?? document.createElement("template");
        if (!template) {
            modalTemplate.innerHTML = `<div class="modal fade data-detail-modal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title w-100 text-center">定位資訊</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <table class="datatable-table"></table>
                        </div>
                    </div>
                </div>
            </div>`.trim();
        }

        this.modal = modalTemplate.content.cloneNode(true).firstElementChild;
        this.bsModal = bootstrap.Modal.getOrCreateInstance(this.modal);
        this.table = this.modal.querySelector("table");
        this.columns = columns;
        this.columns.push({
            title: "定位",
            id: "Location",
            formatter: () => {
                const container = document.createElement("div");
                container.id = "BIM";
                container.style.height = 240 + "px";
                return container;
            },
        });

        modalTemplate.remove();

        this.modal.addEventListener("show.bs.modal", async () => {
            this.bimInit();
        });

        this.modal.addEventListener("hidden.bs.modal", async () => {
            this.bimDispose();
        });
    }
    setData(data) {
        this.data = data;
        this.table.innerHTML = "";
        DT.createTableInner(this.table, this.data, this.columns);
    }
    show() {
        this.bsModal.show();
    }
    hide() {
        this.bsModal.hide();
    }
    async bimInit() {
        if (!this.viewNames) {
            this.viewNames = await $.getJSON("/DropDownList/ViewName");
        }
        if (this.bim) {
            this.bimDispose();
        }
        const container = this.modal.querySelector("#BIM");
        if (!container) return;
        this.bim = new UpViewer(container);
        await this.bim.init();

        if (!this.data?.FSN) return;
        const viewName = this.viewNames.find((x) => x.FSN === this.data.FSN)?.Value;
        if (!viewName) {
            console.error("[RFIDModal FSN change event] Not found ViewName.");
            return;
        }
        await this.bim.loadModels(this.bim.getModelsUrl(viewName));
        if (this.data?.LocationX && this.data?.LocationY) {
            this.bim.activateEquipmentPointTool(new THREE.Vector3(this.data.LocationX, this.data.LocationY, 0), false);
        }
    }
    bimDispose() {
        if (this.bim === null) {
            return;
        }
        this.bim.unloadModels();
        this.bim.dispose();
        this.bim = null;
    }
}
