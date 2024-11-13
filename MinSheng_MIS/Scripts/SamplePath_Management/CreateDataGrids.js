/*新增巡檢設備*/
const AEDGOptions = {
    mdg: '#AddEquipment .modal-datagrid',
    edg: '#Template-datagrid',
    id: "AddEquipment",
    type: "Template",
    initDatagridUrl: "/Datagrid/SamplePath_Management",
    appendDataUrl: "/SamplePath_Management/AddReportForm",
    appendDataKey: "RSN",
    removeDataUrl: "/SamplePath_Management/DeleteReportForm",
    removeDataKey: "RSN",
    filterCheckKey: "",
    datagridOptions: {
        idField: 'RSN',
        remoteSort: false,
        sortOrder: 'asc',
        singleSelect: true,
        selectOnCheck: false,
        checkOnSelect: false,
    },
    columns: [[
        { field: 'EquipStatenum', hidden: true },
        { field: 'InterCode', title: 'RFID內碼', align: 'center', width: 130, sortable: true },
        { field: 'ExterCode', title: 'RFID外碼', align: 'center', width: 130, sortable: true },
        { field: 'EName', title: '設備名稱', align: 'center', width: 250, sortable: true },
        { field: 'EStatus', title: '設備狀態', align: 'center', width: 120, sortable: true },
        { field: 'ESN', title: '設備編號', align: 'center', width: 120, sortable: true },
        { field: 'Area', title: '棟別', align: 'center', width: 120, sortable: true },
        { field: 'Floor', title: '樓層', align: 'center', width: 120, sortable: true },
        { field: 'Brand', title: '廠牌', align: 'center', width: 120, sortable: true },
        { field: 'Model', title: '型號', align: 'center', width: 120, sortable: true },
        { field: 'Frequency', title: '巡檢頻率', align: 'center', width: 120, sortable: true },
    ]],
    frozenColumns: [[
        { field: '_select', checkbox: true, },
        //{
        //    //field: 'action', align: 'center', width: 40, formatter: function (value, row, index) {
        //    //    return `<input type="radio" name="dgRadio">`;
        //    //},
        //},
    ]],
    pageOptions: {
        pageSize: 10,
        showPageList: true,
        pageList: [10, 20, 50],
        beforePageText: '第',
        afterPageText: '頁，共 {pages} 頁',
        displayMsg: '顯示 {from} 到 {to} 筆資料，共 {total} 筆資料'
    },
    evnet: {
        detail: (row, index) => { window.open(`/SamplePath_Management/Read/${row.RSN}`, "_blank"); },
    }
}

function AEDG(options) {
    const self = this;
    this.options = options;
    this.mdg = $(this.options.mdg);
    this.edg = $(this.options.edg);
    this.modal = $("#" + this.options.id);
    this.searchBtn = $(`#${this.options.id} #search`);
    this.addRowBtn = $(`#${this.options.id} #add-row`);
    this.createBtn = $(`#${this.options.type}-create`);
    this.deleteBtn = $(`#${this.options.type}-delete`);
    this.event = this.options.evnet;
    this.observer = null;
    this.appendData = function () {
        let data = self.mdg.datagrid("getChecked");
        console.log("appendData POST", data);

        let btn = self.createBtn;
        self.addSpinner(btn);

        getDeviceData(data.map(d => d.ESN));

        $.ajax({
            url: self.options.appendDataUrl,
            data: JSON.stringify(data.map(d => d[self.options.appendDataKey])),
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: onSuccess,
            error: onError
        })

        function onSuccess(res) {
            console.log("appendData onSuccess", res)
            self.removeSpinner(btn);
            bootstrap.Modal.getInstance(self.modal[0]).hide();

            res.rows.forEach((d) => { self.edg.datagrid("appendRow", d) })

            reloadDeviceData();
        }
        function onError(err) {
            self.removeSpinner(btn);
            createDialogModal({ id: "DialogModal-Error", inner: "新增失敗！", })
        }
    }
    this.removeData = function () {
        let data = self.edg.datagrid("getChecked").map((d) => { return d[self.options.removeDataKey] });
        console.log(data);
        let btn = self.deleteBtn;
        self.addSpinner(btn);

        $.ajax({
            url: self.options.removeDataUrl,
            data: JSON.stringify(data),
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: onSuccess,
            error: onError
        })

        function onSuccess(res) {
            console.log(res);
            self.removeSpinner(btn);

            createDialogModal({ id: "DialogModal-Success", inner: "刪除成功！", })

            res.forEach((d) => { self.edg.datagrid("deleteRow", self.edg.datagrid("getRowIndex", d[self.options.removeDataKey])) })

            reloadDeviceData();

            self.edg.datagrid("clearChecked");

            if (self.edg.datagrid("getRows").length === 0) {
                self.changeEditAreaCss(true)
            }
        }
        function onError(err) {
            self.removeSpinner(btn);
            createDialogModal({ id: "DialogModal-Error", inner: "刪除失敗！", })
            self.edg.datagrid("clearChecked");
        }
    }
    this.initDatagrid = function (dg) {
       
        dg.datagrid(Object.assign({}, self.options.datagridOptions,
            {
                //url: self.options.initDatagridUrl,
                //method: 'POST',
                data: [
                    {
                        InterCode: "XSCXCZS",
                        ExterCode: "XSCXCZS",
                        EName: "螺旋發送機",
                        EStatus: "正常",
                        ESN: "SP-0202A",
                        Area: "前處理機房",
                        Floor: "1F",
                        Brand: "XX廠牌",
                        Model: "W004N",
                        Frequency: "每2小時"
                    },
                    {
                        InterCode: "XSCXCZS",
                        ExterCode: "XSCXCZS",
                        EName: "螺旋發送機",
                        EStatus: "正常",
                        ESN: "SP-0202A",
                        Area: "前處理機房",
                        Floor: "1F",
                        Brand: "XX廠牌",
                        Model: "W004N",
                        Frequency: "每2小時"
                    },

                ],
                queryParams: getQueryParams(`#${self.options.id} form`),
                fit: true,
                pagination: self.options.pageOptions.showPageList,
                pagePosition: 'bottom',
                pageSize: 10,
                frozenColumns: self.options.frozenColumns,
                columns: self.options.columns,
                onLoadSuccess: (data) => { self.filterCheck(dg, data, self.options.filterCheckKey) }
            }))
        $(dg.datagrid('getPager')).pagination(self.options.pageOptions);
        self.addButtonEvent(dg);
    }
    this.initResultDatagrid = function (dg) {
        dg.datagrid(Object.assign({}, self.options.datagridOptions,
            {
                frozenColumns: self.options.frozenColumns,
                columns: self.options.columns,
            }))

        if (this.observer == null) {
            this.observer = new ResizeObserver((entries) => {
                dg.datagrid('resize');
            });
            this.observer.observe(dg.closest(".datatable-easyui")[0]);
        }
        self.addButtonEvent(dg);
    }

    this.modal.one("shown.bs.modal", () => {
        self.initDatagrid(self.mdg);
        self.modal.on("shown.bs.modal", () => { self.mdg.datagrid("load", getQueryParams(`#${self.options.id} form`)) });
        self.modal.on("hidden.bs.modal", () => { $(`#${self.options.id} form`)[0].reset(); self.mdg.datagrid("clearChecked"); });
    });

    this.searchBtn.click(() => { self.loadDatagrid(self.options.id) });

    this.addRowBtn.on("click", () => { self.addRowEvent() });

    this.deleteBtn.on("click", () => { if (this.edg.datagrid("getChecked").length !== 0) { this.removeData(); } });

    return this;
}

AEDG.prototype.changeEditAreaCss = function (reserve = false) {
    let selfJQ = this.createBtn;
    let parent = selfJQ.parent();

    if (reserve) {
        selfJQ.siblings().addClass("d-none");
        parent.addClass("mt-0");
        parent.siblings(".panel-htop").addClass("d-none");
        parent.siblings(".form-group").addClass("d-none");
        return;
    }

    selfJQ.siblings().removeClass("d-none");
    parent.removeClass("mt-0");
    parent.siblings(".datatable-easyui").removeClass("d-none");
    parent.siblings(".form-group").removeClass("d-none");
}
AEDG.prototype.loadDatagrid = function (id) {
    let dg = $(`#${id} .modal-datagrid`);
    dg.datagrid("load", getQueryParams(`#${id} form`));
}
AEDG.prototype.getCheckbox = function (dg, index) {
    return dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].querySelector("input[type=radio]");
}
AEDG.prototype.getChecked = function (dg, index) {
    let dom = this.getCheckbox(dg, index);
    return dom ? dom.checked : null;
}
AEDG.prototype.filterCheck = function (dg, data, key) {
    if (key !== '') {
        data.rows.forEach((row, index) => {
            switch (row[key]) {
                case '9':
                case '10':
                case '11':
                    dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].innerHTML = ""
                    break;
            }
            dg.datagrid('getPanel').find('.datagrid-header-check')[0].innerHTML = ""
        })
    }
}
AEDG.prototype.addRowEvent = function () {
    if (this.mdg.datagrid("getChecked").length !== 0) {

        this.addRowBtn.off("click");

        this.addRowBtn.on("click", () => {
            if (this.mdg.datagrid("getChecked").length !== 0) {
                this.changeEditAreaCss();
                this.appendData();
            }
        });

        if (this.edg.closest(".datatable-easyui").hasClass('d-none')) {
            this.changeEditAreaCss();
            this.initResultDatagrid(this.edg);
        }
        this.addRowBtn.click();
    }
}
AEDG.prototype.addSpinner = function (btn) {
    $(btn).append(` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);
}
AEDG.prototype.removeSpinner = function (btn) {
    $(btn).children(".spinner-border").remove();
}
AEDG.prototype.addButtonEvent = function (dg) {
    let self = this;
    $(dg.datagrid("getPanel")).on("click", "button[data-btn-type]", function () {
        let btn = $(this);
        let index = +(btn.attr("data-index"));
        let type = btn.attr("data-btn-type");
        let row = dg.datagrid('getRows')[index];
        self.event[type](row, index);
    })
}