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
    $("#submit").click(function () {
        console.log("onclick submit");
        save(getCreateSaveData());
    })
    $("#addItem").click(function () {
        console.log("onclick addItem")
        sampleTr.create();
    })
}

function getCreateSaveData() {
    let TemplateName = $("#TemplateName").val();
    if (!TemplateName) {
        dialogError("請輸入巡檢模板名稱！")
        return;
    }
    if (!sampleTr.checkRequired()) {
        dialogError("請至少新增一項巡檢設備！")
        return;
    }
    if (!sampleTr.checkValidity()) {
        dialogError("請檢查是否完成模板內容必填！")
        return;
    }
    //console.log(data);
    return {
        TemplateName,
        TempalteItems: sampleTr.calc()
    };
    function dialogError(inner) {
        createDialogModal({ id: "DialogModal-Error", inner: inner })
    }
}