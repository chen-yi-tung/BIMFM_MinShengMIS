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
        $(`.sample-path-group[data-path-id='${pathID}'] #sample-path-name`).text(pathTitle)
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
        updatePathDisplay(calcPath());
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
 * @param {string} pathID 預設為 null
 * @param {string} selector 預設為 "#current-path-display"
 */
function updatePathDisplay(path = undefined, pathID = null, selector = "#current-path-display") {
    console.log("updatePathDisplay path: ", path);
    //自動更新選項沒有開啟時，calcPath()回傳null，不做任何事
    if (path === null) { return; }

    //清空顯示後，重新帶入對應路線資料
    let display = $(selector);
    let pathID_Display = pathID && $(`.sample-path-group[data-path-id='${pathID}'] #path-display`);
    console.log(pathID_Display)
    display.empty();
    pathID_Display && pathID_Display.empty();
    if (path === undefined || path.length === 0) { return; }
    path.forEach(r => {
        let pathPoint = `<li class="breadcrumb-item">${r}</li>`;
        display.append(pathPoint);
        pathID_Display && pathID_Display.append(pathPoint);
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
    if (point.name) {
        let url = window.location.origin + `/EquipmentInfo_Management/ReadBody/${point.name}`
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
    }
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

    checkNeedSaveCurrentPath(callback);

    function callback() {
        let form = $(selector);
        if (form[0].checkValidity()) {

            $(".sample-path-draw-area").removeClass("d-none");
            sortRouteModal.autoRouteToggle(selector === "#path-form");

            $.ajax({
                url: "/InspectionPlan_Management/AddPlanPath",
                data: JSON.stringify(getDataGridData(form)),
                type: "POST",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: (res) => {
                    console.log(res);

                    let pathID = pathGroup.create(res);
                    addTooltip();

                    $("#current-path-id").val(pathID);

                    sessionStorage.setItem(`P${pathID}_pathData`, JSON.stringify(res));

                    viewerUrl = window.location.origin + res.PathSample.BIMPath;
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
    }

    function getDataGridData(form) {
        let data = {
            ASN: form.find("#ASN").val(),
            FSN: form.find("#FSN").val(),
            PathTitle: form.find("#PathTitle").val() ?? ''
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

const pathGroup = PathGroup();
function PathGroup(setting = {}) {
    let options = setting;
    this.getSetting = () => options
    this.setSetting = (s) => {
        options = s;
    }
    this.create = function (data) {
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

        new Sortable(document.querySelector(".sample-path-group-area"), {
            handle: "#path-handle",
            animation: 150,
        })

        return group.attr('data-path-id');
    }
    return this;
}
PathGroup.prototype.count = 1;



/**
 * 儲存當前路線資料到sessionStorage
 */
function saveCurrentPath(onSuccess = () => { }) {
    let pathID = $("#current-path-id").val()
    let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`))
    let title = $("#current-path-title").val()
    let PathSampleOrder = getPathSampleOrder("#current-path-display .breadcrumb-item");
    let PathSampleRecord = getPathSampleRecord();

    if (PathSampleRecord.every(e => e) === false) {
        createDialogModal({ id: "DialogModal-Error", inner: "儲存失敗！路線超出模型範圍！" })
        return
    }

    pathData.PathSample.PathTitle = title;
    pathData.PathSampleOrder = PathSampleOrder;
    pathData.PathSampleRecord = PathSampleRecord;
    sessionStorage.setItem(`P${pathID}_pathData`, JSON.stringify(pathData))

    sortRouteModal.autoRouteToggle(false);
    updatePathDisplay(PathSampleOrder, pathID);
    onSuccess();
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
    updatePathDisplay(pathData.PathSampleOrder, pathID);

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

/**
 * 獲取路線順序陣列
 * @param {string} selector 預設為".breadcrumb-item"
 * @returns {string[]} 路線順序陣列
 */
function getPathSampleOrder(selector = ".breadcrumb-item") {
    return $(selector).toArray().map(e => e.innerHTML)
}

/**
 * 獲取路線路徑點陣列
 * @returns {object[]} 路線路徑點陣列
 */
function getPathSampleRecord() {
    return ForgeDraw.getForgeLineData().map(e => (e ? { LocationX: e.x, LocationY: e.y } : null))
}

/**
 * 目前的路線資料與已儲存路線資料不一致，則回傳true
 * 
 * 若一致或無已儲存資料則回傳false
 * 
 * 若路線超出模型範圍則回傳undefined
 * @returns {boolean | undefined} 
 */
function isPathDataChange() {
    let pathID = $("#current-path-id").val();
    let oldDataStr = sessionStorage.getItem(`P${pathID}_pathData`)

    if (!oldDataStr) { return false; }

    let pathData = JSON.parse(oldDataStr);
    let title = $("#current-path-title").val()
    let PathSampleOrder = getPathSampleOrder("#current-path-display .breadcrumb-item");
    let PathSampleRecord = getPathSampleRecord();

    if (PathSampleRecord.every(e => e) === false) { return undefined; }

    pathData.PathSample.PathTitle = title;
    pathData.PathSampleOrder = PathSampleOrder;
    pathData.PathSampleRecord = PathSampleRecord;

    //console.log("old",JSON.parse(oldDataStr))
    //console.log("new",pathData)

    return JSON.stringify(pathData) !== oldDataStr;
}

/**
 * 檢查是否需要儲存路徑的標準行為 
 * @param {Function} callback 檢查完畢後要做的事
 */
function checkNeedSaveCurrentPath(callback) {
    let isChange = isPathDataChange();

    if (isChange === undefined) {
        createDialogModal({ id: "DialogModal-Error", inner: "路線超出模型範圍！" })
        return;
    }

    if (!$(".sample-path-draw-area").hasClass('d-none') && isChange === true) {
        let modal = createDialogModal({
            id: "DialogModal",
            inner: "尚未儲存目前路線變更，是否儲存？",
            button: [
                { className: "btn btn-cancel", cancel: true, text: "取消" },
                { className: "btn btn-submit", text: "是", onClick: () => { saveCurrentPath(); callback(); modal.hide(); } },
                { className: "btn btn-delete", text: "否", onClick: () => { callback(); modal.hide(); } },
            ]
        })
        return;
    }

    callback();
}

function addSaveSamplePathEvent() {
    $("#ShowSaveSamplePathModal").click(function () {
        checkNeedSaveCurrentPath(() => {
            bootstrap.Modal.getOrCreateInstance(document.querySelector('#SaveSamplePathModal')).show();
        })
    })
    $("#SaveSamplePathModal").on("shown.bs.modal", async function () {
        let pathID = $("#current-path-id").val();
        
        ForgeDraw.removeAllDevice();
        let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`));
        createBeacons(pathData.PathSample.Beacon);

        putImageToModal();
        putDataToModal();

        function getOrderWithoutEquipment(){
            let devicelist = ForgeDraw.devices;
            let pathList = $("#current-path-display .breadcrumb-item");
            let arr = []
            pathList.each((i, e) => {
                let dp = devicelist.find(dp => dp.name == e.innerHTML);
                if (dp && dp.type === "藍芽"){ arr.push(dp.name); }
            })
            return arr;
        }

        function putImageToModal() {
            const canvas = document.querySelector("#screenshot-canvas");
            let rect = view.getBoundingClientRect();
            canvas.width = rect.width;
            canvas.height = rect.height;
            const ctx = canvas.getContext('2d');
            viewer.getScreenShot(0, 0, (e) => {
                const fimg = new Image();
                fimg.onload = async function () {
                    ctx.drawImage(fimg, 0, 0);
                    const pimg = await ForgeDraw.getScreenShot();
                    ctx.drawImage(pimg, 0, 0);
                }
                fimg.src = e;
            })
        }

        function putDataToModal() {
            let pathID = $("#current-path-id").val();
            let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`));

            $("#SaveSamplePathForm_Area").val(pathData.PathSample.Area);
            $("#SaveSamplePathForm_ASN").val(pathData.PathSample.ASN);
            $("#SaveSamplePathForm_Floor").val(pathData.PathSample.Floor);
            $("#SaveSamplePathForm_FSN").val(pathData.PathSample.FSN);
            $("#SaveSamplePathForm_PathTitle").val(pathData.PathSample.PathTitle);

            updatePathDisplay(getOrderWithoutEquipment(), null, "#SaveSamplePathForm_path-display");
        }
    })
    $("#SaveSamplePathModal").on("hidden.bs.modal", async function () {
        document.querySelector(".screenshot-img-area").innerHTML = '<canvas id="screenshot-canvas"></canvas>';
        reloadDeviceData();
    })
    $("#SaveSamplePath").click(function () {
        let PathSampleOrder = getPathSampleOrder("#SaveSamplePathForm_path-display .breadcrumb-item");
        let PathSampleRecord = getPathSampleRecord();

        if (PathSampleRecord.every(e => e) === false) {
            createDialogModal({ id: "DialogModal-Error", inner: `新增失敗！路線超出模型範圍！` })
            return
        }
        if (PathSampleOrder.length === 0 || PathSampleRecord.length === 0) {
            createDialogModal({ id: "DialogModal-Error", inner: "新增失敗！沒有規劃路線資料！" });
            return;
        }

        let data = {
            PathSample: {
                ASN: $("#SaveSamplePathForm_ASN").val(),
                FSN: $("#SaveSamplePathForm_FSN").val(),
                PathTitle: $("#SaveSamplePathForm_PathTitle").val()
            },
            PathSampleOrder: PathSampleOrder,
            PathSampleRecord: PathSampleRecord
        }

        $.ajax({
            url: "/SamplePath_Management/CreateSamplePath",
            data: JSON.stringify(data),
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: onSuccess,
            error: onError
        })

        function onSuccess(res) {
            console.log(res);
            createDialogModal({
                id: "DialogModal-Success", inner: "儲存模板成功！",
                onHide: () => { bootstrap.Modal.getOrCreateInstance(document.querySelector('#SaveSamplePathModal')).hide(); }
            });
        }
        function onError(res) {
            let errtext;
            switch (res.responseText) {
                case "duplicate title":
                    errtext = "巡檢路線模板名稱重複！";
                    break;
                default:
                    errtext = "";
                    break;
            }
            createDialogModal({ id: "DialogModal-Error", inner: `新增失敗！${errtext}` })
        }
    })
}