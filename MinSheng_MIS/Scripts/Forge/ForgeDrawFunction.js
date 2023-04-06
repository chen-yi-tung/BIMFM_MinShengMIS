//import '/Scripts/ForgeDraw.js'
//import '/Scripts/datatable.js'
//import '/Scripts/Forge/UpViewer.js'
//import '/Scripts/Forge/SortRouteModal.js'

/**
 * 綁定ForgeDraw Toolbar事件
 */
function addToolbarEvent() {
    $("#current-path-title").change(function () {
        let pathID = $("#current-path-id").val()
        let pathTitle = $(this).val()
        let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`))
        pathData.PathSample.PathTitle = pathTitle
        sessionStorage.setItem(`P${pathID}_pathData`, JSON.stringify(pathData))

        //修改上方路線名稱
        $(`[data-path-id=${pathID}] #sample-path-name`).text(pathTitle)
    })
    $("#current-path-edit").click(function () {
        sortRouteModal.create("#current-path-display");
    })
    $("#path-draw").click(function () {
        $(this).blur();
        controlToggle("path-draw", ForgeDraw.Control.DRAW);
    })
    $("#path-eraser").click(function () {
        $(this).blur();
        controlToggle("path-eraser", ForgeDraw.Control.ERASER);
    })
    $("#path-reload").click(function () {
        $(this).blur();
        ForgeDraw.removeAllData();
        sortRouteModal.autoRouteToggle(true);
        updatePathDisplay();
    })
    $("#path-auto").click(function () {
        $(this).blur();
        sortRouteModal.autoRouteToggle();
        updatePathDisplay(calcPath());
    })
}

/**
 * 切換ForgeDraw操作狀態
 * @param {string} id 按鈕HTML ID
 * @param {ForgeDraw.Control} control 要切換的狀態
 */
function controlToggle(id, control) {
    let btn = $("#" + id);
    ForgeDraw.setControl(control);

    $(".path-icon-radio").removeClass("active");
    btn.toggleClass("active", true);
}

/**
 * 顯示工具提示
 * @param {boolean} animate 若為是，顯示2秒後消失。預設為否
 */
function addTooltip(animate = false) {
    let tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    let tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl, {
            customClass: "sample-path-tooltip",
        })
    })
    if (animate) {
        tooltipList.forEach(e => e.show())
        setTimeout(() => {
            tooltipList.forEach(e => e.hide())
        }, 2000)
    }
}

/**
 * 用於讀取資料時，阻止使用者重複點擊按鈕導致多次提交
 * @param {boolean} bool 
 * @param {string[]} selectors
 */
function togglePointerEvent(bool, selectors = null) {
    if (selectors == null) {
        $("button").toggleClass("pe-none", !bool);
        return;
    }
    if (selectors.length !== 0) {
        selectors.forEach(e => {
            $(e).toggleClass("pe-none", !bool);
        })
    }
}

/**
 * 更新設備並計算路徑
 * @returns {string[]} path
 */
function calcPath() {
    //若有線條資料，則更新設備
    if (ForgeDraw.lineData.length > 1) {
        ForgeDraw.devices.forEach((dp) => { dp.update(); })
    }

    //若自動更新選項沒有開啟，則返回null
    if (!sortRouteModal.autoCalcRoute) { return null; }

    //開始計算路徑
    let path = ForgeDraw.getRoute().map(e => e.name);
    //console.log(ForgeDraw.devices.map(e => e.result));

    return path;
}

/**
 * 顯示路線於網頁上
 * @param {string[]} path 路線資料
 * @param {string} selector 預設為 "#current-path-display"
 */
function updatePathDisplay(path = undefined, selector = "#current-path-display") {
    console.log("updatePathDisplay path: ", path);
    let display = $(selector);

    //自動更新選項沒有開啟時，calcPath()回傳null，不做任何事
    if (path === null) { return; }

    //清空顯示後，重新帶入對應路線資料
    display.empty();
    if (path === undefined || path.length === 0) { return; }
    path.forEach(r => {
        let pathPoint = `<li class="breadcrumb-item">${r}</li>`;
        display.append(pathPoint);
    })
}

/**
 * 重新讀取模型
 * @param {string} url 
 * @param {string | number} pathID 
 * @param {function(pathID)} onload 
 */
function loadModel(url, pathID, onload) {
    ForgeDraw.removeAllDevice();
    ForgeDraw.removeAllData();
    ForgeDraw.clearSelectPos();

    //開啟forge顯示
    $(".sample-path-draw-area").removeClass('d-none')
    togglePointerEvent(false);

    viewer.loadModel(url, { keepCurrentModels: false },
        () => {
            $(viewer).one(Autodesk.Viewing.GEOMETRY_LOADED_EVENT, () => {
                $(viewer).one(Autodesk.Viewing.FINAL_FRAME_RENDERED_CHANGED_EVENT, () => {
                    onload(pathID);
                    togglePointerEvent(true);
                })
            })
        },
        (err) => { console.log(err) }
    );
}

/**
 * 根據資料生成藍芽資料點
 * @param {object[]} data 
 * @param {number} data[].dbId
 * @param {string} data[].deviceType
 * @param {string} data[].deviceName
 */
function createBeacons(data) {
    if (data.length == 0) { return }
    data.forEach((e) => {
        let btSprite = PIXI.Sprite.from("/Content/img/bluetooth.svg");
        btSprite.scale.set(0.6);
        btSprite.anchor.set(0.5);
        new ForgeDraw.DevicePoint(e, { sprite: btSprite });
    });
}
/**
 * 根據資料生成路線
 * @param {object[]} data
 * @param {number} data[].LocationX
 * @param {number} data[].LocationY
 */
function createLinePath(data) {
    if (data.length == 0) { return }
    let lineData = data.map((d) => {
        let forgePos = new THREE.Vector3(d.LocationX, d.LocationY, 0);
        let pos = viewer.worldToClient(forgePos);
        let position = new PIXI.Point(pos.x, pos.y);
        return {
            position: position,
            forgePos: forgePos
        };
    });
    ForgeDraw.readLineData(lineData);
}

/**
 * 根據資料生成設備資料點
 * 會分類成三類
 * - MaintainEquipment: 定期保養設備
 * - RepairEquipment: 維修設備
 * - BothEquipment: 維修+保養設備
 * @param {string | number} pathID 
 * @param {object[]} OldPathData
 * @param {object} options
 * @param {string} options.maintain "Maintain-datagrid"
 * @param {string} options.repair "Repair-datagrid"
 */
function createDevices(pathID, OldPathData = null, options = null) {
    let deviceDatas = JSON.parse(sessionStorage.getItem(`DeviceDatas`));
    let pathData = OldPathData ?? JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`));

    let ASN = pathData.PathSample.ASN, FSN = pathData.PathSample.FSN;

    let dataM, dataR, BothE, MaintainE, RepairE;

    if (OldPathData) {
        MaintainE = OldPathData.MaintainEquipment;
        RepairE = OldPathData.RepairEquipment;
        BothE = OldPathData.BothEquipment;
    }
    else {
        dataM = getESN(options ? options.maintain : "Maintain-datagrid");
        dataR = getESN(options ? options.repair : "Repair-datagrid");
        BothE = dataM.filter(x => dataR.includes(x));
        MaintainE = dataM.filter(x => !dataR.includes(x));
        RepairE = dataR.filter(x => !dataM.includes(x));
        pathData.MaintainEquipment = MaintainE;
        pathData.RepairEquipment = RepairE;
        pathData.BothEquipment = BothE;
    }

    sessionStorage.setItem(`P${pathID}_pathData`, JSON.stringify(pathData));

    BothE.forEach(ESN => {
        let e = deviceDatas[ASN][FSN][ESN];
        e.deviceType = "BothDevice";
        new ForgeDraw.DevicePoint(e, { color: ForgeDraw.Colors.BothDevice });
    })
    MaintainE.forEach(ESN => {
        let e = deviceDatas[ASN][FSN][ESN];
        e.deviceType = "Maintain";
        new ForgeDraw.DevicePoint(e, { color: ForgeDraw.Colors.Maintain });
    })
    RepairE.forEach(ESN => {
        let e = deviceDatas[ASN][FSN][ESN];
        e.deviceType = "Repair";
        new ForgeDraw.DevicePoint(e, { color: ForgeDraw.Colors.Repair });
    })

    function getESN(id) {
        if ($("#" + id).css("display") !== 'none') { return []; }

        let data = $("#" + id).datagrid("getRows");
        if (data.length === 0) { return []; }

        let Area = pathData.PathSample.Area;
        let Floor = pathData.PathSample.Floor;

        return data.filter((e, i, arr) => {
            return e.Area == Area && e.Floor == Floor;
        }).map((e) => e.ESN).filter((e, i, arr) => arr.indexOf(e) === i);
    }
}

/**
 * 生成設備詳細資料顯示Modal
 * @param {ForgeDraw.DevicePoint} point 
 */
function createDevicePointDetailModal(point) {
    /*if (point.dbId) {
        let url = `/EquipmentInfo_Management/ReadBody/${point.dbId}`
        $.getJSON(url, function (res) {
            createDataDetailModal({
                title: "設備資訊",
                id: "",
                sn: [
                    { text: "系統別", value: "System" },
                    { text: "子系統別", value: "SubSystem" },
                    { text: "設備名稱", value: "EName" },
                    { text: "區域", value: "Area" },
                    { text: "樓層", value: "Floor" },
                    { text: "空間名稱", value: "RoomName" },
                    { text: "國有財產編碼", value: "PropertyCode" },
                    { text: "廠牌", value: "Brand" },
                    { text: "DBID", value: "DBID" },
                    { text: "RFID", value: "RFID" },
                    { text: "設備狀態", value: "EState" },
                    { text: "座標X", value: "x" },
                    { text: "座標Y", value: "y" },
                ],
                data: Object.assign({
                    x: point.forgePos.x,
                    y: point.forgePos.y
                }, res)
            });
        })
        return;
    }*/
    createDataDetailModal({
        title: "設備資訊",
        id: "",
        sn: [
            { text: "設備名稱", value: "EName" },
            { text: "DBID", value: "DBID" },
            { text: "座標X", value: "x" },
            { text: "座標Y", value: "y" },
        ],
        data: {
            EName: point.name,
            DBID: point.dbId,
            x: point.forgePos.x,
            y: point.forgePos.y
        }
    });

}

/**
 * 生成座標詳細資料顯示Modal
 * @param {ForgeDraw.Point} point 
 */
function createPointDetailModal(point) {
    console.log(point)
    let forgePos = point.forgePos;
    createDataDetailModal({
        title: "座標資訊",
        id: "",
        sn: [
            { text: "座標X", value: "x" },
            { text: "座標Y", value: "y" },
        ],
        data: { x: forgePos ? forgePos.x : undefined, y: forgePos ? forgePos.y : undefined }
    });
}

/**
 * 新增路線
 * @param {string} selector 表單
 */
function createPath(selector) {
    if ($("#PlanDate").val() == '') {
        createDialogModal({
            id: "DialogModal",
            inner: `請輸入計畫日期！`
        })
        return;
    }
    let form = $(selector);
    if (form[0].checkValidity()) {

        $(".sample-path-draw-area").removeClass("d-none");

        $.ajax({
            url: "/InspectionPlan_Management/AddPlanPath",
            data: JSON.stringify(getDataGridData(form)),
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: (res) => {
                console.log(res);
                let pathID = pathGroup.create(res);
                $("#current-path-id").val(pathID);

                sessionStorage.setItem(`P${pathID}_pathData`, JSON.stringify(res));

                viewerUrl = '/' + res.PathSample.BIMPath;
                if (viewer) {
                    loadModel(viewerUrl, pathID, loadPath);
                }
                else {
                    initializeViewer(initializeDrawer);
                }
            },
            error: (err) => { console.log(err) }
        })
    }

    function getDataGridData(form) {
        let data = {
            ASN: form.find("#ASN").val(),
            FSN: form.find("#FSN").val(),
            PathTitle: form.find("#PathTitle").val() ?? '',
            PlanDate: $("#PlanDate").val(),
        };
        //console.log("getDataGridData: ", data);
        return data;
    }
}

/**
 * @typedef PathGroupButtonOptions
 * @property {string} id
 * @property {string} icon
 * @property {string} tooltip
 * @property {Function} onClick
 * 
 * @param {object} options 
 * @param {PathGroupButtonOptions?} options.startButton
 * @param {PathGroupButtonOptions?} options.endButton
 * @param {boolean?} options.delete
 * @returns {PathGroup}
 */
function PathGroup(options) {
    PathGroup.prototype.count = 1;
    PathGroup.prototype.create = function (data) {
        let path = data.PathSampleOrder ? data.PathSampleOrder.map((e) => `<li class="breadcrumb-item">${e}</li>`).join('') : '';
        let PathTitle = data.PathSample ? data.PathSample.PathTitle : '';
        let group = $(`
            <div class="input-group sample-path-group" data-path-id="${PathGroup.prototype.count}">
                <div class="input-group-text">
                    ${options.startButton ? `
                    <button class="path-btn me-2" id="${options.startButton.id}" data-bs-toggle="tooltip" title="${options.startButton.tooltip}">
                    <i class="${options.startButton.icon}">
                    </i></button>` : ''}
                    <span id="sample-path-name">${PathTitle}</span>
                </div>
                <div type="text" class="form-control">
                    <nav>
                        <ol class="breadcrumb" id="path-display">
                            ${path}
                        </ol>
                    </nav>
                    ${options.endButton ? `
                    <button class="path-btn" id="${options.endButton.id}" data-bs-toggle="tooltip" title="${options.endButton.tooltip}">
                    <i class="${options.endButton.icon}">
                    </i></button>` : ''}
                </div>
                ${options.delete ? '<button class="btn-delete-item" data-bs-toggle="tooltip" title="刪除路線"></button>' : ''}
            </div>`);

        $("#current-path-title").val(PathTitle);

        $(".sample-path-group-area").append(group);

        options.delete ? group.find(".btn-delete-item").click(function () {
            let dm = createDialogModal({
                id: "DialogModal", inner: `確定刪除此路線？`,
                button: [
                    {
                        className: "btn btn-cancel",
                        cancel: true,
                        text: "取消",
                    },
                    {
                        className: "btn btn-delete",
                        text: "確定",
                        onClick: function () {
                            let id = group.attr('data-path-id');
                            sessionStorage.removeItem(`P${id}_pathData`);
                            group.remove();
                            $(".sample-path-draw-area").addClass('d-none');
                            dm.hide();
                        }
                    },
                ]
            })
        }) : false;

        options.startButton ? group.find("#" + options.startButton.id).click(options.startButton.onClick) : false;
        options.endButton ? group.find("#" + options.endButton.id).click(options.endButton.onClick) : false;

        PathGroup.prototype.count++;

        return group.attr('data-path-id');
    }
    return this;
}

/**
 * 儲存當前路線資料到sessionStorage
 */
function saveCurrentPath() {
    let pathID = $("#current-path-id").val()
    let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`))
    let title = $("#current-path-title").val()
    let PathSampleOrder = $("#current-path-display .breadcrumb-item").toArray().map(e => e.innerHTML);
    let PathSampleRecord = ForgeDraw.getForgeLineData().map(e => {
        return { LocationX: e.x, LocationY: e.y };
    })
    pathData.PathSample.PathTitle = title;
    pathData.PathSampleOrder = PathSampleOrder;
    pathData.PathSampleRecord = PathSampleRecord;
    sessionStorage.setItem(`P${pathID}_pathData`, JSON.stringify(pathData))
}

/**
 * 讀取指定路線
 * @param {string | number} pathID 
 */
function loadPath(pathID) {
    $("#current-path-id").val(pathID);
    let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`))

    createBeacons(pathData.PathSample.Beacon);
    createDevices(pathID);
    createLinePath(pathData.PathSampleRecord);
    updatePathDisplay(pathData.PathSampleOrder);

    updatePathDisplay(calcPath());
}

/**
 * 從資料庫讀取設備資料
 * @param {string[]} data 從`$(dg).datagrid.('getRows').map(d => d.ESN)`獲取ESN陣列
 */
function getDeviceData(data) {
    let ESNDatas = JSON.parse(sessionStorage.getItem(`ESNDatas`));
    //不重複資料
    let postData = ESNDatas ? new Set(data.filter(e => !ESNDatas.includes(e))) : new Set(data);

    if (postData.size !== 0) {
        $.ajax({
            url: "/InspectionPlan_Management/ResponseDeviceInfo",
            data: JSON.stringify([...postData]),
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: (res) => {
                console.log("getDeviceData onSuccess", res)

                storeESNData(postData, ESNDatas);
                storeDeviceData(res.DeviceData);
            },
            error: (err) => { console.log(err) }
        })
    }

    function storeESNData(postData, ESNDatas) {
        let esnSet = ESNDatas ? new Set([...postData, ...ESNDatas]) : postData;
        sessionStorage.setItem(`ESNDatas`, JSON.stringify([...esnSet]));
    }

    function storeDeviceData(data) {
        let DeviceDatas = JSON.parse(sessionStorage.getItem(`DeviceDatas`)) ?? {};
        for (let i = 0; i < data.length; i++) {
            let e = data[i];

            DeviceDatas[e.ASN] ?? (DeviceDatas[e.ASN] = {})
            DeviceDatas[e.ASN][e.FSN] ?? (DeviceDatas[e.ASN][e.FSN] = {})
            DeviceDatas[e.ASN][e.FSN][e.ESN] = filterData(e);
        }

        sessionStorage.setItem(`DeviceDatas`, JSON.stringify(DeviceDatas));

        function filterData(d) {
            let fos = d.Position ? new THREE.Vector3(d.Position.LocationX, d.Position.LocationY, 0) : undefined;
            let pos = fos ? viewer.worldToClient(fos) : undefined;
            return {
                dbId: d.DBID,
                deviceType: "device",
                deviceName: d.ESN,
                position: pos ? new PIXI.Point(pos.x, pos.y) : undefined,
                forgePos: fos
            }
        }
    }
}

/**
 * 若繪圖區域處於顯示狀態，
 * 則重新讀取設備資料並生成設備資料點
 */
function reloadDeviceData() {
    if (!$(".sample-path-draw-area").hasClass('d-none')) {
        let pathID = $("#current-path-id").val();
        ForgeDraw.removeAllDevice();

        let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`));
        createBeacons(pathData.PathSample.Beacon);
        createDevices(pathID);

        updatePathDisplay(calcPath());
    }
}


function getPathSampleOrder(selector = ".breadcrumb-item") {
    return $(selector).toArray().map(e => e.innerHTML)
}

function getPathSampleRecord() {
    return ForgeDraw.getForgeLineData().map(e => (e ? { LocationX: e.x, LocationY: e.y } : null))
}