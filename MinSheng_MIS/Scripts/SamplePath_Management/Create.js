let fakeData;

let DatagridEvent;

let autoLinkDG_Controller

async function addDropDownList() {
}

async function addAreaFloorEvent() {
    let areaList = ["ModalForm"];
    areaList.forEach((id) => {
        pushSelect(`${id} #RFIDArea`, "/DropDownList/Area");
        let ASN = $(`#${id} #RFIDArea`);

        ASN.on("change", async function () {
            await pushSelect(`${id} #RFIDFloor`, "/DropDownList/Floor" + `?ASN=${ASN.val()}`);
            $(`#${id} #RFIDFloor`).val('');
        });
    })

    //let floorList = ["ModalForm"];
    //floorList.forEach((id) => {
    //    let FSN = $(`#${id} #Floor`);

    //    FSN.on("change", async function () {

    //        if (FSN.val()) {
    //            $(`#${id} #Floor`).val(FSN.children("option:selected").text());
    //        }
    //        else {
    //            $(`#${id} #Floor`).val('');
    //        }
    //    });
    //})
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
        console.log("onclick submit");
        let data = getCreateSaveData(sampleTr, PlanPathSN);
        if (data) save(data);
    })
    $("#clear").click(function () {
        console.log("onclick clear");
        function manualReset(selector = null, excludeIds = []) {
            const fields = $(selector ?? "form").find("input:not([type='button']):not([type='submit']):not([type='reset']), select");
            fields.each(function () {
                const fieldId = $(this).attr("id");
                if (!excludeIds.includes(`#${fieldId}`)) {
                    $(this).val("");
                }
            });
            //console.log(`表單 ${selector} 已重設，排除的參數為：`, excludeIds);
        }
        manualReset("#ModalForm", ["#Frequency"]);
    })
    $("#create-item").click(function () {
        console.log("onclick create-item");
        let feq = $("#InspectionForm #Frequency").val();
        if (!feq) {
            createDialogModal({ id: "DialogModal", inner: "請先選擇巡檢頻率", button: [{ className: "btn btn-export", cancel: true, text: "確定" }] })
            return
        }
        $("#ModalForm #Frequency").val(feq);
        let b = document.createElement("button");
        b.type = "button";
        b.setAttribute("data-bs-toggle", "modal");
        b.setAttribute("data-bs-target", "#AddEquipment");
        document.querySelector("body").appendChild(b);
        b.click();
        b.remove();
    })
}

/**
 * 檢查必填、其他限制並返回相對應資料
 * @returns
 */
function getCreateSaveData(sampleTr, sn) {

    let PathName = $("#InspectionForm #PathName").val(),
        Frequency = $("#InspectionForm #Frequency").val(),
        RFIDInternalCodes = sampleTr.calc()

    if (!PathName) {
        dialogError("請輸入巡檢路線名稱！")
        return;
    }
    if (!Frequency) {
        dialogError("未選擇巡檢頻率！")
        return;
    }
    if (!sampleTr.checkValidity()) {
        dialogError("請至少新增一項巡檢設備！")
        return;
    }

    let data = {
        PathName,
        Frequency,
        RFIDInternalCodes
    }

    if (sn) {
        data.PlanPathSN = sn;
    }

    return data;
    function dialogError(inner) {
        createDialogModal({ id: "DialogModal-Error", inner: inner })
    }
}