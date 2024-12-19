const fakeData = {
    ChartInspectionEquipmentState: [
        { label: "報修", value: 10 },
        { label: "維修", value: 16 },
        { label: "保養", value: 15 },
    ],
    ChartInspectionCompleteState: [
        { label: "已完成", value: 10 },
        { label: "執行中", value: 16 },
        { label: "待執行", value: 15 },
    ],
    EquipmentOperatingState: [
        {
            name: '液位開關',
            items: [
                { state: '3' },
                { state: '3' },
            ]
        },
        {
            name: '進流抽水泵浦',
            items: [
                { state: '3' },
                { state: '3' },
                { state: '3' },
                { state: '3' },
                { state: '3' },
                { state: '1' },
                { state: '2' },
                { state: void 0 },
            ]
        },
        {
            name: '防爆液位開關',
            items: [
                { state: '3' },
                { state: '3' },
            ]
        },
        {
            name: '機械式粗攔污柵(防爆)',
            items: [
                { state: '3' },
                { state: '1' },
                { state: '1' },
            ]
        },
        {
            name: '粗攔污柵進流閘門1',
            value: 68,
            unit: '%'
        },
        {
            name: '粗攔污柵進流閘門2',
            value: 72,
            unit: '%'
        },
    ],
    EnvironmentInfo: [
        { label: "O2", value: 24, unit: 'ppm' },
        { label: "CH4", value: 66.2, unit: 'ppm' },
        { label: "H2S", value: 800, unit: 'ppm' },
        { label: "CO", value: 0, unit: 'ppm' },
    ],
    ChartInspectionAberrantLevel: [
        { label: "軌跡偏移", value: 20 },
        { label: "緊急按鈕", value: 15 }
    ],
    ChartInspectionAberrantResolve: [
        { label: "待處理", value: 15 },
        { label: "處理中", value: 15 },
        { label: "處理完成", value: 53 },
    ],
    InspectionCurrentPos: {
        current: [
            {
                name: '任趙仁',
                heart: 110,
                heartAlert: false,
                location: '進流抽水站 B2F',
                time: '2024/12/09 15:19',
                alert: [],
                position: {
                    x: 100,
                    y: 100
                },
            },
            {
                name: '王曉明',
                heart: 110,
                heartAlert: false,
                location: '進流抽水站 B2F',
                time: '2024/12/09 15:19',
                alert: [],
                position: {
                    x: 100,
                    y: 100
                },
            },
        ],
        another: [
            {
                name: '易施詩',
                heart: 110,
                heartAlert: true,
                location: '進流抽水站 B2F',
                time: '2024/12/09 15:19',
                alert: [
                    { state: '1', type: '1', label: '心率異常' },
                    { state: '1', type: '2', label: '路線偏移' },
                    { state: '2', type: '3', label: '停留過久' },
                ],
                position: {
                    x: 100,
                    y: 100
                },
            },
            {
                name: '王大明',
                heart: 110,
                heartAlert: false,
                location: '進流抽水站 B2F',
                time: '2024/12/09 15:19',
                alert: [],
                position: {
                    x: 100,
                    y: 100
                },
            },
        ],
    }
}
const bim = new BIM()
const FSN = "1_1"
const currentLocation = new CurrentLocation()
window.addEventListener('load', async () => {
    // #region chart options
    const pieSize = 138
    const family = 'Noto Sans TC, sans-serif'
    const legend = { display: false }
    const tooltip = {
        bodyFont: { family, size: 12 },
        callbacks: {
            title: () => '',
            label: (context) => {
                let label = context.label ?? '';
                let value = context.formattedValue ?? '';
                return ` ${label}：${value}`;
            }
        }
    }
    const pieBackground = {
        backgroundColor: "#fff",
        shadow: { color: "rgba(0, 0, 0, 0.25)", blur: 4, offset: { x: 0, y: 4 } }
    }
    const calcPercentage = (data, labels, options = { maximumFractionDigits: 1 }) => {
        const d = data?.datasets?.[0]?.data;
        if (d?.length === 0) return "-";
        const total = d.reduce((t, e) => t + e, 0);
        if (!Array.isArray(labels)) {
            labels = [labels]
        }
        const value = labels.reduce((t, e) => t + getValue(data, e), 0)
        const percentage = value / total * 100;
        const f = new Intl.NumberFormat('en', options)
        return f.format(percentage) + "%";

        function getValue(data, label) {
            const index = data.labels.findIndex(x => x === label);
            if (index === -1) {
                console.warn("[calcPercentage] please select a exist label")
                return 0
            };
            return data.datasets[0].data[index]
        }
    }

    // #endregion

    // #region chart
    init()
    async function init() {
        currentLocation.init();

        const data = { FSN };
        const res = await $.ajax({
            url: "/InspectionPlan_Management/GetCurrentPositionData",
            data,
            type: "GET",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
        })

        // use fakeData
        Object.entries(res).forEach(([k, v]) => {
            if (v?.length === 0) {
                res[k] = fakeData?.[k];
            }
        })
        if (res['InspectionCurrentPos'].current?.length === 0) {
            res['InspectionCurrentPos'].current = fakeData?.InspectionCurrentPos?.current;
        }
        if (res['InspectionCurrentPos'].another?.length === 0) {
            res['InspectionCurrentPos'].another = fakeData?.InspectionCurrentPos?.another;
        }

        console.log(res);

        ChartInspectionEquipmentState(res?.ChartInspectionEquipmentState)
        ChartInspectionCompleteState(res?.ChartInspectionCompleteState)
        EquipmentOperatingState(res?.EquipmentOperatingState)
        EnvironmentInfo(res?.EnvironmentInfo)
        ChartInspectionAberrantLevel(res?.ChartInspectionAberrantLevel)
        ChartInspectionAberrantResolve(res?.ChartInspectionAberrantResolve)
        InspectionCurrentPos(res?.InspectionCurrentPos)


        const ViewName = '進抽站-B2F';
        await bim.setup(ViewName)

        //bim.hideWall()
        bim.createBeaconPoint()
        /* bim.createSamplePath()
        const pathRecord = await bim.createPathRecord()
        document.body.addEventListener('keydown', (e) => {
            console.log("keydown", e.code)
            if (e.code === 'Space') {
                pathRecord.start()
            }
        })
        console.log("%c請按空白鍵開始動畫", "color:green;font-size: 2em;padding:0.5rem;") */

        currentLocation.addEventListener('change', (e) => {
            bim.destroy()
            bim.setup(e.value)
        })
    }

    // #endregion

    // #region chart function
    //本日報修、維修及保養統計
    function ChartInspectionEquipmentState(data) {
        const container = document.getElementById('ChartInspectionEquipmentState');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        ctx.width = pieSize
        ctx.height = pieSize
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '報修狀況',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, pieBackground,
                    htmlLegend: {
                        statistics: {
                            value: (data) => data.reduce((t, e) => t + e, 0),
                            unit: "總計畫數"
                        },
                        percentage: true,
                        value: true,
                    }
                }
            },
            plugins: [
                chartPlugins.pieBackground,
                chartPlugins.htmlLegend
            ]
        })
    }
    //本日巡檢計畫進度
    function ChartInspectionCompleteState(data) {
        const container = document.getElementById('ChartInspectionCompleteState');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        ctx.width = pieSize
        ctx.height = pieSize
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '本日巡檢計畫進度',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, pieBackground,
                    htmlLegend: {
                        statistics: {
                            value: (data) => data.reduce((t, e) => t + e, 0),
                            unit: "總計畫數"
                        },
                        percentage: true,
                        value: true,
                    }
                }
            },
            plugins: [
                chartPlugins.pieBackground,
                chartPlugins.htmlLegend
            ]
        })
    }
    //設備運轉狀態
    function EquipmentOperatingState(data) {
        const container = document.querySelector('#EquipmentOperatingState .simplebar-content');
        const htmls = data.map((d) => {
            return `<div class="item">
                <span class="name">${d.name}</span>
                ${isBoxs(d) ? createBoxs(d) : createValue(d)}
            </div>`

            function isBoxs(d) {
                return d?.items?.length > 0;
            }
            function createBoxs(d) {
                const boxs = d.items.map((e) => {
                    return `<i class="box ${getState(e.state)}"></i>`
                })
                return `<div class="boxs">${boxs.join('')}</div>`;
            }
            function createValue(d) {
                return `<span class="value">${d.value}</span><span class="unit">${d.unit}</span>`;
            }
            function getState(s) {
                switch (s) {
                    case "3":
                        return "red";
                    case "2":
                        return "orange";
                    case "1":
                        return "green";
                    default:
                        return "undefined";
                }
            }
        })
        container.replaceChildren();
        container.insertAdjacentHTML('beforeend', htmls.join(''));
    }
    //環境資訊
    function EnvironmentInfo(data) {
        const container = document.getElementById('EnvironmentInfo');
        const htmls = data.map((d) => {
            return `<div class="item" id="EnvironmentInfo_${d.label}">
                <span class="label">${d.label}</span>
                <span class="value">${d.value}</span>
                <span class="unit">${d.unit}</span>
            </div>`
        })
        container.replaceChildren();
        container.insertAdjacentHTML('beforeend', htmls.join(''));
    }
    //巡檢異常狀態 等級占比
    function ChartInspectionAberrantLevel(data) {
        const container = document.getElementById('ChartInspectionAberrantLevel');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#2CB6F0", "#E77272"]
        ctx.width = pieSize
        ctx.height = pieSize
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '巡檢異常狀態等級占比',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                    cutout: "60%"
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, pieBackground,
                    centerText: {
                        text: [
                            {
                                string: "等級占比",
                                color: "#000",
                                font: { family, weight: 500, size: 14 }
                            }
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: (data) => data.reduce((t, e) => t + e, 0),
                            unit: "總異常狀態數"
                        },
                        percentage: true,
                        value: true,
                    }
                }
            },
            plugins: [
                chartPlugins.pieBackground,
                chartPlugins.centerText,
                chartPlugins.htmlLegend
            ]
        })
    }
    //巡檢異常狀態 處理狀況
    function ChartInspectionAberrantResolve(data) {
        const container = document.getElementById('ChartInspectionAberrantResolve');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#E77272", "#FFA54B", "#72E998"]
        ctx.width = pieSize
        ctx.height = pieSize
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '巡檢異常狀態處理狀況',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                    cutout: "60%"
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, pieBackground,
                    centerText: {
                        text: [
                            {
                                string: "處理狀況",
                                color: "#000",
                                font: { family, weight: 500, size: 12, lineHeight: 1.25 }
                            },
                            {
                                string: (data) => calcPercentage(data, '處理完成'),
                                color: "#E77272",
                                font: { family, weight: 500, size: 20 }
                            },
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: (data) => data.reduce((t, e) => t + e, 0),
                            unit: "總異常狀態數"
                        },
                        percentage: true,
                        value: true,
                    }
                }
            },
            plugins: [
                chartPlugins.pieBackground,
                chartPlugins.centerText,
                chartPlugins.htmlLegend
            ]
        })
    }
    //空間人員即時位置
    function InspectionCurrentPos(data) {
        const infoBox = document.getElementById('box-InspectionCurrentPos');
        const container = document.getElementById('InspectionCurrentPos');
        const container_another = document.getElementById('InspectionAnotherPos');

        const htmls = createPersons(data.current)
        const htmls_another = createPersons(data.another)
        container.replaceChildren()
        container.insertAdjacentHTML('beforeend', htmls.join(''))

        container_another.replaceChildren()
        container_another.insertAdjacentHTML('beforeend', htmls_another.join(''))

        infoBox.addEventListener('click', (e) => {
            const target = e.target;
            if (target && target.classList.contains('plan-person')) {
                const activePersons = Array.from(document.querySelectorAll('.plan-person.active'))
                activePersons.forEach((el) => {
                    el.classList.remove('active')
                })
                target.classList.add('active')
            }
        })

        function createPersons(data) {
            return data.map((d) => {
                return createPerson(d)
            })

            function createPerson(data) {
                return `<div class="plan-person ${isAlert(data) ? 'error' : ''}">
                    <div class="plan-person-icon"><i class="${getIcon(data)}"></i></div>
                    <div class="plan-person-content">
                        <div class="plan-person-infos">
                            <div class="plan-person-info">
                                <span class="plan-person-name">${data.name}</span>
                                <div class="plan-person-heart">
                                    <i class="${getHeartIcon(data.heartAlert)}"></i>
                                    <span class="value">${data.heart}</span>
                                    <span class="unit">下/分</span>
                                </div>
                                <div class="plan-person-locate">
                                    <i class="fa-solid fa-location-dot"></i>
                                    <span class="value">${data.location}</span>
                                </div>
                            </div>
                            <div class="plan-person-alert">${createAlert(data?.alert)}</div>
                        </div>
                        <div class="plan-person-time">最後更新時間：<span>${data.time}</span></div>
                    </div>
                </div>`

                function getIcon(d) {
                    if (isAlert(d)) {
                        return "fa-solid fa-triangle-exclamation";
                    }
                    return "fa-solid fa-person-walking";
                }

                function getHeartIcon(d) {
                    if (d) {
                        return "fa-solid fa-heart-circle-exclamation";
                    }
                    return "fa-solid fa-heart";
                }
            }

            function isAlert(d) {
                return d?.alert?.length > 0;
            }

            function createAlert(d) {
                if (d?.length === 0) { return ''; }
                return d.map((e) => {
                    const state = getState(e.state);
                    const icon = getIcon(e.type);
                    return `<div class="plan-person-alert-item ${state}">
                        <i class="${icon}"></i>
                        <span class="label">${e.label}</span>
                    </div>`
                }).join('')

                function getState(s) {
                    switch (s) {
                        case "1": return "red";
                        case "2": return "orange";
                        default: return "";
                    }
                }
                function getIcon(s) {
                    switch (s) {
                        case "1": return "fa-solid fa-heart-circle-exclamation";
                        case "2": return "fa-solid fa-shoe-prints";
                        case "3": return "fa-solid fa-user-clock";
                        default: return "";
                    }
                }
            }
        }


    }
    // #endregion
})

function BIM() {
    const self = this
    Autodesk.Viewing.Private.InitParametersSetting.alpha = true; //設定透明背景可用
    const clientContainer = document.getElementById('BIM')
    const profileSettings = {
        name: "customSettings",
        description: "My personal settings.",
        settings: {}, //API:https://aps.autodesk.com/en/docs/viewer/v7/reference/globals/TypeDefs/Settings/
        persistent: [],
        extensions: {
            load: ["Viewer.Toolkit"],
            unload: [/* "Autodesk.Section", "Autodesk.Measure", "Autodesk.Explode" */]
        }
    }
    const options = {
        keepCurrentModels: true,
        globalOffset: { x: 0, y: 0, z: 0 }
    };
    this.viewer = null;
    this.offset = new THREE.Vector3(0, 0, 0);
    this.setup = function setup(ViewName) {
        const type = [
            'AR',
            'E',
            'EL',
            'F',
            'PP',
            'PPO',
            'VE',
            'WW'
        ];
        const urls = type.map((t) => {
            return {
                type: t,
                url: `/BimModels/TopView/${t}/Resource/3D 視圖/${ViewName}/${ViewName}.svf`
            }
        })
        this.viewer = new Autodesk.Viewing.Viewer3D(clientContainer, { profileSettings });
        this.viewer.loadExtension("Viewer.Loading", { loader: `<div class="lds-default">${Array(12).fill('<div></div>').join('')}</div>` })

        return new Promise((resolve, reject) => {
            Autodesk.Viewing.Initializer({ env: "Local" }, async () => {
                this.viewer.start();
                this.viewer.impl.controls.handleKeyDown = function (e) { }
                this.viewer.impl.controls.handleKeyUp = function (e) { }
                await this.viewer.loadExtension("Viewer.Toolkit")

                //load urns
                const models = await Promise.all(urls.map(({ url, type }) => {
                    return new Promise(async (resolve, _) => {
                        this.viewer.loadModel(window.location.origin + url, {
                            ...options,
                            modelOverrideName: type
                        },
                            async (model) => { await this.viewer.waitForLoadDone(); resolve(model); },
                            async (model) => { reject(model); }
                        )
                    })
                }))

                //setting 3d view env
                this.viewer.setBackgroundOpacity(0);
                this.viewer.setBackgroundColor();
                this.viewer.setLightPreset(16); //設定環境光源 = 雪地

                for (const model of models) {
                    await onLoadDone(model)
                }
                this.viewer.loading.hide()
                this.viewer.toolkit.autoFitModelsTop(models.filter(e => e), 10, true)
                resolve(true)
            });
        })

        async function onLoadDone(model) {
            console.log("onLoadDone", model)
            if (model.loader.basePath.includes("Beacon")) {

                const newmat = new THREE.MeshPhongMaterial({
                    color: 0x1010ff,
                    emissive: 0x001061,
                    reflectivity: 0,
                    shininess: 0
                })
                await self.viewer.toolkit.changeMaterial(model, model.getRootId(), newmat)
                let AllBeacons = await self.viewer.toolkit.getAlldbIds(model.getInstanceTree(), model.getRootId())
                console.log("AllBeacons", AllBeacons)
                console.log("顏色變更完畢")
            }
            return
        }
    }
    this.destroy = function destroy() {
        if (this.viewer == null) { return }
        this.viewer.tearDown()
        this.viewer.finish()
        this.viewer = null
        $(clientContainer).empty();
    }
    this.createBeaconPoint = async () => {
        let res = await getDataAsync(`/SamplePath_Management/ReadBimPathDevices/${FSN}`)
        createBeaconPoint(res.BIMDevices)
    }
    this.createSamplePath = createSamplePath
    this.createPathRecord = createPathRecord
    this.hideWall = hideWall

    async function createBeaconPoint(data) {
        console.log(data)
        const model = self.viewer.getAllModels().find((model) => model.loader.basePath.includes("Beacon"));
        const pins = []
        data.forEach((d) => {
            let position = self.viewer.toolkit.getBoundingBox(model, d.dbId).getCenter()
            let pin = new ForgePin({
                viewer: self.viewer,
                position,
                data: {},
                img: "/Content/img/bluetooth.svg",
                id: `bluetooth-${d.dbId}`
            })

            $(pin.e).append(`<div class="bluetooth-name">${d.deviceName}</div>`)

            pin.show()
            pin.update()
            pins.push(pin)
        })
        self.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, () => {
            pins.forEach((pin) => { pin.update() })
        })
    }
    async function createSamplePath() {
        const scene = 'SamplePath'
        const box = self.viewer.getAllModels().reduce((total, model) => {
            total.union(model.getBoundingBox())
            return total
        }, new THREE.Box3())
        const z = box.getCenter().z - (box.getSize().z / 2) + 3
        self.viewer.overlays.addScene(scene)

        const geometry = new THREE.Geometry();
        const material = new THREE.MeshBasicMaterial({ color: 0x29B6F6 })

        const { PathSampleRecord: data } = await getDataAsync(`/jsonSamples/samplePath_test.json`)
        //console.log(data)

        data.forEach(({ LocationX: x, LocationY: y }, i) => {
            let p = createPoint(new THREE.Vector3(x, y, z))
            geometry.mergeMesh(p)
            if (i != 0) {
                let { LocationX: x2, LocationY: y2 } = data[i - 1]
                let eg = createExtrude(
                    new THREE.Vector3(x2, y2, z),
                    new THREE.Vector3(x, y, z),
                )
                geometry.mergeMesh(eg)
            }
        })
        self.viewer.overlays.addMesh(new THREE.Mesh(geometry, material), scene)

        function createExtrude(v1, v2) {
            const radius = 0.1
            const shape = new THREE.Shape()
            shape.absarc(0, 0, radius, 0, Math.PI * 2, false)

            const path = new THREE.LineCurve3(v1, v2)

            const geometry = new THREE.ExtrudeGeometry(shape, {
                steps: 1,
                extrudePath: path
            })
            const mesh = new THREE.Mesh(geometry)
            return mesh
            //self.viewer.overlays.addMesh(mesh, 'SamplePath')
        }

        function createPoint(pos) {
            const geometry = new THREE.SphereGeometry(0.1, 16, 12);
            const mesh = new THREE.Mesh(geometry)
            mesh.position.copy(pos)
            return mesh
            //self.viewer.overlays.addMesh(mesh, 'SamplePath')
        }
    }
    async function createPathRecord() {
        const scene = 'PathRecord'
        self.viewer.overlays.addScene(scene)

        const pin = new ForgePin({
            viewer: self.viewer,
            position: new THREE.Vector3(),
            img: "/Content/img/pin.svg",
            id: "person-" + Math.random().toString(36).slice(2, 7)
        });
        pin.addPopover({
            offset: ['0%', '140%'],
            html: `<div class="pin-popover"><div class="popover-header"><span class="num">Pt01</span><span class="name">王大明</span></div><div class="popover-body"><img src="/Content/img/heart.svg" height="14"><span class="label">心律：</span><span class="value">110</span><span class="unit">下/分</span></div></div>`,
            setContent: function (data) {
                //the function to set html content
                //"this" is popover
                const $e = $(this.e);
                $e.find(".value").text(data.value);
            },
            clickEvent: function (event) {
                //"this" is pin
                !this.popover.isShow()
                    ? (this.popover.show(), this.setZIndex(100))
                    : (this.popover.hide(), this.setZIndex());
            },
        })
        pin.setZIndex(1)
        pin.hide()

        self.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, () => {
            pin.update()
        })

        const material = new THREE.MeshBasicMaterial({ color: 0xD9C832 })
        let count = 0

        const data = await getDataAsync(`/jsonSamples/pathRecord_test.json`)
        const height = 5
        const len = data.length
        let timer = null
        this.start = () => {
            if (timer === null) {
                if (self.viewer.overlays.hasScene(scene)) {
                    self.viewer.overlays.removeScene(scene)
                    self.viewer.overlays.addScene(scene)
                }
                timer = setInterval(() => { animate(data) }, 100)
                pin.show()
                pin.popover.show()
            }
        }
        $("#Play").click(this.start.bind(this))
        return this

        function animate(data) {
            let { x, y, z } = data[count].position
            z = z - height
            let p = createPoint(new THREE.Vector3(x, y, z), material)
            self.viewer.overlays.addMesh(p, scene)

            pin.position.set(x, y, z)
            pin.show()

            if (count != 0) {
                let { x: x2, y: y2, z: z2 } = data[count - 1].position
                z2 = z2 - height
                let eg = createExtrude(
                    new THREE.Vector3(x2, y2, z2),
                    new THREE.Vector3(x, y, z),
                    material
                )
                self.viewer.overlays.addMesh(eg, scene)
            }
            //self.viewer.impl.sceneUpdated(true)
            if (count <= len - 2) {
                count++
                console.log('animate running')
            }
            else {
                console.log('animate end')
                count = 0
                clearInterval(timer)
                timer = null
            }
        }

        function createExtrude(v1, v2, mat) {
            const radius = 0.1
            const shape = new THREE.Shape()
            shape.absarc(0, 0, radius, 0, Math.PI * 2, false)

            const path = new THREE.LineCurve3(v1, v2)

            const geometry = new THREE.ExtrudeGeometry(shape, {
                steps: 1,
                extrudePath: path
            })
            const mesh = new THREE.Mesh(geometry, mat)
            return mesh
            //self.viewer.overlays.addMesh(mesh, 'SamplePath')
        }

        function createPoint(pos, mat) {
            const geometry = new THREE.SphereGeometry(0.1, 16, 12);
            const mesh = new THREE.Mesh(geometry, mat)
            mesh.position.copy(pos)
            return mesh
            //self.viewer.overlays.addMesh(mesh, 'SamplePath')
        }
    }
    async function hideWall() {
        const box = self.viewer.getAllModels().reduce((total, model) => {
            total.union(model.getBoundingBox())
            return total
        }, new THREE.Box3())
        const z = box.getCenter().z + (box.getSize().z / 2)
        const intersectBox = new THREE.Box3(
            new THREE.Vector3().copy(box.min).setZ(z - 1),
            new THREE.Vector3().copy(box.max).setZ(z + 1)
        )

        const allDbids = await new Promise(async (resolve) => {
            const arr = []
            const models = self.viewer.getAllModels()
            for (const model of models) {
                let dbIds = await self.viewer.toolkit.getAlldbIds(model.getInstanceTree(), model.getRootId())
                arr.push({ model, dbIds })
            }
            resolve(arr)
        })

        allDbids.forEach(({ model, dbIds }) => {
            dbIds.forEach((dbId) => {
                let bbox = self.viewer.toolkit.getBoundingBox(model, dbId)
                if (intersectBox.isIntersectionBox(bbox)) {
                    self.viewer.hide(dbId)
                }
            })
        })

        self.viewer.search("M_混凝土", (res) => {
            res.forEach((dbId) => { self.viewer.hide(dbId) })
        })
    }
    function getDataAsync(url) {
        return new Promise((success, error) => {
            $.ajax({
                url: url,
                type: "GET",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success,
                error
            })
        })
    }

    return this
}

//目前位置
function CurrentLocation() {
    const menu = document.querySelector(".current-location-menu")
    const text = document.getElementById("CurrentLocation")
    this.activeIndex = 0;
    this.value;
    this.data = [];
    this.init = async () => {
        this.data = await $.getJSON("/DropDownList/ViewName")
        menu.replaceChildren();
        const htmls = this.data.map((e, i) => {
            const active = this.activeIndex === i ? 'active' : '';
            return `<li><a class="dropdown-item ${active}" data-index="${i}">${e.Text}</a></li>`
        })
        menu.insertAdjacentHTML('beforeend', htmls.join(''));
        this.updateData()

        menu.addEventListener('click', (e) => {
            const target = e?.target;
            if (target && target.classList.contains('dropdown-item')) {
                const index = parseInt(target.dataset.index);
                if (this.activeIndex !== index) {
                    this.activeIndex = index;
                    this.updateData()
                }
            }
        })
    }
    this.updateData = () => {
        const d = this.data?.[this.activeIndex];
        if (!d) return;
        text.textContent = d.Text;
        this.value = d.Value;
        const items = Array.from(menu.querySelectorAll(".dropdown-item"))
        items.forEach((e) => {
            if (parseInt(e.dataset.index) === this.activeIndex) {
                e.classList.add('active')
                return
            }
            e.classList.remove('active')
        })
        console.log(items);

        menu.dispatchEvent(new Event('change'))
    }
    this.addEventListener = (eventType, callback) => {
        menu.addEventListener(eventType, callback.bind(this, this))
    }

    return this
}