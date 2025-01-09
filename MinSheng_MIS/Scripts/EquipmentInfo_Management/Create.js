async function checkRFID() {
    //後端取得RFID
    const RFID = await $.getJSON(`/RFID/CheckRFID`)
        .then((res) => {
            if (res.ErrorMessage) {
                DT.createDialogModal("掃描失敗！<br>" + res.ErrorMessage);
                return null;
            }
            return res.Datas.trim();
        })
        .catch((ex) => {
            DT.createDialogModal("掃描失敗！" + ex.responseText);
            return null;
        });
    if (!RFID) return;

    console.log("checkRFID", RFID);


    //檢查有無重複
    const exist = Origin_RFID.findIndex((d) => d.InternalCode === RFID);
    if (exist !== -1) {
        DT.createDialogModal("此RFID已存在！");
        return;
    }

    //顯示彈跳視窗
    showRFIDPopup(RFID);   
}

//顯示 RFID彈跳視窗
async function showRFIDPopup(RFIDData, btn) {
    $('#RFIDEquipmentInfo').find('input, select, textarea').val('');
    const d = btn === 'edit' ? JSON.parse(decodeURIComponent(RFIDData)) : RFIDData;
    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('RFIDEquipmentInfo'));

    modal.show();
    await formDropdown.pushSelect({ id: "Modal_ASN", url: "/DropDownList/Area" });
    pushRFIDData(d, btn)

    //塞值進 RFID表格
    async function pushRFIDData(data, btn) {
        if (btn === "edit") {
            await formDropdown.ASN({ id: "Modal_ASN", fsnId: "Modal_FSN", value: data.ASN });
            await formDropdown.FSN({ id: "Modal_FSN", data: data.ASN, value: data.FSN });
            for (const key in data) {
                let element = document.querySelector(`#RFIDEquipmentInfo #${key}`)

                if (element) {
                    element.value = data[key];
                }
            }
        } else {
            await formDropdown.ASN({ id: "Modal_ASN", fsnId: "Modal_FSN" });
            await formDropdown.FSN({ id: "Modal_FSN" });
            $('#InternalCode').val(data);
        }
    }
}