let fakeData;

let DatagridEvent;

let autoLinkDG_Controller

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
        dialogError("請至少新增一條巡檢路線！")
        return;
    }
    if (!sampleTr.checkValidity()) {
        dialogError("請檢查是否完成模板內容必填！")
        return;
    }
    return {
        TemplateName,
        Contents: sampleTr.calc()
    };
    function dialogError(inner) {
        createDialogModal({ id: "DialogModal-Error", inner: inner })
    }
}