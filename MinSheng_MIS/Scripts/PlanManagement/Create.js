let fakeData;

let DatagridEvent;

let autoLinkDG_Controller

async function addDropDownList(selector) {
    console.log("selector", selector);
    await $(selector).tagbox({
        url: "/DropDownList/AssignmentUserName",
        textField: 'Text',
        valueField: 'Value',
        hasDownArrow: true,
        limitToList: true,
        validateOnCreate: false,
        tagStyler: function (value) {
            if (value) {
                return 'background:#5480CA; color:#FFF; padding: 4px; height: fit-content;';
            }
        }
    });
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
        save(getCreateSaveData());
    })
    $("#addItem").click(function () {
        console.log("onclick addItem")
        sampleTr.create();
    })
}

function getCreateSaveData() {
    let IPName = $("#IPName").val();
    let PlanDate = $("#PlanDate").val();
    if (!IPName) {
        dialogError("請輸入工單名稱！")
        return;
    }
    if (!PlanDate) {
        dialogError("請輸入工單日期名稱！")
        return;
    }
    if (!sampleTr.checkRequired()) {
        dialogError("請至少新增一項巡檢路線！")
        return;
    }
    if (!sampleTr.checkValidity()) {
        dialogError("請檢查是否完成模板內容必填！")
        return;
    }
    return {
        IPName,
        PlanDate,
        Inspections: sampleTr.calc()
    }
    function dialogError(inner) {
        createDialogModal({ id: "DialogModal-Error", inner: inner })
    }
}