function RepairRecord(selector, data) {
    console.log("data" ,data)
    //const sn = {
    //    EquipmentItem: [
    //        { text: "設備名稱", value: "IName", colspan: true },
    //        { text: "設備型號", value: "Model", colspan: true },
    //        { text: "所在位置", value: "Location", colspan: true },
    //        { text: "檢查項目", value: "InspectItems" },
    //        { text: "填報列表", value: "ReportItems" },
    //    ],
    //};
    console.log('selector', $(selector));
    console.log('data.IName', data.IName);

    $(selector).append(
        data.InspectionRecord ?
            createAccordion({
                id: `InspectionRecord`,
                state: data.State,
                sn: [
                        { text: "巡檢狀態", value: "IState" },
                        { text: "巡檢頻率", value: "Ifrequency" },
                        { text: "巡檢數量", value: "INum" },
                ],
                subsn: [
                    { text: "所在位置", value: "Location", colspan: true },
                    { text: "檢查項目", value: "InspectItems", type: "dualCol" },
                    { text: "填報列表", value: "ReportItems", type: "dualCol" },
                ],
                data: data.InspectionRecord,
                itemTitleKey: `IName`,
                itemSubTitleKey: `ITime`,
                layer: 2,
                icon: "clipboard-list",
            })
            : "",
    )
}

function InspectionRecord(selector, data) {
    console.log("InspectionRecord data", data)
    console.log('InspectionRecord selector', $(selector));

    $(selector).append(
        data.InspectionRecord ?
            createInspectionTable({

                id: `InspectionRecord`,
                sn: [
                    { text: "巡檢頻率", value: "Ifrequency", colspan: "3" },
                    { text: "檢查項目", value: "InspectItems", colspan: "2", itemNum: true },
                    { text: "填報項目名稱/單位", value: "ReportItems", type: "dualCol" },
                ],
                data: data.InspectionRecord,
            })
            : "",
    )
}

function EquipmentRFID(selector, data) {
    console.log("EquipmentRFID data", data)
    console.log('EquipmentRFID selector', $(selector));

    $(selector).append(
        data.RFID ?
            createAccordion({
                id: `RFID`,
                sn: [
                    { text: "RFID名稱", value: "IName" },
                    { text: "RFID內碼", value: "InterCode" },
                    { text: "RFID外碼", value: "ExterCode" },
                    { text: "棟別", value: "Area" },
                    { text: "樓層", value: "Floor" },
                    { text: "定位", value: "Location" },
                    { text: "備註", value: "Memo" },
                ],
                data: data.RFID,
                itemTitleKey: `IName`,
            })
            : "",
    )
}

function MaintainInfo(selector, data) {
    console.log("MaintainInfo data", data)
    console.log('MaintainInfo selector', $(selector));

    $(selector).append(
        data.SampleInfo.MaintainItems ?
            createInspectionTable({
                id: `MaintainInfo`,
                sn: [
                    { text: "保養項目/週期", value: "MaintainItems", type: "dualCol" },
                ],
                data: data.SampleInfo,
            })
            : "",
    )
}

function InspectionInfo(selector, data) {
    console.log("InspectionInfo data", data)
    console.log('InspectionInfo selector', $(selector));

    $(selector).append(
        data.SampleInfo.InspectionItems ?
            createInspectionTable({
                id: `InspectionInfo`,
                sn: [
                    { text: "巡檢頻率", value: "Ifrequency", colspan: "3" },
                    { text: "檢查項目", value: "InspectItems", colspan: "2", itemNum: "true" },
                    { text: "填報項目名稱/單位", value: "ReportItems", type: "dualCol" },
                ],
                data: data.SampleInfo.InspectionItems,
            })
            : "",
    )
}
