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
                time: '2024-12-09 15:19',
                alert: [],
                position: {
                    x: 0,
                    y: 0
                },
            },
            {
                name: '王曉明',
                heart: 110,
                heartAlert: false,
                location: '進流抽水站 B2F',
                time: '2024-12-09 15:19',
                alert: [],
                position: {
                    x: 42,
                    y: 12
                },
            },
        ],
        another: [
            {
                name: '易施詩',
                heart: 110,
                heartAlert: true,
                location: '前處理機房 渠道操作層',
                time: '2024-12-09 15:19',
                alert: [
                    { state: '1', type: '1', label: '心率異常' },
                    { state: '1', type: '2', label: '路線偏移' },
                    { state: '2', type: '3', label: '停留過久' },
                    { state: '2', type: '4', label: '緊急按鈕' },
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
                time: '2024-12-09 15:19',
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
const currentLocation = new CurrentLocation()
const alertCollapse = new AlertCollapse()

const InspectionCurrentPos_Data = { current: [], another: [] };
const InspectionCurrentPos_Pins = [];
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
    const emptyDoughnut = {
        color: 'rgba(0,0,0,0.3)',
        borderColor: 'rgba(0,0,0,0.2)',
        font: {
            family: 'Noto Sans TC',
            size: 16,
        },
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
    const updateTime = 1000;
    let timer;
    init()

    async function getData(useFake = false) {
        const res = await $.ajax({
            url: "/InspectionPlan_Management/GetCurrentPositionData",
            data: { FSN: currentLocation.FSN },
            type: "GET",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
        })
        // console.log("getData", res);
        // use fakeData
        if (useFake) {
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
        }
        return res
    }
    async function init() {
        const [res, ,] = await Promise.all([
            getData(),
            currentLocation.init(),
            alertCollapse.init(),
            bim.init()
        ])

        ChartInspectionEquipmentState(res?.ChartInspectionEquipmentState)
        ChartInspectionCompleteState(res?.ChartInspectionCompleteState)
        EquipmentOperatingState(res?.EquipmentOperatingState)
        EnvironmentInfo(res?.EnvironmentInfo)
        ChartInspectionAberrantLevel(res?.ChartInspectionAberrantLevel)
        ChartInspectionAberrantResolve(res?.ChartInspectionAberrantResolve)

        await bim.loadModels(bim.getModelsUrl(currentLocation.value))
        await createBeaconPoint(currentLocation.FSN);
        InspectionCurrentPos(res?.InspectionCurrentPos)

        currentLocation.addEventListener('change', async (e) => {
            clearTimeout(timer);

            bim.destroyBeaconPoint()
            bim.unloadModels()
            await bim.loadModels(bim.getModelsUrl(currentLocation.value))
            await createBeaconPoint(currentLocation.FSN)

            update()
        })

        bim.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, () => {
            InspectionCurrentPos_Pins.forEach((pin) => {
                pin.update()
            })
        })

        bim.viewer.loading.addEventListener('hide', () => {
            InspectionCurrentPos_Pins.forEach((pin) => {
                pin.hide()
            })
        })

        bim.viewer.loading.addEventListener('show', () => {
            InspectionCurrentPos_Pins.forEach((pin) => {
                pin.show()
                pin.update()
            })
        })

        timer = setTimeout(update, updateTime)
    }
    async function update() {
        clearTimeout(timer);
        const res = await getData()

        ChartInspectionEquipmentState(res?.ChartInspectionEquipmentState)
        ChartInspectionCompleteState(res?.ChartInspectionCompleteState)
        EquipmentOperatingState(res?.EquipmentOperatingState)
        EnvironmentInfo(res?.EnvironmentInfo)
        ChartInspectionAberrantLevel(res?.ChartInspectionAberrantLevel)
        ChartInspectionAberrantResolve(res?.ChartInspectionAberrantResolve)
        InspectionCurrentPos(res?.InspectionCurrentPos)

        timer = setTimeout(update, updateTime)
    }
    async function createBeaconPoint(FSN) {
        const data = { FSN };
        const beaconPoint = await $.ajax({
            url: "/Beacon/GetFloorBeacons",
            data,
            type: "GET",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
        }).then((res) => {
            if (res.ErrorMessage) throw new Error(res.ErrorMessage);
            if (res.Datas) return res.Datas;
            return res;
        })
        console.log(beaconPoint);
        bim.createBeaconPoint(beaconPoint)

        // download beaconPoint
        const pins = {
            FSN,
            pins: bim.beaconPoint.map((pin) => {
                return {
                    ...pin.data,
                    position: pin?.position?.toArray?.() ?? null
                }
            })
        }
        console.log('PINS', pins)
        downloadJson(pins, `BeaconPoint_${FSN}`)

    }
    // #endregion

    // #region chart function
    //本日設備維修及保養統計
    function ChartInspectionEquipmentState(data) {
        const container = document.getElementById('ChartInspectionEquipmentState');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        ctx.width = pieSize
        ctx.height = pieSize
        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.labels = data.map(x => x.label);
            chart.data.datasets[0].data = data.map(x => x.value);
            chart.update();
            return;
        }
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '本日設備維修及保養統計',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, pieBackground, emptyDoughnut,
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
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //本日巡檢總計畫進度
    function ChartInspectionCompleteState(data) {
        const container = document.getElementById('ChartInspectionCompleteState');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        ctx.width = pieSize
        ctx.height = pieSize
        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.labels = data.map(x => x.label);
            chart.data.datasets[0].data = data.map(x => x.value);
            chart.update();
            return;
        }
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '本日巡檢總計畫進度',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, pieBackground, emptyDoughnut,
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
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
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
                        return "orange";
                    case "2":
                        return "red";
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
    //本月緊急事件等級占比/處理狀況
    function ChartInspectionAberrantLevel(data) {
        const container = document.getElementById('ChartInspectionAberrantLevel');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#2CB6F0", "#E77272"]
        ctx.width = pieSize
        ctx.height = pieSize
        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.labels = data.map(x => x.label);
            chart.data.datasets[0].data = data.map(x => x.value);
            chart.update();
            return;
        }
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
        let chart = Chart.getChart(ctx)
        if (chart) {
            chart.data.labels = data.map(x => x.label);
            chart.data.datasets[0].data = data.map(x => x.value);
            chart.update();
            return;
        }
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
        InspectionCurrentPos_Pins.forEach((pin) => {
            pin.destroy()
        })
        InspectionCurrentPos_Pins.length = 0;
        InspectionCurrentPos_Data.current = data.current;
        InspectionCurrentPos_Data.another = data.another;
        const infoBox = document.getElementById('box-InspectionCurrentPos');
        const container = document.getElementById('InspectionCurrentPos');
        const container_another = document.getElementById('InspectionAnotherPos');

        const htmls = createPersons(InspectionCurrentPos_Data.current, true)
        const htmls_another = createPersons(InspectionCurrentPos_Data.another)
        container.replaceChildren()
        container.insertAdjacentHTML('beforeend', htmls.join(''))

        container_another.replaceChildren()
        container_another.insertAdjacentHTML('beforeend', htmls_another.join(''))

        if (container.dataset.activePerson) {
            const activePerson = container.querySelector(`.plan-person[data-name="${container.dataset.activePerson}"]`)
            if (activePerson) {
                activePerson.classList.add('active');
                InspectionCurrentPos_Pins.forEach((pin) => {
                    if (pin.data.name === container.dataset.activePerson) {
                        pin.popover.show()
                    }
                    else {
                        pin.popover.hide()
                    }
                })
            }
        }

        if (infoBox.dataset.initialized) {
            return;
        }

        infoBox.addEventListener('click', (e) => {
            const target = e.target;
            if (target && target.classList.contains('plan-person')) {
                if (target.parentElement.id === 'InspectionAnotherPos') {
                    const d = InspectionCurrentPos_Data.another.find(x => x.name === target.dataset.name);
                    console.log("InspectionAnotherPos Select:", d)
                    currentLocation.setViewName(d.ViewName);

                    container.replaceChildren()
                    container.appendChild(target)
                    container_another.replaceChildren()
                    return;
                }
                const activePersons = Array.from(document.querySelectorAll('.plan-person.active'))
                activePersons.forEach((el) => {
                    el.classList.remove('active')
                })

                // remove current active persons
                if (activePersons.findIndex(x => x === target) !== -1) {
                    container.dataset.activePerson = '';
                    InspectionCurrentPos_Pins.forEach((pin) => {
                        pin.popover.hide()
                    })
                    return;
                }
                target.classList.add('active')
                container.dataset.activePerson = target.dataset.name;

                InspectionCurrentPos_Pins.forEach((pin) => {
                    if (pin.data.name === target.dataset.name) {
                        pin.popover.show()
                    }
                    else {
                        pin.popover.hide()
                    }
                })
            }
        })

        infoBox.dataset.initialized = true;

        function createPersons(data, needCreatePin = false) {

            return data.map((d) => {
                return createPerson(d)
            })
            function createPerson(data) {
                needCreatePin && createPin(data)
                data.time = dateTransform(data.time);
                return `<div class="plan-person ${isAlert(data) ? 'error' : ''}" data-name="${data.name}">
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

                function createPin(d) {
                    const pin = new ForgePin({
                        viewer: bim.viewer,
                        id: d.name,
                        position: new THREE.Vector3(d.position.x, d.position.y, 0),
                        img: "/Content/img/pin.svg",
                        data: d,
                    })
                    pin.addPopover({
                        offset: ['50%', 'calc(-100% + 16px)'],
                        html: `<div class="pin-popover">
                            <div class="popover-header">
                                <span class="num">Pt01</span><span class="name">${d.name}</span>
                            </div>
                            <div class="popover-body">
                                <i class="${getHeartIcon(d.heartAlert)}"></i>
                                <span class="label">心率：</span>
                                <span class="value">${d.heart}</span>
                                <span class="unit">下/分</span>
                            </div>
                        </div>`,
                    })
                    InspectionCurrentPos_Pins.push(pin)

                    if (!bim.viewer.loading.loading) {
                        pin.show()
                        pin.update()
                    }

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
                        case "1": return "orange";
                        case "2": return "red";
                        default: return "";
                    }
                }
                function getIcon(s) {
                    switch (s) {
                        case "1": return "fa-solid fa-heart-circle-exclamation";
                        case "2": return "fa-solid fa-shoe-prints";
                        case "3": return "fa-solid fa-user-clock";
                        case "4": return "fa-solid fa-land-mine-on";
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
    this.value = null;
    this.FSN = null;
    this.data = [];
    this.init = async () => {
        this.data = await $.getJSON("/DropDownList/ViewName")
        menu.replaceChildren();
        this.activeIndex = this.data.findIndex((e) => e.Value === '進抽站-B2F')
        const htmls = this.data.map((e, i) => {
            const active = this.activeIndex === i ? 'active' : '';
            const disabled = e.Value === '進抽站-B3F'
            return `<li><button class="dropdown-item ${active}" data-index="${i}" data-value="${e.Value}" ${disabled ? 'disabled' : ''}>${e.Text}</button></li>`
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
        this.FSN = d.FSN;
        const items = Array.from(menu.querySelectorAll(".dropdown-item"))
        items.forEach((e) => {
            if (parseInt(e.dataset.index) === this.activeIndex) {
                e.classList.add('active')
                return
            }
            e.classList.remove('active')
        })
        console.log("update CurrentLocation Data", d);

        menu.dispatchEvent(new Event('change'))
    }
    this.setViewName = (viewName) => {
        const activeIndex = this.data.findIndex(x => x.Value === viewName);
        if (activeIndex === -1) {
            throw new Error('[CurrentLocation.setViewName] Invalid ViewName.')
        }
        this.activeIndex = activeIndex
        this.updateData();
    }
    this.addEventListener = (eventType, callback) => {
        menu.addEventListener(eventType, callback.bind(this, this))
    }

    return this
}

//小鈴鐺
function AlertCollapse() {
    this.container;
    this.btn;
    this.data = [];
    this.isRead = new Set();
    this.timer = null;
    this.getData = async () => {
        this.data = await $.getJSON("/WarningMessage_Management/BellMessageInfo")
        const html = this.data.map(createItem).join('');
        this.container.replaceChildren()
        this.container.insertAdjacentHTML('beforeend', html)
    }
    this.getRead = async () => {
        await $.getJSON("/WarningMessage_Management/GetHaveReadMessage").then(x => {
            x.Datas.forEach(d => this.isRead.add(d))
        })
        let count = 0;
        for (const d of this.data) {
            if (!this.isRead.has(d.WMSN)) {
                count++;
            }
        }
        this.badge.textContent = count || null;
    }
    this.read = async (list) => {
        await $.ajax({
            url: `/WarningMessage_Management/PostHaveReadMessage`,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(list),
        })
    }
    this.init = async () => {
        this.btn = document.querySelector(`[data-bs-target="#AlertCollapse"]`)
        this.collapse = document.querySelector("#AlertCollapse")
        this.container = this.collapse.querySelector(".simplebar-content")
        this.badge = this.btn.querySelector(".badge")
        this.update()
        this.collapse.addEventListener('show.bs.collapse', async () => {
            this.read(this.data.map(d => d.WMSN))
        })
    }
    this.update = async () => {
        clearTimeout(this.timer)
        await this.getData();
        await this.getRead();
        this.timer = setTimeout(this.update, 10000)
    }
    function createItem(d) {
        return `
        <a class="alert-item" target="_blank" data-level="${d.WMType}" data-process-state="${d.WMState}">
            <i class="icon-alert"></i>
            <span class="road">${d.Location ?? ""}</span>
            <span class="time">${d.TimeOfOccurrence}</span>
            <span class="title">${d.Message}</span>
            <span class="process-state"></span>
        </a>`
    }
    return this;
}

const __DEBUG_DOWNLOAD_JSON__ = false;
function downloadJson(obj, fileName) {
    if (!__DEBUG_DOWNLOAD_JSON__) return;
    const json = "data:text/json;charset=utf-8," + encodeURIComponent(JSON.stringify(obj));
    const a = document.createElement('a');
    a.setAttribute("href", json);
    a.setAttribute("download", fileName + ".json");
    document.body.appendChild(a);
    a.click();
    a.remove();
}