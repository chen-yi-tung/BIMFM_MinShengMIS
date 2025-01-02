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
const bim = new UpViewer(document.getElementById('BIM'))
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
        await bim.init()
        await bim.loadModels(bim.getModelsUrl(ViewName))

        //bim.hideWall()
        bim.createBeaconPoint([
            {
                dbId: 20132,
                GUID: 'f39fb506-457a-40e9-b4aa-3c6702d4fc1c-00389a20',
                ElementID: 3709472,
                deviceName: "BT-20132",
            },
            {
                dbId: 20133,
                GUID: 'f39fb506-457a-40e9-b4aa-3c6702d4fc1c-00389f14',
                ElementID: 3710740,
                deviceName: "BT-20133",
            },
            {
                dbId: 20134,
                GUID: 'f39fb506-457a-40e9-b4aa-3c6702d4fc1c-00389f2e',
                ElementID: 3710766,
                deviceName: "BT-20134",
            },
            {
                dbId: 20142,
                GUID: 'f39fb506-457a-40e9-b4aa-3c6702d4fc1c-00389f67',
                ElementID: 3710823,
                deviceName: "BT-20142",
            },
        ])

        //bim.activateEquipmentPointTool(new THREE.Vector3(0, 0, 0), true);

        /* bim.createSamplePath()
        const pathRecord = await bim.createPathRecord()
        document.body.addEventListener('keydown', (e) => {
            console.log("keydown", e.code)
            if (e.code === 'Space') {
                pathRecord.start()
            }
        })
        console.log("%c請按空白鍵開始動畫", "color:green;font-size: 2em;padding:0.5rem;") */

        currentLocation.addEventListener('change', async (e) => {
            bim.dispose()
            await bim.init()
            await bim.loadModels(bim.getModelsUrl(currentLocation.value))
            //bim.activateEquipmentPointTool(new THREE.Vector3(0, 0, 0), true);
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