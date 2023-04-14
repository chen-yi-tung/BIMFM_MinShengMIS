let fakeData;

let DatagridEvent;

let maintainDG_Controller, repairDG_Controller;

async function addDropDownList() {
    await pushSelect("Shift", "/DropDownList/Shift");
    await pushSelect("MaintainUserID", "/DropDownList/AllMyName");
    await pushSelect("RepairUserID", "/DropDownList/AllMyName");

    let DMM = 'DatagridModal-Maintain';
    await pushSelect(DMM + " #FormItemState", "/DropDownList/AddFormItemState");
    await pushSelect(DMM + " #EState", "/DropDownList/EState?url=AddToPlan");
    await pushSelect(DMM + " #StockState", "/DropDownList/StockState");

    let DMR = 'DatagridModal-Repair';
    await pushSelect(DMR + " #ReportLevel", "/DropDownList/ReportLevel");
    await pushSelect(DMR + " #ReportState", '/DropDownList/Report_Management_Management_ReportFormState?url=CanAddToPlanReportState');
    await pushSelect(DMR + " #UserID", "/DropDownList/ReportUser");
    await pushSelect(DMR + " #StockState", "/DropDownList/StockState");

    await $('#UserID').tagbox({
        url: "/DropDownList/AllMyName",
        textField: 'Text',
        valueField: 'Value',
        hasDownArrow: true,
        limitToList: true,
    });
}

async function addAreaFloorEvent() {
    let areaList = ["path-form", "sample-path-form", "DatagridModal-Maintain", "DatagridModal-Repair"];
    areaList.forEach((id) => {
        pushSelect(`${id} #ASN`, "/DropDownList/Area");
        let ASN = $(`#${id} #ASN`);

        ASN.on("change", async function () {

            if (ASN.val()) {
                $(`#${id} #Area`).val(ASN.children("option:selected").text());
            }
            else {
                $(`#${id} #Area`).val('');
            }

            await pushSelect(`${id} #FSN`, "/DropDownList/Floor" + `?ASN=${ASN.val()}`);
            $(`#${id} #FSN`).val('');
        });
    })

    let floorList = ["path-form", "sample-path-form", "DatagridModal-Maintain", "DatagridModal-Repair"];
    floorList.forEach((id) => {
        let FSN = $(`#${id} #FSN`);

        FSN.on("change", async function () {

            if (FSN.val()) {
                $(`#${id} #Floor`).val(FSN.children("option:selected").text());
            }
            else {
                $(`#${id} #Floor`).val('');
            }

            if (id == "sample-path-form") {
                await pushSelect(`${id} #PathTitle`, "/DropDownList/PathTitle" + `?FSN=${FSN.val()}`);
                $(`#${id} #PathTitle`).val('');
            }
        });
    })
}

function addButtonEvent() {
    $("#back").click(function () {
        history.back();
    })
    $("#path-create").click(function (e) {
        e.preventDefault();
        console.log("onclick path-create");
        createPath("#path-form")
    })
    $("#sample-path-create").click(function (e) {
        e.preventDefault();
        console.log("onclick sample-path-create");
        createPath("#sample-path-form")
    })
    $("#path-save").click(function () {
        saveCurrentPath(() => { createDialogModal({ id: "DialogModal", inner: "儲存成功！" }) })
    })
    $("#submit").click(function () {
        console.log("onclick submit")
        checkNeedSaveCurrentPath(save);
    })
}

function getCreateSaveData() {
    let MaintainEquipment = $("#Maintain-datagrid").parent().hasClass("d-none") ? null : $("#Maintain-datagrid").datagrid('getRows').map(e => e.EMFISN);
    let MaintainUserID = $("#MaintainUserID").val();

    if (MaintainEquipment !== null && MaintainUserID === '') {
        dialogError("未選擇定期保養審核人員！")
        return;
    }

    let RepairEquipment = $("#Repair-datagrid").parent().hasClass("d-none") ? null : $("#Repair-datagrid").datagrid('getRows').map(e => e.RSN);
    let RepairUserID = $("#RepairUserID").val();

    if (RepairEquipment !== null && RepairUserID === '') {
        dialogError("未選擇維修審核人員！")
        return;
    }

    let InspectionPlanPaths = getPathDatas()
    if (InspectionPlanPaths.length === 0) {
        dialogError("至少需要一條巡檢路線！")
        return;
    }

    for (const e of InspectionPlanPaths) {
        if (e.error && e.error === "order") {
            dialogError(`計畫中設備未排入巡檢路線 ${e.pathTitle} 中！`)
            return;
        }
    }

    if (!checkAllAreaFloor(InspectionPlanPaths)) {
        dialogError(`巡檢路線中無計畫中設備的棟別樓層！`)
        return;
    }

    let data = {
        IPName: $("#IPName").val(),
        PlanCreateUserID: $("#NavbarUserID").val(),
        PlanDate: $("#PlanDate").val(),
        Shift: $("#Shift").val(),
        UserID: $("#UserID").tagbox('getValues'),
        MaintainUserID: MaintainUserID,
        RepairUserID: RepairUserID,
        MaintainEquipment: MaintainEquipment,
        RepairEquipment: RepairEquipment,
        InspectionPlanPaths: InspectionPlanPaths
    }

    console.log(data);

    //return data;

    function getPathDatas() {
        return $(".sample-path-group[data-path-id]").toArray().map((e) => {
            let pathID = $(e).attr('data-path-id');
            let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`));
            if (!checkOrder(pathData)) {
                return {
                    error: "order",
                    pathID: pathID,
                    pathTitle: pathData.PathSample.PathTitle
                }
            }
            return pathData;
        })
    }
    function checkAllAreaFloor(data) {
        for (const e of getEquip()) {
            for (const d of data) {
                if (d.error) {
                    return false;
                }
                if (e.ASN !== d.PathSample.ASN || e.FSN !== d.PathSample.FSN) {
                    return false;
                }
            }
        }

        return true;

        function getEquip() {
            return [...getDgRows(maintainDG_Controller), ...getDgRows(repairDG_Controller)]
        }
    }
    function checkOrder(data) {
        let equip = getEquipData();
        let beacon = data.PathSample.Beacon.map(e => e.deviceName);
        let order = data.PathSampleOrder.filter(e => !beacon.includes(e));
        let result;

        if (equip.length > order.length) {
            result = check(order, equip);
        } else if (equip.length <= order.length) {
            result = check(equip, order);
        }

        console.log("checkOrder:", {
            equip: equip,
            order: order,
            result: result
        });

        return result.every((e) => e);

        function getEquipData() {
            return [...new Set([...getESNs(maintainDG_Controller), ...getESNs(repairDG_Controller)])];
        }
        function getESNs(dgc) {
            return getDgRows(dgc).filter(e => {
                return e.ASN === data.PathSample.ASN && e.FSN === data.PathSample.FSN
            }).map(e => e.ESN)
        }
        function check(a, b) {
            return b.map(e => a.includes(e));
        }
    }
    function getDgRows(dgc) {
        return dgc.edg.datagrid('getRows');
    }

    function dialogError(inner) {
        createDialogModal({ id: "DialogModal-Error", inner: inner })
    }
}