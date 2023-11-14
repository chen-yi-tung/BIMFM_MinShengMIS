window.addEventListener('load', async () => {
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
    const chartOpt_Bar = (data) => {
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
        return {
            data, type: 'bar', options: {
                indexAxis: 'y',
                responsive: false,
                scales: { x: axisOpt, y: axisOpt },
                plugins: { legend, tooltip }
            }
        }
    }

    // #endregion

    // #region chart create
    Sewage_Progress()
    Electricity_Usage_Information()

    Inspection_Aberrant_Level()
    Inspection_Aberrant_Resolve()

    Equipment_Availability_Rate()
    // #endregion

    // #region chart create function
    //當日汙水處理量
    function Sewage_Progress() {
        const progress = document.querySelector('#Sewage_Progress [role="progressbar"]')
        progress.ariaValueNow = 25
        progress.style.width = "25%"
    }
    //用電資訊
    function Electricity_Usage_Information() {
        const container = document.getElementById('Electricity_Usage_Information');
        const ctx = getOrCreateElement(container, 'canvas')

        const data = [
            { label: "RO單元處理", value: 12 },
            { label: "汙泥貯槽", value: 19 },
            { label: "放流水系統", value: 3 },
            { label: "回收水系統", value: 5 },
            { label: "膜率處理", value: 2 },
            { label: "生物處理", value: 3 },
            { label: "前處理", value: 1 },
        ]
        ctx.width = 357
        ctx.height = 265
        new Chart(ctx, chartOpt_Bar({
            labels: data.map(x => x.label),
            datasets: [{
                label: '用電資訊',
                borderWidth: 0,
                backgroundColor: "#FFCE56",
                barPercentage: 0.5,
                categoryPercentage: 1,
                data: data.map(x => x.value)
            }]
        }))
    }

    //巡檢異常狀態 等級占比
    function Inspection_Aberrant_Level() {
        const container = document.getElementById('Inspection_Aberrant_Level');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#4269AC", "#E77272"]
        const data = [
            { label: "軌跡偏離", value: 15 },
            { label: "緊急", value: 15 }
        ]
        ctx.width = 138
        ctx.height = 138
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
    function Inspection_Aberrant_Resolve() {
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
    function Equipment_Availability_Rate() {
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
    // #endregion

    // #region GIS function
    const { map, view, i3sLayer, widgets } = await GIS()
    async function GIS() {
        await loadArcgis([
            "esri/config",
            "esri/Map",
            "esri/Basemap",
            "esri/WebScene",
            "esri/views/SceneView",
            "esri/layers/SceneLayer",
            "esri/layers/FeatureLayer",
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
        await init_Marker()
        await init_Building()

        return {
            map,
            view,
            i3sLayer,
            widgets
        }

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
            });

            view.when(() => {
                //顯示點擊位置經緯度
                view.on('click', async (event) => {
                    //console.log(event.mapPoint.longitude, event.mapPoint.latitude);
                    console.log(view.camera);

                    const hit = await view.hitTest(event)
                    //console.log(hit);
                    if (hit.results?.[0]?.graphic?.attributes?.Factory) {
                        const Factory = hit.results[0].graphic.attributes.Factory
                        console.log(Factory)
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

        async function init_Marker() {
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
                title: "nopopup",
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
        }

        async function init_Building() {
            //加入建築
            const scene = new Arcgis.WebScene({ portalItem: { id: "46c0a281b7a64168914d44c50d074ef0" } })
            scene.load().then(() => {
                scene.layers.forEach((layer) => {
                    map.add(layer)
                })
            })
        }
    }
    // #endregion

    addAreaFloorEvent()
    async function addAreaFloorEvent() {
        const Factory = $(`#Factory`);
        const Area = $(`#Area`);
        const Floor = $(`#Floor`);
        pushSelect(`Area`, "/DropDownList/Area", "請選擇棟別");

        Area.on("change", async function () {
            await pushSelect(`Floor`, "/DropDownList/Floor" + `?ASN=${Area.val()}`, "請選擇樓層");
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
                widgets.homeWidget.go()
            }
        })

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
    function changeFactory(Factory) {
        if (Factory !== "民生廠") { return }

        const boxList = [
            "Sewage",
            "Electricity_Usage_Information",
            "Environment",
            "Inspection_Current_Status",
            "Inspection_Current_Pos",
            "Inspection_Aberrant",
            "Equipment_Availability_Rate",
        ]


        $("#Factory").val(Factory)
        view.goTo({
            position: {
                latitude: 25.065488306537205,
                longitude: 121.56885233618826,
                z: 186.21390903182328
            },
            heading: 217.381934034877,
            tilt: 48.18596147373148
        })
    }
    function changeArea(Factory, Area) {
        console.log(Factory, Area)
    }
    function changeFloor(Factory, Area, Floor) {
        console.log(Factory, Area, Floor)
    }
})