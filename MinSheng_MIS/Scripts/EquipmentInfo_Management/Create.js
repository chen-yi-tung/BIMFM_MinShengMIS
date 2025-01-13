const DEBUG_TEST = false;
async function init_Create() {
    const fileUploader = new FileUploader({
        container: "#FileUploader",
        className: "form-group col-3fr required",
        icon: "plus",
        buttonText: "上傳照片",
        label: "照片",
        id: "EPhoto",
    });

    document.getElementById("back").onclick = () => history.back();
    document.getElementById("submit").onclick = () => save();
    const RFIDScanBtn = new RFID_ScanButton({
        id: "rfid",
        fake: DEBUG_TEST,
        onScanEnd(RFID) {
            //檢查有無重複
            const exist = RFIDGrid.grid.getRowIndex(RFID);
            if (exist !== -1) {
                DT.createDialogModal("此RFID已存在！");
                return;
            }

            RFIDModal.setData({ InternalCode: RFID });
            RFIDModal.show();
        },
    });
    const RFIDModal = new RFID_Modal({
        template: document.getElementById("RFIDModal"),
        submitButtonSelector: "#add-row",
        async init() {
            this.bim = new UpViewer(this.modal.querySelector("#BIM"));

            this.viewNames = await $.getJSON("/DropDownList/ViewName");
            const asn = this.modal.querySelector("#ASN");
            const fsn = this.modal.querySelector("#FSN");
            if (!asn.dataset.fdInitialized) {
                fsn.addEventListener("change", async () => {
                    this.bim.unloadModels();
                    if (!fsn.value) return;
                    const viewName = this.viewNames.find((x) => x.FSN === fsn.value)?.Value;
                    if (!viewName) {
                        console.error("[RFIDModal FSN change event] Not found ViewName.");
                        return;
                    }
                    await this.bim.loadModels(this.bim.getModelsUrl(viewName));
                    this.bim.activateEquipmentPointTool(new THREE.Vector3(this.data?.LocationX ?? 0, this.data?.LocationY ?? 0, 0), true);
                });
            }
            await formDropdown.ASN({ id: asn, fsnId: fsn });

            this.modal.addEventListener("show.bs.modal", async () => {
                await this.bim.init();
                fsn.dispatchEvent(new Event("change"));
            });
            this.modal.addEventListener("hidden.bs.modal", async () => {
                this.bim.dispose();
            });
        },
        setData(data = {}) {
            this.modal.querySelector("#isEdit").value = data.isEdit ?? false;
            this.modal.querySelector("#InternalCode").value = data.InternalCode ?? "";
            this.modal.querySelector("#ExternalCode").value = data.ExternalCode ?? "";
            formDropdown.ASN({
                id: this.modal.querySelector("#ASN"),
                fsnId: this.modal.querySelector("#FSN"),
                value: data.ASN,
                fsnValue: data.FSN,
            });
            this.modal.querySelector("#Memo").value = data.Memo ?? "";
            this.data = data;
        },
        getData() {
            const ASN = this.modal.querySelector("#ASN").value ?? null;
            const Area = this.modal.querySelector(`#ASN option[value="${ASN}"]`).textContent ?? null;
            const FSN = this.modal.querySelector("#FSN").value ?? null;
            const Floor = this.modal.querySelector(`#FSN option[value="${FSN}"]`).textContent ?? null;
            const location = this.bim.equipmentPointTool.getPosition();
            const data = {
                isEdit: this.modal.querySelector("#isEdit").value,
                InternalCode: this.modal.querySelector("#InternalCode").value,
                ExternalCode: this.modal.querySelector("#ExternalCode").value,
                Name: this.modal.querySelector("#Name").value,
                ASN,
                Area,
                FSN,
                Floor,
                Memo: this.modal.querySelector("#Memo").value,
                LocationX: location.x,
                LocationY: location.y,
            };
            this.data = data;
            return data;
        },
        async submit() {
            const form = this.modal.querySelector("form");
            if (!form.reportValidity()) {
                return;
            }
            
            const data = this.getData();

            if (data.isEdit === "true") {
                RFIDGrid.edit(data);
                this.hide();
                return;
            }
            data.isEdit = true;
            RFIDGrid.add(data);
            this.hide();
        },
        reset() {
            this.setData();
        },
    });
    const RFIDGrid = new RFID_Grid({
        container: document.getElementById(`RFIDGridContainer`),
        maxRowSize: 100,
        grid: {
            id: "RFID_Gird",
            type: "grid",
            className: "datatable-grid form-group col-3fr mt-2 mt-md-0",
            bodyClassName: "h-100 overflow-auto",
            items: {
                thead: true,
                metaKey: "InternalCode",
                columns: [
                    {
                        id: "_Edit",
                        type: "btn",
                        width: 130,
                        tdClassName: "p-2",
                        btnClassName: "d-flex justify-content-center gap-2",
                        button: [
                            {
                                text: "編輯",
                                className: "btn btn-datagrid",
                                onClick(e, v, row) {
                                    row.isEdit = true;
                                    RFIDModal.setData(row);
                                    RFIDModal.show();
                                },
                            },
                            {
                                icon: "fa-solid fa-location-dot",
                                className: "btn btn-location",
                                onClick(e, v, row) {
                                    RFIDLocationModal.setData(row);
                                    RFIDLocationModal.show();
                                },
                            },
                        ],
                    },
                    { id: "InternalCode", title: "RFID內碼", width: 130 },
                    { id: "ExternalCode", title: "RFID外碼", width: 130 },
                    { id: "Name", title: "名稱" },
                    { id: "Area", title: "棟別" },
                    { id: "Floor", title: "樓層" },
                    { id: "Memo", title: "備註" },
                    {
                        id: "_Delete",
                        type: "btn",
                        className: "p-1 pt-2",
                        width: 48,
                        button: {
                            className: "btn-delete-item",
                            onClick(e, v, row) {
                                RFIDGrid.remove(row);
                            },
                        },
                    },
                ],
            },
        },
        onAdd() {
            RFIDScanBtn.disabled = RFIDGrid.checkMaxRowSize();
        },
        onRemove() {
            RFIDScanBtn.disabled = RFIDGrid.checkMaxRowSize();
        },
    });
    const RFIDLocationModal = new RFID_Location_Modal();
    RFIDModal.init();

    formDropdown.ASN();
    formDropdown.pushSelect({
        id: "SampleName",
        url: "/DropDownList/OneDeviceOneCardTemplates",
    });

    init_SampleName();

    function save() {
        //指定驗證的form
        const form = document.getElementById("EquipForm");
        let isValidity = [...form.elements].map((e) => e.reportValidity()).every((e) => e);
        if (!isValidity) {
            console.log("驗證不通過");
            return;
        }

        const TSN = document.getElementById("SampleName").value;
        const SampleData = IntegrateSampleData(TSN);
        const fd = new FormData();

        //有選模板的話，才傳送模板資訊
        if (TSN) {
            fd.append("TSN", TSN);
            SampleData.AddFieldList.forEach((item, i) => {
                for (const key in item) {
                    fd.append(`AddFieldList[${i}][${key}]`, item[key]);
                }
            });
            SampleData.MaintainItemList.forEach((item, i) => {
                for (const key in item) {
                    fd.append(`MaintainItemList[${i}][${key}]`, item[key]);
                }
            });
        }

        if (fileUploader.hasFile()) {
            fd.append("EPhoto", fileUploader.getFile());
        }
        document.querySelectorAll("#basicZone input, #basicZone select, #basicZone textarea").forEach((el) => {
            if (el.id && el.type !== "file" && el.type !== "checkbox" && el.id !== "ASN") {
                fd.append(el.id, el.value);
            }
        });
        RFIDGrid.data.forEach((item, i) => {
            const data = {
                InternalCode: item.InternalCode,
                ExternalCode: item.ExternalCode,
                Name: item.Name,
                FSN: item.FSN,
                Location_X: item.LocationX,
                Location_Y: item.LocationY,
                Memo: item.Memo,
            }
            for (const key in data) {
                fd.append(`RFIDList[${i}][${key}]`, data[key]);
            }
        });

        console.log("最後傳出的資料為", Object.fromEntries(fd));

        if (DEBUG_TEST) return;

        $.ajax({
            url: "/EquipmentInfo_Management/CreateEquipment",
            data: fd,
            type: "POST",
            contentType: false,
            processData: false,
            success: onSuccess,
            error: onError,
        });

        function onSuccess(res) {
            console.log(res);
            if (res.ErrorMessage) {
                createDialogModal({ id: "DialogModal-Error", inner: `新增失敗！${res.ErrorMessage || ""}` });
                return;
            }
            createDialogModal({
                id: "DialogModal-Success",
                inner: "新增成功！",
                onHide: () => {
                    window.location.href = "/EquipmentInfo_Management/Index";
                },
            });
        }
        function onError(res) {
            console.log(res);
            createDialogModal({ id: "DialogModal-Error", inner: `新增失敗！${res?.responseText || ""}` });
        }

        function IntegrateSampleData(TSN) {
            if (TSN) {
                //整理 增設基本資料欄位
                const addItems = $("#addItemZone")
                    .children()
                    .toArray()
                    .map((e) => ({
                        AFSN: $(e).find("input[name^='addField_SN']").val(),
                        Value: $(e).find("input[name^='addField_value']").val(),
                    }));

                //整理 保養項目/週期/下次保養日期
                const maintainItems = $("#MaintainZone")
                    .children()
                    .toArray()
                    .map((e) => ({
                        MISSN: $(e).find("input[name^='addField_SN']").val(),
                        Period: $(e).find("select[name^='period']").val(),
                        NextMaintainDate: $(e).find("input[name^='nextMaintainDate']").val(),
                    }));

                return {
                    AddFieldList: addItems,
                    MaintainItemList: maintainItems,
                };
            }
        }
    }
}

function init_SampleName() {
    //監聽 模板名稱，有選擇就顯示模板資訊內容
    document.getElementById("SampleName").addEventListener("change", function () {
        const sampleContent = document.getElementById("sampleContent");
        if (this.value) {
            sampleContent.style.display = "flex";
        } else {
            sampleContent.style.display = "none";
        }

        if (this.value !== "") {
            $.ajax({
                url: `/OneDeviceOneCard_Management/ReadBody/${this.value}`,
                type: "GET",
                contentType: "application/json",
                processData: false,
                success: onSuccess,
                error: onError,
            });
        }

        function onSuccess(res) {
            const data = res.Datas;
            if (!data) {
                console.log("無模板資訊內容");
                return;
            }
            showSampleContent(data);

        }
        function onError(res) {
            console.log("拿取模板名稱失敗");
        }
    });

    //建立 模板內容
    function showSampleContent(Sampledata, data) {
        const addFieldSection = document.getElementById("addFieldSection");
        const maintainSection = document.getElementById("maintainSection");
        const inspectionInfoSection = document.getElementById("inspectionInfoSection");

        //先清空，防止舊資料疊加
        document.getElementById("addItemZone").innerHTML = "";
        document.getElementById("MaintainZone").innerHTML = "";
        document.getElementById("InspectionRecord").innerHTML = "";

        if (Sampledata.AddItemList.length > 0) {
            createAddItem(Sampledata, data?.AddItemList);
            addFieldSection.style.display = "block";
        } else {
            addFieldSection.style.display = "none";
        }

        if (Sampledata.MaintainItemList.length > 0) {
            createMaintainItem(Sampledata.MaintainItemList, "MaintainZone", data?.MaintainItemList);
            maintainSection.style.display = "block";
        } else {
            maintainSection.style.display = "none";
        }

        if (Sampledata.Frequency) {
            DT.createTable("#InspectionRecord", {
                className: "border-0 w-100",
                data: Sampledata,
                items: [
                    { title: "巡檢頻率", id: "Frequency", formatter: (v) => `每${v}小時` },
                    {
                        title: "檢查項目", id: "CheckItemList", type: "items", items: [
                            { id: "Value", className: "text-start" }
                        ]
                    },
                    {
                        title: "填報項目名稱/單位", id: "ReportItemList", type: "items", items: [
                            { id: "Value", className: "text-start" },
                            { id: "Unit", width: 80 },
                        ]
                    },
                ]
            })
            inspectionInfoSection.style.display = "block";
        } else {
            inspectionInfoSection.style.display = "none";
        }

        if (!data) return;
        //監聽 模板的保養週期變更時，若有上次保養日期則自動換算下次保養日期
        document.querySelectorAll('select[data-name="Period"]').forEach(select => {
            select.addEventListener('change', function (event) {
                const selectValue = event.target.value;

                const parentDiv = event.target.closest('div');
                const input_SN = parentDiv.querySelector('input[id^="addField_SN"]');
                const input_NextMaintainDate = parentDiv.querySelector('input[data-name="NextMaintainDate"]');

                if (input_SN) {
                    const matchedData = data.MaintainItemList.find(item => item.MISSN === input_SN.value);

                    if (matchedData) {
                        const LastMaintainDate = matchedData.LastMaintainDate;

                        computeNextMaintainDate(selectValue, LastMaintainDate, input_NextMaintainDate);
                    }
                }
            });
        });

        //新增 增設基本資訊欄位
        function createAddItem(SampleData, data) {
            const addItemZone = document.getElementById("addItemZone");
            SampleData.AddItemList.forEach((field, index) => {
                const div = document.createElement("div");
                div.className = "form-group";

                const input_SN = document.createElement("input");
                input_SN.type = "text";
                input_SN.id = `addField_SN-${index + 1}`;
                input_SN.name = `addField_SN-${index + 1}`;
                input_SN.hidden = true;

                const label = document.createElement("label");
                const addItemLabel = SampleData.AddItemList.find(label => label.AFSN === field.AFSN);
                label.setAttribute("for", `addField-${index + 1}`);
                label.textContent = addItemLabel.Value;

                const input_value = document.createElement("input");
                input_value.className = "form-control";
                input_value.type = "text";
                input_value.id = `addField_value-${index + 1}`;
                input_value.name = `addField_value-${index + 1}`;
                input_value.dataset.afsn = field.AFSN;

                //塞值
                if (data) {
                    const addItemValue = data.find(item => item.AFSN === field.AFSN);
                    input_value.value = addItemValue.Value;
                } else {
                    input_SN.value = field.AFSN;
                }

                div.appendChild(input_SN);
                div.appendChild(label);
                div.appendChild(input_value);
                addItemZone.appendChild(div);

            });
        }
        //計算下次保養日期
        function computeNextMaintainDate(period, LastMaintainDate, NextMaintainDate) {
            let unit = '';
            switch (period) {
                case '1': unit = 'day'; break;
                case '2': unit = 'month'; break;
                case '3': unit = 'quarter'; break;
                case '4': unit = 'year'; break;
            }

            if (LastMaintainDate && LastMaintainDate !== null) {
                let date = null;
                if (unit === 'quarter') {
                    date = dayjs(LastMaintainDate).add('3', 'month');
                } else {
                    date = dayjs(LastMaintainDate).add('1', unit);
                }
                NextMaintainDate.value = date.format('YYYY-MM-DD');
            }
        }
    }
}

