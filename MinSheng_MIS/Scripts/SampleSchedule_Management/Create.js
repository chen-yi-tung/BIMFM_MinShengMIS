let fakeData;

let DatagridEvent;

let autoLinkDG_Controller

async function addDropDownList() {
    //await pushSelect("Shift", "/DropDownList/Shift");
    //await pushSelect("MaintainUserID", "/DropDownList/AllMyName");
    //await pushSelect("RepairUserID", "/DropDownList/AllMyName");

    //const DMM = "DatagridModal-Maintain";
    //await pushSelect(DMM + " #FormItemState", "/DropDownList/AddFormItemState");
    //await pushSelect(DMM + " #EState", "/DropDownList/EState?url=AddToPlan");
    //await pushSelect(DMM + " #StockState", "/DropDownList/StockState");

    //const DMR = "DatagridModal-Repair";
    //await pushSelect(DMR + " #ReportLevel", "/DropDownList/ReportLevel");
    //await pushSelect(DMR + " #ReportState", "/DropDownList/Report_Management_Management_ReportFormState?url=CanAddToPlanReportState");
    //await pushSelect(DMR + " #UserID", "/DropDownList/ReportUser");
    //await pushSelect(DMR + " #StockState", "/DropDownList/StockState");

    //await $('#UserID').tagbox({
    //    url: "/DropDownList/AllMyName",
    //    textField: 'Text',
    //    valueField: 'Value',
    //    hasDownArrow: true,
    //    limitToList: true,
    //    validateOnCreate: false
    //});
}

async function addAreaFloorEvent() {
//    let areaList = ["path-form", "sample-path-form", "DatagridModal-Maintain", "DatagridModal-Repair"];
//    areaList.forEach((id) => {
//        pushSelect(`${id} #ASN`, "/DropDownList/Area");
//        let ASN = $(`#${id} #ASN`);

//        ASN.on("change", async function () {

//            if (ASN.val()) {
//                $(`#${id} #Area`).val(ASN.children("option:selected").text());
//            }
//            else {
//                $(`#${id} #Area`).val('');
//            }

//            await pushSelect(`${id} #FSN`, "/DropDownList/Floor" + `?ASN=${ASN.val()}`);
//            $(`#${id} #FSN`).val('');
//        });
//    })

//    let floorList = ["path-form", "sample-path-form", "DatagridModal-Maintain", "DatagridModal-Repair"];
//    floorList.forEach((id) => {
//        let FSN = $(`#${id} #FSN`);

//        FSN.on("change", async function () {

//            if (FSN.val()) {
//                $(`#${id} #Floor`).val(FSN.children("option:selected").text());
//            }
//            else {
//                $(`#${id} #Floor`).val('');
//            }

//            if (id == "sample-path-form") {
//                await pushSelect(`${id} #PathTitle`, "/DropDownList/PathTitle" + `?FSN=${FSN.val()}`);
//                $(`#${id} #PathTitle`).val('');
//            }
//        });
//    })
}

function addButtonEvent() {
    $("#back").click(function () {
        history.back();
    })
    $("#path-create").click(function (e) {
        //e.preventDefault();
        console.log("onclick path-create");
        createPath("#path-form")
    })
    $("#sample-path-create").click(function (e) {
        //e.preventDefault();
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

function getCreateSaveData(IPSN = undefined) {

    let IPName = $("#IPName").val(),
        PlanCreateUserID = $("#NavbarUserID").val(),
        PlanDate = $("#PlanDate").val(),

    if (!IPName) {
        dialogError("請輸入工單名稱！")
        return;
    }
    if (!PlanDate) {
        dialogError("請輸入工單日期！")
        return;
    }

    let MaintainEquipment = $("#Maintain-datagrid").parent().hasClass("d-none") ? null : $("#Maintain-datagrid").datagrid('getRows').map(e => e.EMFISN);
    let MaintainUserID = $("#MaintainUserID").val();

    if (MaintainEquipment !== null && MaintainUserID === '') {
        dialogError("未選擇定期保養審核人員！")
        return;
    }

    let data = {
        IPName: IPName,
        PlanCreateUserID: PlanCreateUserID,
        PlanDate: PlanDate,
        Shift: Shift,
        UserID: UserID,
        MaintainUserID: MaintainUserID,
        RepairUserID: RepairUserID,
        MaintainEquipment: MaintainEquipment,
        RepairEquipment: RepairEquipment,
        InspectionPlanPaths: InspectionPlanPaths
    }

    if (IPSN !== '' || IPSN !== undefined || IPSN !== null) {
        data.IPSN = IPSN
    }

    console.log(data);

    return data;

    function findUnusedEquip(data) {
        let equip = getEquip();
        let result = equip.filter((e, i, arr) => {
            return data.findIndex(d => {
                if (d.error) return false
                return e.ASN == d.PathSample.ASN && e.FSN === d.PathSample.FSN
            }) === -1
        })
        console.log("findUnusedEquip result:", result)

        return result;

        function getEquip() {
            return [...getDgRows(autoLinkDG_Controller), ...getDgRows(templateDG_Controller)]
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

        return {
            equip: equip,
            order: order,
            result: result
        };

        function getEquipData() {
            return [...new Set([...getESNs(autoLinkDG_Controller), ...getESNs(templateDG_Controller)])];
        }
        function getESNs(dgc) {
            return getDgRows(dgc).filter(e => {
                return e.ASN == data.PathSample.ASN && e.FSN === data.PathSample.FSN
            }).map(e => e.ESN)
        }
        function check(a, b) {
            return b.map(e => a.includes(e));
        }
    }
    function getDgRows(dgc) {
        if (dgc.edg.parent().hasClass('d-none')) {
            return [];
        }
        return dgc.edg.datagrid('getRows');
    }
    function dialogError(inner) {
        createDialogModal({ id: "DialogModal-Error", inner: inner })
    }
}