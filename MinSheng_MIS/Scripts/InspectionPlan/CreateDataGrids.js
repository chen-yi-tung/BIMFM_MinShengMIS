/* function addDatagridEvent() {
    const M_MDG = $('#DatagridModal-Maintain .modal-datagrid');
    const E_MDG = $('#Maintain-datagrid');

    const M_RDG = $('#DatagridModal-Repair .modal-datagrid');
    const E_RDG = $('#Repair-datagrid');

    let M_MSC = null;

    let M_RSC = null;

    const options = {
        //rownumbers: true,
        remoteSort: false,
        sortOrder: 'asc',
        singleSelect: false,
        selectOnCheck: true,
        checkOnSelect: true
    };

    const pageOptions = {
        pageSize: 10,
        showPageList: true,
        pageList: [10, 20, 50],
        beforePageText: '第',
        afterPageText: '頁，共 {pages} 頁',
        displayMsg: '顯示 {from} 到 {to} 筆資料，共 {total} 筆資料'
    };

    const MaintainColumns = {
        idField: 'EMFISN',
        //sortName: 'EMFISN',
        columns: [[
            { field: 'FormItemStatenum', hidden: true },
            { field: 'StockState', title: '庫存狀態', align: 'center', width: 120, sortable: true },
            { field: 'FormItemState', title: '保養項目狀態', align: 'center', width: 150, sortable: true },
            { field: 'EState', title: '設備狀態', align: 'center', width: 120, sortable: true },
            { field: 'Area', title: '棟別', align: 'center', width: 130, sortable: true },
            { field: 'Floor', title: '樓層', align: 'center', width: 80, sortable: true },

            { field: 'ESN', title: '設備編號', align: 'center', width: 100, sortable: true },
            { field: 'EName', title: '設備名稱', align: 'center', width: 220, sortable: true },

            { field: 'EMFISN', title: '保養單項目編號', align: 'center', width: 225, sortable: true },

            { field: 'MIName', title: '保養項目', align: 'center', width: 200, sortable: true },
            { field: 'Period', title: '週期', align: 'center', width: 100, sortable: true },
            { field: 'Unit', title: '週期單位', align: 'center', width: 130, sortable: true },

            { field: 'LastTime', title: '上次保養', align: 'center', width: 150, sortable: true },
            { field: 'Date', title: '最近應保養', align: 'center', width: 150, sortable: true },
        ]]
    };

    this.mdg = {
        onDetail: function (index) {
            M_MSC = getChecked(M_MDG, index);
            let row = M_MDG.datagrid('getRows')[index];
            let url = `/MaintainForm_Management/Read/${row.EMFISN}`;
            window.open(url, "_blank");
        },
        onLocate: function (index) {
            M_MSC = getChecked(M_MDG, index);
            let row = M_MDG.datagrid('getRows')[index];
            let url = `/MaintainForm_Management/Read/${row.EMFISN}`;
            window.open(url, "_blank");
        }
    }

    this.emdg = {
        onDetail: function (index) {
            M_MSC = getChecked(E_MDG, index);
            let row = E_MDG.datagrid('getRows')[index];
            let url = `/MaintainForm_Management/Read/${row.EMFISN}`
            window.open(url, "_blank");
        },
        onLocate: function (index) {
            M_MSC = getChecked(E_MDG, index);
            let row = E_MDG.datagrid('getRows')[index];
            let url = `/MaintainForm_Management/Read/${row.EMFISN}`
            window.open(url, "_blank");
        }
    }

    const RepairColumns = {
        idField: 'RSN',
        //sortName: 'RSN',
        columns: [[
            { field: 'ReportStatenum', hidden: true },
            { field: 'StockState', title: '庫存狀態', align: 'center', width: 120, sortable: true },
            { field: 'ReportState', title: '報修單狀態', align: 'center', width: 145, sortable: true },
            { field: 'EState', title: '設備狀態', align: 'center', width: 120, sortable: true },
            { field: 'Area', title: '棟別', align: 'center', width: 130, sortable: true },
            { field: 'Floor', title: '樓層', align: 'center', width: 80, sortable: true },

            { field: 'ESN', title: '設備編號', align: 'center', width: 100, sortable: true },
            { field: 'EName', title: '設備名稱', align: 'center', width: 220, sortable: true },

            { field: 'RSN', title: '報修單號', align: 'center', width: 116, sortable: true },
            { field: 'ReportLevel', title: '報修等級', align: 'center', width: 95, sortable: true },
            { field: 'Date', title: '報修時間', align: 'center', width: 200, sortable: true },
            { field: 'MyName', title: '報修人員', align: 'center', width: 110, sortable: true },
            { field: 'ReportContent', title: '報修內容', align: 'center', width: 300, sortable: true },
        ]]
    }

    this.rdg = {
        onDetail: function (index) {
            M_RSC = getChecked(M_RDG, index);
            let row = M_RDG.datagrid('getRows')[index];
            let url = `/Report_Management/Read/${row.RSN}`
            window.open(url, "_blank");
        },
        onLocate: function (index) {
            M_RSC = getChecked(M_RDG, index);
            let row = M_RDG.datagrid('getRows')[index];
            let url = `/Report_Management/Read/${row.RSN}`
            window.open(url, "_blank");
        }
    }

    this.erdg = {
        onDetail: function (index) {
            M_RSC = getChecked(E_RDG, index);
            let row = E_RDG.datagrid('getRows')[index];
            let url = `/Report_Management/Read/${row.RSN}`
            window.open(url, "_blank");
        },
        onLocate: function (index) {
            M_RSC = getChecked(E_RDG, index);
            let row = E_RDG.datagrid('getRows')[index];
            let url = `/Report_Management/Read/${row.RSN}`
            window.open(url, "_blank");
        }
    }

    $("#Maintain-create").one("click", MaintainCreate)

    $("#Repair-create").one("click", RepairCreate)

    function MaintainCreate() {
        const id = "DatagridModal-Maintain";
        const modal = $("#" + id);
        const dataGrid = $(`#${id} .modal-datagrid`);
        modal.one("shown.bs.modal", () => {
            initDatagrid(dataGrid);
            modal.on("shown.bs.modal", () => { dataGrid.datagrid("load") });
            modal.on("hidden.bs.modal", () => { $(`#${id} form`)[0].reset(); dataGrid.datagrid("clearChecked"); });
        });

        $(`#${id} #search`).click(() => { loadDatagrid(id) });

        $(`#${id} #add-row`).on("click", () => { addRowEvent(M_MDG, E_MDG, "Maintain") });

        function initDatagrid(dg) {
            dg.datagrid(
                Object.assign(
                    {
                        url: "/MaintainForm_Management/MaintainForm_Management",
                        method: 'POST',
                        queryParams: getQueryParams(`#${id} form`),
                        fit: true,
                        pagination: true,
                        pagePosition: 'bottom',
                        pageSize: 10,
                        frozenColumns: [[
                            { field: '_select', checkbox: true, },
                            {
                                field: '_detail', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.mdg.onDetail(${index})">詳情</button>`;
                                }
                            },
                            {
                                field: '_locate', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.mdg.onLocate(${index})">定位</button>`;
                                }
                            }
                        ]],
                        onBeforeSelect: (index) => onBeforeSelect(dg, index, M_MSC, false),
                        onBeforeUnselect: (index) => onBeforeSelect(dg, index, M_MSC, true),
                        onLoadSuccess: (data) => {
                            data.rows.forEach((row, index, arr) => {
                                switch (row.FormItemStatenum) {
                                    case '9':
                                    case '10':
                                    case '11':
                                        dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].innerHTML = ""
                                        break;
                                }
                            })
                            dg.datagrid('getPanel').find('.datagrid-header-check')[0].innerHTML = ""
                        }
                    },
                    options,
                    MaintainColumns
                )
            )
            $(dg.datagrid('getPager')).pagination(pageOptions);
        }
        function initResultDatagrid(dg) {
            dg.datagrid(
                Object.assign(
                    {
                        frozenColumns: [[
                            { field: '_select', checkbox: true, },
                            {
                                field: '_detail', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.emdg.onDetail(${index})">詳情</button>`;
                                }
                            },
                            {
                                field: '_locate', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.emdg.onLocate(${index})">定位</button>`;
                                }
                            }
                        ]],
                        onBeforeSelect: (index) => onSelectBtn(dg, index, M_MSC),
                        onBeforeUnselect: (index) => onSelectBtn(dg, index, M_MSC),
                    },
                    options,
                    MaintainColumns
                )
            )
            window.addEventListener("resize", (event) => {
                dg.datagrid('resize');
            });
        }
        function addRowEvent(mdg, edg, type) {
            if (mdg.datagrid("getChecked").length !== 0) {
                let addRowBtn = $(`#${id} #add-row`);
                let createBtn = $(`#${type}-create`);
                let deleteBtn = $(`#${type}-delete`);

                addRowBtn.off("click");

                addRowBtn.on("click", function () {
                    if (mdg.datagrid("getChecked").length !== 0) {
                        changeEditAreaCss.call(createBtn[0]);
                        appendData.call(this);
                    }
                });

                deleteBtn.on("click", function () {
                    if (edg.datagrid("getChecked").length !== 0) {
                        removeData.call(this);
                    }
                });

                changeEditAreaCss.call(createBtn[0]);
                initResultDatagrid(edg);
                addRowBtn.click();
            }
        }
        function appendData() {
            let data = M_MDG.datagrid("getChecked");
            console.log("appendData POST", data);
            let btn = $(this);
            btn.append(` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);

            getDeviceData(data.map(d => d.ESN));

            $.ajax({
                url: "/InspectionPlan_Management/AddMaintainForm",
                data: JSON.stringify(data.map(d => d.EMFISN)),
                type: "POST",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: onSuccess,
                error: onError
            })

            function onSuccess(res) {
                console.log("appendData onSuccess", res)
                btn.children(".spinner-border").remove();
                bootstrap.Modal.getInstance(modal[0]).hide();

                res.rows.forEach((d) => { E_MDG.datagrid("appendRow", d) })

                reloadDeviceData();
            }
            function onError(err) {
                btn.children(".spinner-border").remove();
                createDialogModal({
                    id: "DialogModal-Error",
                    inner: "新增失敗！",
                    button: [{ className: "btn btn-delete", cancel: true, text: "確定" }]
                })
            }
        }
        function removeData() {
            let data = E_MDG.datagrid("getChecked").map((d) => { return d.EMFISN });
            console.log(data);
            let btn = $(this);
            btn.append(` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);

            $.ajax({
                url: "/InspectionPlan_Management/DeleteMaintainForm",
                data: JSON.stringify(data),
                type: "POST",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: onSuccess,
                error: onError
            })

            function onSuccess(res) {
                console.log(res);
                btn.children(".spinner-border").remove();
                bootstrap.Modal.getInstance(modal[0]).hide();

                createDialogModal({
                    id: "DialogModal-Success",
                    inner: "刪除成功！",
                })

                res.forEach((d) => { E_MDG.datagrid("deleteRow", E_MDG.datagrid("getRowIndex", d.EMFISN)) })

                reloadDeviceData();

                E_MDG.datagrid("clearChecked");

                if (E_MDG.datagrid("getRows").length === 0) {
                    changeEditAreaCss.call($("#Maintain-create")[0], true)
                }
            }
            function onError(err) {
                btn.children(".spinner-border").remove();
                createDialogModal({
                    id: "DialogModal-Error",
                    inner: "刪除失敗！",
                })
                E_MDG.datagrid("clearChecked");
            }
        }
    }
    function RepairCreate() {
        const id = "DatagridModal-Repair";
        const modal = $("#" + id);
        const dataGrid = $(`#${id} .modal-datagrid`);
        modal.one("shown.bs.modal", () => {
            initDatagrid(dataGrid);
            modal.on("shown.bs.modal", () => { dataGrid.datagrid("load") });
            modal.on("hidden.bs.modal", () => { $(`#${id} form`)[0].reset(); dataGrid.datagrid("clearChecked"); });
        });
        $(`#${id} #search`).click(() => { loadDatagrid(id) });

        $(`#${id} #add-row`).on("click", () => { addRowEvent(M_RDG, E_RDG, "Repair") });

        function initDatagrid(dg) {
            dg.datagrid(
                Object.assign(
                    {
                        url: '/Datagrid/Report_Management',
                        method: 'POST',
                        queryParams: getQueryParams(`#${id} form`),
                        fit: true,
                        pagination: true,
                        pagePosition: 'bottom',
                        pageSize: 10,
                        frozenColumns: [[
                            { field: '_select', checkbox: true },
                            {
                                field: '_detail', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.rdg.onDetail(${index})">詳情</button>`;
                                }
                            },
                            {
                                field: '_locate', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.rdg.onLocate(${index})">定位</button>`;
                                }
                            }
                        ]],
                        onBeforeSelect: (index) => onBeforeSelect(dg, index, M_RSC, false),
                        onBeforeUnselect: (index) => onBeforeSelect(dg, index, M_RSC, true),
                        onLoadSuccess: (data) => {
                            data.rows.forEach((row, index, arr) => {
                                switch (row.ReportStatenum) {
                                    case '9':
                                    case '10':
                                    case '11':
                                        dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].innerHTML = ""
                                        break;
                                }
                            })
                            dg.datagrid('getPanel').find('.datagrid-header-check')[0].innerHTML = ""
                        }
                    },
                    options,
                    RepairColumns
                )
            )
            $(dg.datagrid('getPager')).pagination(pageOptions);
        }
        function initResultDatagrid(dg) {
            dg.datagrid(
                Object.assign(
                    {
                        frozenColumns: [[
                            { field: '_select', checkbox: true },
                            {
                                field: '_detail', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.erdg.onDetail(${index})">詳情</button>`;
                                }
                            },
                            {
                                field: '_locate', align: 'center', width: 71,
                                formatter: (val, row, index) => {
                                    return `<button class="btn btn-datagrid" onclick="DatagridEvent.erdg.onLocate(${index})">定位</button>`;
                                }
                            }
                        ]],
                        onBeforeSelect: (index) => onSelectBtn(dg, index, M_RSC),
                        onBeforeUnselect: (index) => onSelectBtn(dg, index, M_RSC),
                    },
                    options,
                    RepairColumns
                )
            )
            window.addEventListener("resize", (event) => {
                dg.datagrid('resize');
            });
        }
        function addRowEvent(mdg, edg, type) {
            if (mdg.datagrid("getChecked").length !== 0) {
                let addRowBtn = $(`#${id} #add-row`);
                let createBtn = $(`#${type}-create`);
                let deleteBtn = $(`#${type}-delete`);

                addRowBtn.off("click");

                addRowBtn.on("click", function () {
                    if (mdg.datagrid("getChecked").length !== 0) {
                        changeEditAreaCss.call(createBtn[0]);
                        appendData.call(this);
                    }
                });

                deleteBtn.on("click", function () {
                    if (edg.datagrid("getChecked").length !== 0) {
                        removeData.call(this);
                    }
                });

                changeEditAreaCss.call(createBtn[0]);
                initResultDatagrid(edg);
                addRowBtn.click();
            }
        }
        function appendData() {
            let data = M_RDG.datagrid("getChecked");
            console.log("appendData POST", data);
            let btn = $(this);
            btn.append(` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);

            getDeviceData(data.map(d => d.ESN));

            $.ajax({
                url: "/InspectionPlan_Management/AddReportForm",
                data: JSON.stringify(data.map(d => d.RSN)),
                type: "POST",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: onSuccess,
                error: onError
            })

            function onSuccess(res) {
                console.log("appendData onSuccess", res)
                btn.children(".spinner-border").remove();
                bootstrap.Modal.getInstance(modal[0]).hide();

                res.rows.forEach((d) => { E_RDG.datagrid("appendRow", d) })

                reloadDeviceData();
            }
            function onError(err) {
                btn.children(".spinner-border").remove();
                createDialogModal({
                    id: "DialogModal-Error",
                    inner: "新增失敗！",
                })
            }
        }
        function removeData() {
            let data = E_RDG.datagrid("getChecked").map((d) => { return d.RSN });
            console.log(data);
            let btn = $(this);
            btn.append(` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);

            $.ajax({
                url: "/InspectionPlan_Management/DeleteReportForm",
                data: JSON.stringify(data),
                type: "POST",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: onSuccess,
                error: onError
            })

            function onSuccess(res) {
                console.log(res);
                btn.children(".spinner-border").remove();
                bootstrap.Modal.getInstance(modal[0]).hide();

                createDialogModal({
                    id: "DialogModal-Success",
                    inner: "刪除成功！",
                })

                res.forEach((d) => { E_RDG.datagrid("deleteRow", E_RDG.datagrid("getRowIndex", d.RSN)) })

                reloadDeviceData();

                E_RDG.datagrid("clearChecked");

                if (E_RDG.datagrid("getRows").length === 0) {
                    changeEditAreaCss.call($("#Repair-create")[0], true)
                }
            }
            function onError(err) {
                btn.children(".spinner-border").remove();
                createDialogModal({
                    id: "DialogModal-Error",
                    inner: "刪除失敗！",
                })
                E_RDG.datagrid("clearChecked");
            }
        }
    }

    function changeEditAreaCss(reserve = false) {

        let selfJQ = $(this);
        let parent = selfJQ.parent();

        if (reserve) {
            selfJQ.siblings().addClass("d-none");
            parent.addClass("mt-0");
            parent.siblings(".datatable-easyui").addClass("d-none");
            parent.siblings(".form-group").addClass("d-none");
            return;
        }

        selfJQ.siblings().removeClass("d-none");
        parent.removeClass("mt-0");
        parent.siblings(".datatable-easyui").removeClass("d-none");
        parent.siblings(".form-group").removeClass("d-none");
    }
    function loadDatagrid(id) {
        let dg = $(`#${id} .modal-datagrid`);
        dg.datagrid("load", getQueryParams(`#${id} form`));
    }
    function getCheckbox(dg, index) {
        return dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].querySelector(".datagrid-cell-check");
    }
    function getChecked(dg, index) {
        let dom = getCheckbox(dg, index);
        return dom ? dom.checked : null;
    }
    function onSelectBtn(dg, index, sc) {
        if (sc !== null && sc == getChecked(dg, index)) {
            sc = null;
            return false;
        }
        return true;
    }
    function onBeforeSelect(dg, index, sc, bool) {
        if (getCheckbox(dg, index) == null) return bool;
        return onSelectBtn(dg, index, sc);
    }
    return this;
} */

const MDGOptions = {
    mdg: '#DatagridModal-Maintain .modal-datagrid',
    edg: '#Maintain-datagrid',
    id: "DatagridModal-Maintain",
    type: "Maintain",
    initDatagridUrl: "/MaintainForm_Management/MaintainForm_Management",
    appendDataUrl: "/InspectionPlan_Management/AddMaintainForm",
    appendDataKey: "EMFISN",
    removeDataUrl: "/InspectionPlan_Management/DeleteMaintainForm",
    removeDataKey: "EMFISN",
    filterCheckKey: "FormItemStatenum",
    datagridOptions: {
        idField: 'EMFISN',
        remoteSort: false,
        sortOrder: 'asc',
        singleSelect: true,
        selectOnCheck: false,
        checkOnSelect: false
    },
    columns: [[
        { field: 'FormItemStatenum', hidden: true },
        { field: 'StockState', title: '庫存狀態', align: 'center', width: 120, sortable: true },
        { field: 'FormItemState', title: '保養項目狀態', align: 'center', width: 150, sortable: true },
        { field: 'EState', title: '設備狀態', align: 'center', width: 120, sortable: true },
        { field: 'Area', title: '棟別', align: 'center', width: 130, sortable: true },
        { field: 'Floor', title: '樓層', align: 'center', width: 80, sortable: true },

        { field: 'ESN', title: '設備編號', align: 'center', width: 100, sortable: true },
        { field: 'EName', title: '設備名稱', align: 'center', width: 220, sortable: true },

        { field: 'EMFISN', title: '保養單項目編號', align: 'center', width: 225, sortable: true },

        { field: 'MIName', title: '保養項目', align: 'center', width: 200, sortable: true },
        { field: 'Period', title: '週期', align: 'center', width: 100, sortable: true },
        { field: 'Unit', title: '週期單位', align: 'center', width: 130, sortable: true },

        { field: 'LastTime', title: '上次保養', align: 'center', width: 150, sortable: true },
        { field: 'Date', title: '最近應保養', align: 'center', width: 150, sortable: true },
    ]],
    frozenColumns: [[
        { field: '_select', checkbox: true, },
        {
            field: '_detail', align: 'center', width: 71, formatter: (val, row, index) => {
                return `<button class="btn btn-datagrid" data-index="${index}" data-btn-type="detail">詳情</button>`;
            }
        },
        {
            field: '_locate', align: 'center', width: 71,
            formatter: (val, row, index) => {
                let disabled = row.DBID == null || row.DBID == "" ? 'disabled' : '';
                return `<button class="btn btn-datagrid" data-index="${index}" data-btn-type="locate" ${disabled}>定位</button>`;
            }
        }
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
        detail: (row, index) => { window.open(`/MaintainForm_Management/Read/${row.EMFISN}`, "_blank"); },
        locate: (row, index) => { window.open(`/MaintainForm_Management/Read/${row.EMFISN}`, "_blank"); },
    }
}

const RDGOptions = {
    mdg: '#DatagridModal-Repair .modal-datagrid',
    edg: '#Repair-datagrid',
    id: "DatagridModal-Repair",
    type: "Repair",
    initDatagridUrl: "/Datagrid/Report_Management",
    appendDataUrl: "/InspectionPlan_Management/AddReportForm",
    appendDataKey: "RSN",
    removeDataUrl: "/InspectionPlan_Management/DeleteReportForm",
    removeDataKey: "RSN",
    filterCheckKey: "ReportStatenum",
    datagridOptions: {
        idField: 'RSN',
        remoteSort: false,
        sortOrder: 'asc',
        singleSelect: true,
        selectOnCheck: false,
        checkOnSelect: false
    },
    columns: [[
        { field: 'ReportStatenum', hidden: true },
        { field: 'StockState', title: '庫存狀態', align: 'center', width: 120, sortable: true },
        { field: 'ReportState', title: '報修單狀態', align: 'center', width: 145, sortable: true },
        { field: 'EState', title: '設備狀態', align: 'center', width: 120, sortable: true },
        { field: 'Area', title: '棟別', align: 'center', width: 130, sortable: true },
        { field: 'Floor', title: '樓層', align: 'center', width: 80, sortable: true },

        { field: 'ESN', title: '設備編號', align: 'center', width: 100, sortable: true },
        { field: 'EName', title: '設備名稱', align: 'center', width: 220, sortable: true },

        { field: 'RSN', title: '報修單號', align: 'center', width: 116, sortable: true },
        { field: 'ReportLevel', title: '報修等級', align: 'center', width: 95, sortable: true },
        { field: 'Date', title: '報修時間', align: 'center', width: 200, sortable: true },
        { field: 'MyName', title: '報修人員', align: 'center', width: 110, sortable: true },
        { field: 'ReportContent', title: '報修內容', align: 'center', width: 300, sortable: true },
    ]],
    frozenColumns: [[
        { field: '_select', checkbox: true, },
        {
            field: '_detail', align: 'center', width: 71, formatter: (val, row, index) => {
                return `<button class="btn btn-datagrid" data-index="${index}" data-btn-type="detail">詳情</button>`;
            }
        },
        {
            field: '_locate', align: 'center', width: 71,
            formatter: (val, row, index) => {
                let disabled = row.DBID == null || row.DBID == "" ? 'disabled' : '';
                return `<button class="btn btn-datagrid" data-index="${index}" data-btn-type="locate" ${disabled}>定位</button>`;
            }
        }
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
        detail: (row, index) => { window.open(`/Report_Management/Read/${row.RSN}`, "_blank"); },
        locate: (row, index) => { window.open(`/Report_Management/Read/${row.RSN}`, "_blank"); },
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
                url: self.options.initDatagridUrl,
                method: 'POST',
                queryParams: getQueryParams(`#${self.options.id} form`),
                fit: true,
                pagination: true,
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
        window.addEventListener("resize", (event) => { dg.datagrid('resize'); });
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
        parent.siblings(".datatable-easyui").addClass("d-none");
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
    return dg.datagrid('getPanel').find('.datagrid-row [field="_select"]')[index].querySelector("input[type=checkbox]");
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