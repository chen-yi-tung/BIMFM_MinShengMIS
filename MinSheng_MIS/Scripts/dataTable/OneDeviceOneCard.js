function SampleInfo(selector, data) {

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
    $(selector).append(
        data.AddItemList ?
            createInspectionTable({
                id: `AddItemList`,
                sn: [
                    { text: "增設欄位名稱", value: "AddItemList", itemNum: "Value" },
                ],
                data: data,
            })
            : "",
    )
}
function MaintainInfo(selector, data) {
    $(selector).append(
        data.MaintainItemList ?
            createInspectionTable({
                id: `MaintainItemList`,
                sn: [
                    { text: "保養項目", value: "MaintainItemList", itemNum: "Value" },
                ],
                data: data,
            })
            : "",
    )
}
function InspectionInfo(selector, data) {

    $(selector).append(
        data ?
            createTableInner(
                data,
                [
                    {
                        text: "巡檢頻率",
                        value: "Frequency",
                        colspan: true
                    }
                ]
            )
            : "",
    )
    $(selector).append(
        data.InspectItemList ?
            createTableInner(data,
                [
                    {
                        text: "檢查項目",
                        value: "InspectItemList",
                        itemNum: [
                            { value: "Value" },
                        ],
                        colspan: true
                    },
                ]
            )
            : "",
    )
    $(selector).append(
        data.ReportItemList ?
            createTableInner(data,
                [
                    {
                        text: "填報項目名稱/單位",
                        value: "ReportItemList",
                        itemNum: [
                            { value: "Value" },
                            { value: "Unit" },
                        ],
                    },
                ]
            )
            : "",
    )

    //$(selector).append(
    //    data.InspectItemList ?
    //        createInspectionTable({
    //            id: `InspectionInfo`,
    //            sn: [
    //                { text: "巡檢頻率", value: "Ifrequency", colspan: "3" },
    //                { text: "檢查項目", value: "InspectItems", colspan: "2", itemNum: "true" },
    //                { text: "填報項目名稱/單位", value: "ReportItems", type: "dualCol" },
    //            ],
    //            data: data.InspectItemList,
    //        })
    //        : "",
    //)
}

function ShowEquipment(selector, data, addItems) {
    //轉換日期格式
    if (Array.isArray(data)) {
        data.forEach(item => {
            if (item.InstallDate) {
                item.InstallDate = item.InstallDate.replace(/-/g, "/");
            }
        });
    }
    //console.log("data", data)
    //console.log("addItems", addItems)
    //console.log('selector', $(selector));
    //console.log('data.IName', data.IName);

    $(selector).append(
        data ?
            createAccordion({
                id: "Equipment",
                type: "addEquipmentSetting",
                sn: [
                    { text: "設備圖片", value: "FilePath", url: true },
                    { text: "棟別", value: "ASN" },
                    { text: "樓層", value: "FSN" },
                    { text: "設備廠牌", value: "Brand" },
                    { text: "設備型號", value: "Model" },
                    { text: "設備廠商", value: "Vendor" },
                    { text: "連絡電話", value: "ContactPhone" },
                    { text: "安裝日期", value: "InstallDate" },
                    { text: "使用電壓", value: "OperatingVoltage" },
                    { text: "其他耗材資料", value: "OtherInfo" },
                    { text: "備註", value: "Memo" },
                ],
                ESN: `ESN`,
                data: data,
                addItems: addItems,
                itemTitleKey: `EName`,
                itemSubTitleKey: `NO`,
            })
            : "",
    )
}