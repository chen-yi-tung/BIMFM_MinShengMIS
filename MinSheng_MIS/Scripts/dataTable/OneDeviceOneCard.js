function SampleInfo(selector, data) {
    console.log("SampleInfo data", data)
    console.log('SampleInfo selector', $(selector));

    $(selector).append(
        data ?
            createInspectionTable({

                id: `SampleInfo`,
                sn: [
                    { text: "模板名稱", value: "SampleName" },
                ],
                data: data,
            })
            : "",
    )
}

function AddField(selector, data) {
    console.log("AddField data", data)
    console.log('AddField selector', $(selector));

    $(selector).append(
        data.AddField ?
            createInspectionTable({
                id: `AddField`,
                sn: [
                    { text: "增設欄位名稱", value: "AddField", itemNum: true },
                ],
                data: data,
            })
            : "",
    )
}

function MaintainInfo(selector, data) {
    console.log("MaintainInfo data", data)
    console.log('MaintainInfo selector', $(selector));

    $(selector).append(
        data.MaintainItems ?
            createInspectionTable({
                id: `MaintainItems`,
                sn: [
                    { text: "保養項目", value: "MaintainItems", itemNum: true },
                ],
                data: data,
            })
            : "",
    )
}

function InspectionInfo(selector, data) {
    console.log("InspectionInfo data", data)
    console.log('InspectionInfo selector', $(selector));

    $(selector).append(
        data.InspectionItems ?
            createInspectionTable({
                id: `InspectionInfo`,
                sn: [
                    { text: "巡檢頻率", value: "Ifrequency", colspan: "3" },
                    { text: "檢查項目", value: "InspectItems", colspan: "2", itemNum: "true" },
                    { text: "填報項目名稱/單位", value: "ReportItems", type: "dualCol" },
                ],
                data: data.InspectionItems,
            })
            : "",
    )
}
