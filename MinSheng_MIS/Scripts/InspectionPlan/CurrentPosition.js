const bim = new BIM()
const Floor = "B2F"
const FSN = "1_1"
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
    const shadowPlugin = {
        color: 'rgba(0,0,0,0.25)',
        blur: 4,
        offset: { x: 0, y: 4 }
    }

    // #endregion

    // #region chart
    Equipment_State()
    Inspection_Complete_State()
    Inspection_Aberrant_Level()
    Inspection_Aberrant_Resolve()
    await bim.setup([
        `/BimModels/01/Resource/3D View/進抽站${Floor}/進抽站${Floor}.svf`,
        `/BimModels/02/Resource/3D View/進抽站${Floor}_Beacon/進抽站${Floor}_Beacon.svf`
    ])
    bim.hideWall()
    bim.createBeaconPoint()
    bim.createSamplePath()
    const pathRecord = await bim.createPathRecord()
    const keyMng = bim.viewer.getHotkeyManager()
    const handleKeyDown = keyMng.handleKeyDown
    keyMng.handleKeyDown = function (e) {
        handleKeyDown(e)
        console.log("keydown", e.code)
        if (e.code === 'Space') {
            pathRecord.start()
        }
    }
    console.log("%c請按空白鍵開始動畫","color:green;font-size: 2em;padding:0.5rem;")

    // #endregion

    // #region chart function
    //報修狀況
    function Equipment_State() {
        const container = document.getElementById('Equipment_State');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        const data = [
            { label: "報修", value: 10 },
            { label: "維修中", value: 16 },
            { label: "保養中", value: 15 },
        ]
        ctx.width = pieSize
        ctx.height = pieSize
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '報修狀況',
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
                                string: (() => {
                                    let total = data.reduce((t, e) => t + e.value, 0)
                                    let value = data.find(x => x.label == "報修").value
                                    return (Math.floor(value / total * 1000) / 10) + "%"
                                })(),
                                color: "#000",
                                font: { family, weight: 500, size: 20 }
                            }
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: data.reduce((t, e) => t + e.value, 0),
                            unit: "總計畫數"
                        },
                        percentage: true
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
    //本日巡檢計畫進度
    function Inspection_Complete_State() {
        const container = document.getElementById('Inspection_Complete_State');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        const data = [
            { label: "已完成", value: 10 },
            { label: "執行中", value: 16 },
            { label: "待執行", value: 15 },
        ]
        ctx.width = pieSize
        ctx.height = pieSize
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '本日巡檢計畫進度',
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
                                string: (() => {
                                    let total = data.reduce((t, e) => t + e.value, 0)
                                    let value = data.find(x => x.label == "已完成").value
                                    return (Math.floor(value / total * 1000) / 10) + "%"
                                })(),
                                color: "#000",
                                font: { family, weight: 500, size: 20 }
                            }
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: data.reduce((t, e) => t + e.value, 0),
                            unit: "總計畫數"
                        },
                        percentage: true
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
    //巡檢異常狀態 等級占比
    function Inspection_Aberrant_Level() {
        const container = document.getElementById('Inspection_Aberrant_Level');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#2CB6F0", "#E77272"]
        const data = [
            { label: "一般", value: 20 },
            { label: "緊急", value: 15 }
        ]
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
                            value: data.reduce((t, e) => t + e.value, 0),
                            unit: "總異常狀態數"
                        },
                        percentage: true
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
    function Inspection_Aberrant_Resolve() {
        const container = document.getElementById('Inspection_Aberrant_Resolve');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#4269AC", "#E77272"]
        const data = [
            { label: "已處理", value: 53 },
            { label: "未處理", value: 15 }
        ]
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
                                string: (() => {
                                    let total = data.reduce((t, e) => t + e.value, 0)
                                    let value = data.find(x => x.label == "已處理").value
                                    return (Math.floor(value / total * 1000) / 10) + "%"
                                })(),
                                color: "#E77272",
                                font: { family, weight: 500, size: 20 }
                            },
                            {
                                string: "處理狀況",
                                color: "#000",
                                font: { family, weight: 500, size: 12, lineHeight: 1.25 }
                            }
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: data.reduce((t, e) => t + e.value, 0),
                            unit: "總異常狀態數"
                        },
                        percentage: true
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
    this.setup = function setup(urls) {
        this.viewer = new Autodesk.Viewing.GuiViewer3D(clientContainer, { profileSettings });
        this.viewer.loadExtension("Viewer.Loading", { loader: `<div class="lds-default">${Array(12).fill('<div></div>').join('')}</div>` })

        return new Promise((resolve, reject) => {
            Autodesk.Viewing.Initializer({ env: "Local" }, async () => {
                this.viewer.start();
                await this.viewer.loadExtension("Viewer.Toolkit")

                //load urns
                const models = await Promise.all(urls.map((url) => {
                    return new Promise(async (resolve, _) => {
                        this.viewer.loadModel(window.location.origin + url, options,
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