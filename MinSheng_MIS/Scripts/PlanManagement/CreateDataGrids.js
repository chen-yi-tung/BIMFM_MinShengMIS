/*每日巡檢時程模板*/
const ALDGOptions = {
    mdg: '#AutoLink .modal-datagrid',
    edg: '#Template-datagrid',
    id: "AutoLink",
    type: "Template",
    initDatagridUrl: "/Datagrid/PlanManagement",
    appendDataUrl: "/PlanManagement/AddReportForm",
    appendDataKey: "RSN",
    removeDataUrl: "/PlanManagement/DeleteReportForm",
    removeDataKey: "RSN",
    filterCheckKey: "ReportStatenum",
    datagridOptions: {
        idField: 'RSN',
        remoteSort: false,
        sortOrder: 'asc',
        singleSelect: true,
        selectOnCheck: false,
        checkOnSelect: false,
    },
    columns: [[
        { field: 'Name', title: '每日巡檢時程模板', align: 'center', width: 400, sortable: true },
        {
            field: '_info', align: 'center', width: 150, formatter: (val, row, index) => {
                return `<button class="btn btn-datagrid" data-index="${index}" data-btn-type="read"
                            onclick="window.open('PlanManagement/Management/${ row.IPSN }', "_blank")"
                        >巡檢排程資訊</button>`;
            }
        },
    ]],
    frozenColumns: [[
        /*{ field: '_select', checkbox: true, },*/
        {
            field: 'action', align: 'center', width: 40, formatter: function (value, row, index) {
                return `<input type="radio" name="dgRadio">`;
            },
        },
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
        detail: (row, index) => { window.open(`/PlanManagement/Read/${row.RSN}`, "_blank"); },
    }
}

function IPDG(options) {
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
                    { Name: "前處理機房巡檢" },
                    { Name: "生物處理機台巡檢" },
                    { Name: "2" },
                    { Name: "3" },
                    { Name: "4" },
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

IPDG.prototype.changeEditAreaCss = function (reserve = false) {
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
IPDG.prototype.loadDatagrid = function (id) {
    let dg = $(`#${id} .modal-datagrid`);
    dg.datagrid("load", getQueryParams(`#${id} form`));
}
IPDG.prototype.getCheckbox = function (dg, index) {
    return dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].querySelector("input[type=radio]");
}
IPDG.prototype.getChecked = function (dg, index) {
    let dom = this.getCheckbox(dg, index);
    return dom ? dom.checked : null;
}
IPDG.prototype.filterCheck = function (dg, data, key) {
    data.rows.forEach((row, index) => {
        switch (row[key]) {
            case '9':
            case '10':
            case '11':
                dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].innerHTML = ""
                break;
        }
    })
    dg.datagrid('getPanel').find('.datagrid-header-check')[0].innerHTML = ""
}
IPDG.prototype.addRowEvent = function () {
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
IPDG.prototype.addSpinner = function (btn) {
    $(btn).append(` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);
}
IPDG.prototype.removeSpinner = function (btn) {
    $(btn).children(".spinner-border").remove();
}
IPDG.prototype.addButtonEvent = function (dg) {
    let self = this;
    $(dg.datagrid("getPanel")).on("click", "button[data-btn-type]", function () {
        let btn = $(this);
        let index = +(btn.attr("data-index"));
        let type = btn.attr("data-btn-type");
        let row = dg.datagrid('getRows')[index];
        self.event[type](row, index);
    })
}