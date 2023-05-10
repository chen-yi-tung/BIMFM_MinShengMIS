function InquiryPlan(selector, data) {
    const sn = {
        InquiryUserNameData: [
            { text: "單號", value: "IOSN" },
            { text: "詢價人", value: "InquiryUserName" },
        ],
        MFRInfo: [
            { text: "供應商", value: "MFRSN" },
            { text: "聯絡人", value: "ContactPerson" },
            { text: "電話", value: "MFRTelNO" },
            { text: "手機", value: "MFRMBPhone" },
            { text: "電子郵件", value: "MFREmail" },
            { text: "地址", value: "MFRAddress" },
            { text: "網站", value: "MFRWeb" },
            { text: "主要商品", value: "MFRMainProduct" },
        ],
        PaymentInfo: [
            { text: "付款方式", value: "PaymentMethod" },
            { text: "交貨方式", value: "DeliveryMethod" },
            { text: "交貨時間", value: "DeliveryDate" },
            { text: "交貨地點", value: "DeliveryLocation" },
        ],
    };

    $(selector).append(
        createTableOuter({
            title: "詢價人資料",
            id: "InquiryUserNameData",
            className: "datatable-small mt-5",
            inner: createTableInner(data.InquiryUserNameData, sn.InquiryUserNameData),
        }),
        createTableOuter({
            title: "供應商資料",
            id: "MFRInfo",
            className: "datatable-small",
            inner: createTableInner(data.MFRInfo, sn.MFRInfo),
        }),
        createTableOuter({
            title: "交貨資訊",
            id: "PaymentInfo",
            className: "datatable-small",
            inner: createTableInner(data.PaymentInfo, sn.PaymentInfo),
        }),
    );

}