const chartCreator = new ChartCreator()

window.addEventListener('load', async () => {
    // #region GIS function
    const { map, view, i3sLayer, widgets, FactoryLayer } = await GIS()
    async function GIS() {
        await loadArcgis([
            "esri/config",
            "esri/Map",
            "esri/Basemap",
            "esri/WebScene",
            "esri/views/SceneView",
            "esri/layers/SceneLayer",
            "esri/layers/FeatureLayer",
            "esri/layers/GroupLayer",
            "esri/core/reactiveUtils",
            "esri/renderers/UniqueValueRenderer",
            "esri/rest/support/Query",
            "esri/Graphic",
            "esri/geometry/Point",
            "esri/geometry/support/geodesicUtils",
        ])
        Arcgis.config.portalUrl = "https://arcgisbim.bimfm.com.tw/portal";
        const map = await initMap()
        const view = await initView(map)
        const i3sLayer = await init_i3sLayer()
        const widgets = await initWidgets()
        await init_FactoriesMarker()
        const FactoryLayer = await init_FactoryLayer()

        return {
            map,
            view,
            i3sLayer,
            widgets,
            FactoryLayer
        }

        // #region init function
        async function initMap() {
            //地面圖層
            const map = new Arcgis.Map({
                basemap: new Arcgis.Basemap({ portalItem: { id: "a1a38b810a4547a4b52cacf7ababebe2" } }),
                ground: "world-elevation"
            });
            map.addHandles(Arcgis.reactiveUtils.when(
                () => map.ground.loaded === true,
                () => {
                    //可將視角轉到地面下的設定
                    map.ground.navigationConstraint = { type: "none" };
                },
                { once: true }
            ))
            return map
        }

        async function initView(map) {
            const view = new Arcgis.SceneView({
                map,
                container: "GIS",
                camera: {
                    position: {
                        longitude: 120.90344547484935,
                        latitude: 23.65463255995128,
                        z: 909138.2457838273
                    },
                    heading: 0.12155189235580274,
                    tilt: 0.4377931440911283
                },
                qualityProfile: "high",
                environment: {
                    atmosphere: {
                        quality: "high"
                    },
                },
                constraints: {
                    altitude: {
                        max: 909138.2457838273
                    }
                },
                popup: {
                    dockOptions: {
                        buttonEnabled: true
                    }
                },
                popupEnabled: false
            });

            view.when(() => {
                //顯示點擊位置經緯度
                view.on('click', async (event) => {
                    console.log(event.mapPoint.longitude, event.mapPoint.latitude);
                    console.log(view.camera);

                    const hit = await view.hitTest(event)
                    console.log(hit.results);
                    const attr = hit?.results?.[0]?.graphic?.attributes
                    if (!attr) { return }
                    const Factory = attr?.Factory
                    const Area = attr?.Area
                    const Floor = attr?.Floor

                    if (Floor) {
                        changeFloor(Factory, Area, Floor)
                    }
                    else if (Area) {
                        changeArea(Factory, Area)
                    }
                    else if (Factory) {
                        changeFactory(Factory)
                    }
                })

                //取消視角自由縮放拖移設定
                /* view.navigation.mouseWheelZoomEnabled = false;
                let dragHandler = view.on("drag", function (event) {
                    event.stopPropagation()
                }); */

                //view error
                view.watch("fatalError", function (error) {
                    if (error) {
                        console.error("Fatal Error! View has lost its WebGL context. Attempting to recover...");
                        view.tryFatalErrorRecovery();
                    }
                });
            })
            return view
        }

        async function initWidgets() {
            const widgets = {
                homeWidget: await homeWidget(view),
                fullscreenWidget: await fullscreenWidget(view),
                basemapGalleryWidget: await basemapGalleryWidget(view),
                daylightWidget: await daylightWidget(view),
                elevationProfileWidget: await elevationProfileWidget(view),
                measureExpandWidget: await measureExpandWidget(view),
                settingWidget: await settingWidget(view, { i3sLayer }),
            }
            //console.log(widgets)

            widgets.settingWidget.setSettingValue("map-ground-opacity", 1)

            view.ui.move([
                "basemapGallery",
                "daylight",
                "measure",
                "elevationProfile",
                "setting",

                "home",
                "zoom",
                "navigation-toggle",
                "fullscreen",
                "compass",
            ], "bottom-left");
            return widgets
        }

        async function init_i3sLayer() {
            //國土計畫圖層
            const i3sLayer = new Arcgis.SceneLayer({
                url: "https://i3s.nlsc.gov.tw/building/i3s/SceneServer/layers/0",
                title: "nopopup",
                listMode: "hide",
                //visible: false,
                elevationInfo: {
                    offset: -4
                },
                renderer: {
                    type: "simple",
                    symbol: {
                        type: "mesh-3d",
                        symbolLayers: [{
                            type: "fill",  // autocasts as new FillSymbol3DLayer()
                            material: {
                                color: [255, 255, 255, 1],
                                colorMixMode: 'replace'
                            }
                        }]
                    }
                }
            })
            map.add(i3sLayer)
            return i3sLayer
        }

        async function init_FactoriesMarker() {
            const data = [
                {
                    Factory: "民生廠",
                    Longitude: 121.56762073267188,
                    Latitude: 25.064001296355343
                },
                {
                    Factory: "新竹廠",
                    Longitude: 120.99905786800876,
                    Latitude: 24.866641522580533
                },
                {
                    Factory: "大里廠",
                    Longitude: 120.7123751260719,
                    Latitude: 24.102027970057133
                }
            ]
            const layer = new Arcgis.FeatureLayer({
                title: "Factories",
                fields: [
                    { name: "objectId", alias: "objectId", type: "oid" },
                    { name: "Factory", alias: "廠區", type: "string" },
                    { name: "Longitude", alias: "經度", type: "double" },
                    { name: "Latitude", alias: "緯度", type: "double" },
                ],
                objectIdField: "objectId",
                outFields: ["*"],
                source: data.map((d) => ({ geometry: { type: "point", x: d.Longitude, y: d.Latitude }, attributes: d })),
                elevationInfo: {
                    mode: "relative-to-scene",
                    offset: 0
                },
                maxScale: 5000,
                renderer: {
                    type: "simple",
                    symbol: {
                        type: "point-3d",
                        symbolLayers: [{
                            type: "icon",
                            size: 42,
                            resource: { href: "/Content/img/gis-marker.png" },
                            material: { color: "red" }
                        }]
                    }
                }
            });

            map.add(layer);
            return layer
        }

        async function init_FactoryLayer() {
            //加入建築
            const scene = new Arcgis.WebScene({ portalItem: { id: "46c0a281b7a64168914d44c50d074ef0" } })
            const layer = new Arcgis.GroupLayer({
                title: "FactoryLayer",
                visible: false,
                minScale: 10000,
            });
            const data = [
                {
                    Factory: "民生廠",
                    Area: "進流抽水站",
                    Longitude: 121.56775499499734,
                    Latitude: 25.06434939672518
                },
                {
                    Factory: "民生廠",
                    Area: "前處理機房",
                    Longitude: 121.56750306461882,
                    Latitude: 25.064317229187477
                },
                {
                    Factory: "民生廠",
                    Area: "生物處理機房",
                    Longitude: 121.56747206810206,
                    Latitude: 25.063957226641904
                },
                {
                    Factory: "民生廠",
                    Area: "環境教育館",
                    Longitude: 121.56781598219244,
                    Latitude: 25.064022709699742
                }
            ]
            const labelClass = (Area, color) => ({
                where: `Area = '${Area}'`,
                labelPlacement: "above-center",
                labelExpressionInfo: { expression: "$feature.Area" },
                symbol: {
                    type: "label-3d",
                    symbolLayers: [{
                        type: "text",
                        size: 12,
                        font: { family: "Noto Sans TC, sans-serif", weight: "bold" },
                        material: { color: "#fff" },
                        background: { color }
                    }],
                    verticalOffset: {
                        screenLength: 12
                    },
                    callout: {
                        type: "line",
                        size: 2,
                        color: "#fff"
                    }
                }
            })
            const uniqueValueInfo = (value, color) => ({
                value,
                symbol: {
                    type: "point-3d",
                    symbolLayers: [{
                        type: "icon",
                        size: 16,
                        material: { color },
                        outline: { color: "#ffffff", width: 4 }
                    }]
                }
            })
            const AreaLayer = new Arcgis.FeatureLayer({
                title: "Areas",
                fields: [
                    { name: "objectId", alias: "objectId", type: "oid" },
                    { name: "Factory", alias: "廠區", type: "string" },
                    { name: "Area", alias: "棟別", type: "string" },
                    { name: "Longitude", alias: "經度", type: "double" },
                    { name: "Latitude", alias: "緯度", type: "double" },
                ],
                objectIdField: "objectId",
                outFields: ["*"],
                source: data.map((d) => ({ geometry: { type: "point", x: d.Longitude, y: d.Latitude }, attributes: d })),
                elevationInfo: {
                    mode: "relative-to-scene",
                    offset: 1
                },
                labelingInfo: [
                    labelClass("進流抽水站", "#EA4E44"),
                    labelClass("前處理機房", "#F6A454"),
                    labelClass("生物處理機房", "#4F74D2"),
                    labelClass("環境教育館", "#29CC84"),
                ],
                renderer: {
                    type: "unique-value",
                    field: "Area",
                    defaultSymbol: { type: "point-3d" },
                    uniqueValueInfos: [
                        uniqueValueInfo("進流抽水站", "#F97171"),
                        uniqueValueInfo("前處理機房", "#FFC070"),
                        uniqueValueInfo("生物處理機房", "#8399DB"),
                        uniqueValueInfo("環境教育館", "#6DE8AF"),
                    ]
                },
            });

            await scene.load()

            layer.layers.push(scene.layers.items[0], AreaLayer);

            map.add(layer)

            return layer
        }
        // #endregion
    }
    // #endregion

    // #region BIM function
    async function BIM() {
        
    }
    // #endregion

    // #region header select method
    const boxList = {
        "Home": [
            "Sewage",
            "Electricity_Usage_Information",
            "Environment",
            "Inspection_Current_Status",
            "Inspection_Current_Pos",
            "Inspection_Aberrant",
            "Equipment_Availability_Rate",
        ],
        "Factory": [
            "Sewage",
            "Electricity_Usage_Information",
            "Environment",
            "Equipment_Operating_State",
            "Inspection_Aberrant",
            "Equipment_Availability_Rate",
            "Equipment_Level_Rate",
        ],
        "Area": [

        ],
        "Floor": [

        ]
    }
    setup()
    async function setup() {
        const Factory = $(`#Factory`);
        const Area = $(`#Area`);
        const Floor = $(`#Floor`);
        pushSelect(`Area`, "/DropDownList/Area", "請選擇棟別", "Text", "Text");

        Area.on("change", async function () {
            await pushSelect(`Floor`, "/DropDownList/Floor" + `?ASN=${Area.val()}`, "請選擇樓層", "Text", "Text");
            Floor.val('');
        });

        $("#header-search-btn").on('click', function () {
            const vals = [
                Factory.val(),
                Area.val(),
                Floor.val()
            ]
            if (vals[0] != '' && vals[1] != '' && vals[2] != '') {
                changeFloor(...vals)
            }
            if (vals[0] != '' && vals[1] != '') {
                changeArea(...vals)
            }
            if (vals[0] != '') {
                changeFactory(...vals)
            }
            else {
                goHome()
            }
        })

        $(".info-area[data-mode='home']").each((i, e) => {
            $(e).removeClass("d-none")
        })

        goHome()

        async function pushSelect(selectId, jsonUrl, defaultText = "請選擇", name = "Text", value = "Value") {
            const $select = $("#" + selectId);
            await $.getJSON(jsonUrl, function (data) {
                $select.empty();
                $select.append('<option value="">' + defaultText + '</option>');
                $.each(data, function (i, e) {
                    $select.append('<option value="' + e[value] + '">' + e[name] + '</option>')
                })
            });
        }
    }
    function goHome() {
        changeLocation()
        widgets.homeWidget.go()
    }
    function changeFactory(Factory) {
        if (Factory !== "民生廠") { return }
        changeLocation("Factory", { Factory })

        view.goTo({
            position: {
                latitude: 25.065488306537205,
                longitude: 121.56885233618826,
                z: 186.21390903182328
            },
            heading: 217.381934034877,
            tilt: 48.18596147373148
        })
        FactoryLayer.visible = true
    }
    function changeArea(Factory, Area) {
        if (Area !== "進流抽水站") { return }
        changeLocation("Area", { Factory, Area })
    }
    function changeFloor(Factory, Area, Floor) {
        changeLocation("Floor", { Factory, Area, Floor })
    }
    function changeLocation(mode = "Home", { Factory = '', Area = '', Floor = '' } = {}) {
        console.log("changeLocation", mode, Factory, Area, Floor)
        if (Factory + Area + Floor == '') {
            $("#Current_Location").addClass("d-none")
        }
        else {
            $("#Current_Location").removeClass("d-none")
        }
        $("#Current_Factory").text(Factory)
        $("#Current_Area").text(Area)
        $("#Current_Floor").text(Floor)

        $("#box-Environment").attr("data-mode", mode)

        $("#Factory").val(Factory)
        $("#Area").val(Area)
        $("#Floor").val(Floor)

        switch (true) {
            case Floor !== '':
            case Area !== '':
                $("#GIS").addClass("d-none")
                $("#BIM").removeClass("d-none")
                break;
            case Factory !== '':
            default:
                $("#GIS").removeClass("d-none")
                $("#BIM").addClass("d-none")
                break;
        }

        $(".info-box").each((i, e) => {
            $(e).addClass("d-none")
        })
        boxList?.[mode]?.forEach((e) => {
            $("#box-" + e).removeClass("d-none")
            eval(`chartCreator?.${e}?.()`)
        })
    }
    // #endregion
})

function ChartCreator() {
    // #region chart options
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
    const axisOpt = {
        ticks: {
            color: "#1D3156",
            font: { family, size: 16 }
        },
        border: {
            color: "transparent",
            dash: [4, 4]
        },
        grid: {
            color: "#696969",
            tickWidth: 0
        }
    }
    // #endregion

    // #region chart create function
    //當日汙水處理量
    this.Sewage = function () {
        const progress = document.querySelector('#Sewage_Progress [role="progressbar"]')
        progress.ariaValueNow = 25
        progress.style.width = "25%"
    }
    //用電資訊
    this.Electricity_Usage_Information = function () {
        const container = document.getElementById('Electricity_Usage_Information');
        const ctx = getOrCreateElement(container, 'canvas')

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        const mode = $("#box-Environment").attr("data-mode")
        const data = {
            "Home": [
                { label: "RO單元處理", value: 1200 },
                { label: "汙泥貯槽", value: 1900 },
                { label: "放流水系統", value: 300 },
                { label: "回收水系統", value: 500 },
                { label: "膜率處理", value: 200 },
                { label: "生物處理", value: 410 },
                { label: "前處理", value: 400 },
            ],
            "Factory": [
                { label: "進流抽水站", value: 522 },
                { label: "前處理機房", value: 360 },
                { label: "生物處理機房", value: 410 },
                { label: "環境教育館", value: 259 },
            ],
            "Area": [
                { label: "1F", value: 10 },
                { label: "2F", value: 12 },
                { label: "B1F", value: 247 },
                { label: "B2F", value: 253 },
            ]
        }
        switch (mode) {
            case "Home":
                ctx.width = 360
                ctx.height = 265
                chart = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: data[mode].map(x => x.label),
                        datasets: [{
                            label: '用電資訊',
                            borderWidth: 0,
                            backgroundColor: "#FFCE56",
                            barThickness: 16,
                            data: data[mode].map(x => x.value)
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: false,
                        scales: { x: axisOpt, y: axisOpt },
                        plugins: { legend, tooltip }
                    }
                })
                break;
            case "Factory":
                ctx.width = 360
                ctx.height = 265
                chart = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: data[mode].map(x => x.label),
                        datasets: [{
                            label: '用電資訊',
                            borderWidth: 0,
                            backgroundColor: ["#F97171", "#FFC070", "#8399DB", "#6DE8AF"],
                            barThickness: 16,
                            data: data[mode].map(x => x.value)
                        }]
                    },
                    options: {
                        responsive: false,
                        scales: {
                            x: {
                                ticks: {
                                    color: "#1D3156",
                                    font: { family, size: 12 }
                                },
                                border: {
                                    color: "transparent",
                                    dash: [4, 4]
                                },
                                grid: {
                                    color: "#696969",
                                    tickWidth: 0
                                }
                            },
                            y: axisOpt
                        },
                        plugins: { legend, tooltip }
                    }
                })
                break;
            case "Area":
                ctx.width = 360
                ctx.height = 265
                chart = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: data[mode].map(x => x.label),
                        datasets: [{
                            label: '用電資訊',
                            borderWidth: 0,
                            backgroundColor: "#FFCE56",
                            barThickness: 16,
                            data: data[mode].map(x => x.value)
                        }]
                    },
                    options: {
                        responsive: false,
                        scales: { x: axisOpt, y: axisOpt },
                        plugins: { legend, tooltip }
                    }
                })
                break;
        }
    }
    //巡檢異常狀態
    this.Inspection_Aberrant = function () {
        this.Inspection_Aberrant_Level()
        this.Inspection_Aberrant_Resolve()
    }
    //巡檢異常狀態 等級占比
    this.Inspection_Aberrant_Level = function () {
        const container = document.getElementById('Inspection_Aberrant_Level');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#4269AC", "#E77272"]
        const data = [
            { label: "軌跡偏離", value: 15 },
            { label: "緊急", value: 15 }
        ]
        ctx.width = 138
        ctx.height = 138

        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.datasets[0].data = data.map(x => x.value)
            chart.update()
            return
        }

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備總妥善率',
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
    this.Inspection_Aberrant_Resolve = function () {
        const container = document.getElementById('Inspection_Aberrant_Resolve');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#4269AC", "#E77272"]
        const data = [
            { label: "已處理", value: 53 },
            { label: "處理中", value: 15 },
            { label: "待處理", value: 15 }
        ]
        ctx.width = 138
        ctx.height = 138

        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.datasets[0].data = data.map(x => x.value)
            chart.update()
            return
        }

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備總妥善率',
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
                                    let value = data.find(x => x.label == "已處理").value + data.find(x => x.label == "處理中").value
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

    //設備總妥善率
    this.Equipment_Availability_Rate = function () {
        const container = document.getElementById('Equipment_Availability_Rate');
        const ctx = getOrCreateElement(container, 'canvas')

        const backgroundColor = ["#72E998", "#E9CD68", "#E77272"]
        const data = [
            { label: "正常", value: 15 },
            { label: "維修中", value: 7 },
            { label: "異常", value: 3 },
        ]
        ctx.width = 138
        ctx.height = 138

        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.datasets[0].data = data.map(x => x.value)
            chart.update()
            return
        }

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備總妥善率',
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
                        inline: true,
                        text: [
                            {
                                string: (() => {
                                    let total = data.reduce((t, x) => t + x.value, 0)
                                    let value = data.find(x => x.label == "正常").value
                                    return Math.floor(value / total * 100)
                                })(),
                                color: "#000", font: { family, weight: 500, size: 32 }
                            },
                            {
                                string: "%", color: "#000", font: { family, weight: 500, size: 14 }
                            }
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: data.find(x => x.label == "異常").value,
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

    //故障設備等級分布
    this.Equipment_Level_Rate = function () {
        const container = document.getElementById('Equipment_Level_Rate');
        const ctx = getOrCreateElement(container, 'canvas')

        const backgroundColor = ["#72E998", "#E9CD68", "#E77272"]
        const data = [
            { label: "一般", value: 15 },
            { label: "緊急", value: 7 },
            { label: "最速件", value: 3 },
        ]
        ctx.width = 138
        ctx.height = 138

        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.datasets[0].data = data.map(x => x.value)
            chart.update()
            console.log(chart)
            return
        }

        chart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備故障等級分布',
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
                        inline: true,
                        text: [{
                            string: "等級分布", color: "#000", font: { family, weight: 500, size: 14 }
                        }]
                    },
                    htmlLegend: {
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
    //設備運轉狀態
    this.Equipment_Operating_State = function () {
        const container = document.getElementById('Equipment_Operating_State');
    }
    // #endregion
    return this;
}