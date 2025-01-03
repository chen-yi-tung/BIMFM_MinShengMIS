const fakeData = {
    ChartInspectionCompleteState: [
        { label: "已完成", value: 10 },
        { label: "執行中", value: 16 },
        { label: "待執行", value: 15 },
    ],
    ChartInspectionEquipmentState: [
        { label: "維修+保養", value: 128 },
        { label: "維修", value: 19 },
        { label: "保養", value: 15 }
    ],
    InspectionMembers: [
        {
            MyName: "John Doe",
            PlanNum: 12,
            PlanCompleteNum: 11,
            PlanCompletionRate: 0.916,
            MaintainNum: 8,
            MaintainCompleteNum: 7,
            MaintainCompletionRate: 0.875,
            RepairNum: 4,
            RepairCompleteNum: 1,
            RepairCompletionRate: 0.25,
        }
    ],
    ChartInspectionAberrantLevel: [
        { label: "一般", value: 20 },
        { label: "緊急", value: 15 }
    ],
    ChartInspectionAberrantResolve: [
        { label: "待處理", value: 15 },
        { label: "處理中", value: 15 },
        { label: "處理完成", value: 53 },
    ],
    ChartEquipmentProgressStatistics: [
        { label: "待派工", value: { Maintain: 7, Repair: 8 } },
        { label: "待執行", value: { Maintain: 8, Repair: 5 } },
        { label: "待審核", value: { Maintain: 12, Repair: 8 } },
        { label: "審核通過", value: { Maintain: 15, Repair: 10 } },
        { label: "審核未過", value: { Maintain: 3, Repair: 1 } },
    ],
    ChartEquipmentLevelRate: [
        { label: "一般", value: 15 },
        { label: "緊急", value: 7 },
        { label: "最速件", value: 3 },
    ],
    ChartEquipmentTypeRate: [
        { label: "類型一", value: 2 },
        { label: "類型二", value: 3 },
        { label: "類型三", value: 3 },
        { label: "類型四", value: 1 },
        { label: "類型五", value: 3 },
    ]
}

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
    const shadowPlugin = {
        color: 'rgba(0,0,0,0.25)',
        blur: 4,
        offset: { x: 0, y: 4 },
        drawWhenEmpty: false
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

    // #region init

    //預設最小民國年分
    const MIN_YEAR = 2023;
    //最大民國年分 => 今年
    const MAX_YEAR = new Date().getFullYear();
    //目前月份
    const MAX_MONTH = new Date().getMonth() + 1;
    //預設民國年分
    const DEFAULT_YEAR = MAX_YEAR;

    const StartYear = $("#StartYear"),
        StartMonth = $("#StartMonth"),
        EndYear = $("#EndYear"),
        EndMonth = $("#EndMonth");

    init()

    async function init() {
        for (let y = MIN_YEAR; y <= MAX_YEAR; y++) {
            StartYear.append(`<option value="${y}">${y - 1911}年度</option>`)
            EndYear.append(`<option value="${y}">${y - 1911}年度</option>`)
        }
        StartYear.change(maxMonth)
        EndYear.change(maxMonth)

        StartYear.val(DEFAULT_YEAR)
        StartYear.change()
        EndYear.val(DEFAULT_YEAR)
        EndYear.change()

        function maxMonth() {
            const options = [].slice.call(this.nextElementSibling.children)
            const selectedMonth = parseInt(this.nextElementSibling.value)
            if (this.value == MAX_YEAR) {
                options.forEach((o) => { o.hidden = o.value > MAX_MONTH })
                this.nextElementSibling.value = Math.min(selectedMonth, MAX_MONTH);
                return
            }
            options.forEach((o) => { o.hidden = false });
        }


        search();
        $("#Search").click(search)
        function search() {
            $(".info-area").addClass("loading")

            let data = {
                year1: StartYear.val(),
                month1: StartMonth.val(),
                year2: EndYear.val(),
                month2: EndMonth.val(),
            }

            data = orderDate(data)

            StartYear.val(data.year1)
            StartMonth.val(data.month1)
            EndYear.val(data.year2)
            EndMonth.val(data.month2)

            $.ajax({
                url: "/InspectionPlan_Management/GetInspectionPlanInformation",
                data,
                type: "GET",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: generate,
                error: generate
            })
        }
        function generate(res) {
            console.log(res);
            ChartInspectionCompleteState(res?.ChartInspectionCompleteState)
            ChartInspectionEquipmentState(res?.ChartInspectionEquipmentState)
            InspectionMembers(res?.InspectionMembers)
            ChartInspectionAberrantLevel(res?.ChartInspectionAberrantLevel)
            ChartInspectionAberrantResolve(res?.ChartInspectionAberrantResolve)
            ChartEquipmentProgressStatistics(res?.ChartEquipmentProgressStatistics)
            ChartEquipmentLevelRate(res?.ChartEquipmentLevelRate)
            ChartEquipmentTypeRate(res?.ChartEquipmentTypeRate)

            $(".info-area").removeClass("loading")
        }
    }
    function orderDate(_data) {
        let data = _data;
        if (parseInt(_data.year1) > parseInt(_data.year2)) {
            data = {
                year1: _data.year2,
                month1: _data.month2,
                year2: _data.year1,
                month2: _data.month1,
            }
        }
        else if (parseInt(_data.year1) == parseInt(_data.year2) && parseInt(_data.month1) > parseInt(_data.month2)) {
            data = {
                year1: _data.year1,
                month1: _data.month2,
                year2: _data.year2,
                month2: _data.month1,
            }
        }
        return data
    }
    // #endregion

    // #region chart function
    //巡檢總計畫完成狀態
    function ChartInspectionCompleteState(data) {
        console.log(data);

        const container = document.getElementById('ChartInspectionCompleteState');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '巡檢總計畫完成狀態',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, shadowPlugin,
                    htmlLegend: {
                        statistics: {
                            value: (data) => data.reduce((t, e) => t + e, 0),
                            unit: "總計畫數",
                        },
                        percentage: false,
                        value: true,
                    }
                }
            },
            plugins: [
                chartPlugins.shadowPlugin,
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //設備維修及保養統計
    function ChartInspectionEquipmentState(data) {
        const container = document.getElementById('ChartInspectionEquipmentState');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備維修及保養統計',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                layout: { padding: 4 },
                plugins: {
                    legend, tooltip, shadowPlugin,
                    htmlLegend: {
                        statistics: {
                            value: (data) => data.reduce((t, e) => t + e, 0),
                            unit: "總設備數"
                        },
                        percentage: false,
                        value: true,
                    }
                }
            },
            plugins: [
                chartPlugins.shadowPlugin,
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //巡檢人員表格
    function InspectionMembers(data) {
        const row = $("#InspectionMembers .row").first()
        const list = $("#InspectionMembers .simplebar-content")
        list.empty()

        data.forEach((e) => {
            let item = row.clone()
            item.find("#MyName").text(e.MyName)
            item.find("#PlanNum").text(e.PlanNum)
            item.find("#MaintainNum").text(e.MaintainNum)
            item.find("#RepairNum").text(e.RepairNum)
            calcCompletion("Plan", e)
            calcCompletion("Maintain", e)
            calcCompletion("Repair", e)

            list.append(item)

            function calcCompletion(id = "Plan", data) {
                const NumKey = `${id}CompleteNum`;
                const RateKey = `${id}CompletionRate`;
                item.find("#" + NumKey).text(data[NumKey])
                item.find("#" + RateKey).text(Math.floor(data[RateKey] * 100 * 100) / 100 + "%")
                const complete = data[RateKey] >= 0.5
                item.find(`#${NumKey}[data-complete]`).attr("data-complete", complete)
                item.find(`#${RateKey}[data-complete]`).attr("data-complete", complete)

            }
        })
    }
    //緊急事件 等級占比
    function ChartInspectionAberrantLevel(data) {
        const container = document.getElementById('ChartInspectionAberrantLevel');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#2CB6F0", "#E77272"]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '緊急事件等級占比',
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
                        value: false
                    }
                }
            },
            plugins: [
                chartPlugins.pieBackground,
                chartPlugins.centerText,
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //緊急事件 處理狀況
    function ChartInspectionAberrantResolve(data) {
        const container = document.getElementById('ChartInspectionAberrantResolve');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#E77272", "#FFA54B", "#72E998"]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '緊急事件處理狀況',
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
                            unit: "緊急事件"
                        },
                        percentage: false,
                        value: true,
                    }
                }
            },
            plugins: [
                chartPlugins.pieBackground,
                chartPlugins.centerText,
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //設備保養及維修進度統計
    const mediaQueryList = window.matchMedia("(max-width:700px)")
    function ChartEquipmentProgressStatistics(data) {
        const container = document.getElementById('ChartEquipmentProgressStatistics');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#4269AC", "#72BEE9", "#BC72E9", "#FFAB2E", "#E77272"]
        const options = (indexAxis = 'y') => ({
            type: 'bar',
            data: {
                labels: ["設備保養", "設備維修"],
                datasets: data.map(({ label, value: { Maintain, Repair } }, i) => {
                    return {
                        label,
                        data: [Maintain, Repair],
                        backgroundColor: backgroundColor[i],
                        borderWidth: 0,
                        barPercentage: 0.4,
                        categoryPercentage: 1,
                    }
                })
            },
            options: {
                maintainAspectRatio: false,
                responsive: false,
                indexAxis,
                scales: {
                    x: {
                        stacked: true,
                        ticks: {
                            color: "#DDDCDC",
                            font: { family, size: 14 },
                            padding: 6
                        },
                        border: {
                            color: "#DADADA"
                        },
                        grid: {
                            color: "#DADADA",
                            drawTicks: false
                        }
                    },
                    y: {
                        stacked: true,
                        ticks: {
                            color: "#EFEFEF",
                            font: { family, size: 14 },
                            padding: 6
                        },
                        grid: {
                            color(context) {
                                if (context.type == "tick" && context.index == 0) {
                                    return "#DADADA"
                                }
                                return "transparent"
                            },
                            drawTicks: false
                        }
                    }
                },
                plugins: {
                    legend,
                    tooltip: {
                        bodyFont: { family, size: 12 },
                        callbacks: {
                            title: () => '',
                            label: (context) => {
                                let label = context?.dataset?.label ?? '';
                                let value = context.formattedValue ?? '';
                                return ` ${label}：${value}`;
                            }
                        }
                    },
                    htmlLegend: { value: false }
                }
            },
            plugins: [chartPlugins.htmlLegend]
        })

        mediaQueryList.onchange = onMediaChange
        onMediaChange(mediaQueryList)

        function onMediaChange(evt) {
            const matches = evt.matches
            let chart = Chart.getChart(ctx)
            if (chart) {
                chart.destroy();
            }

            if (matches) {
                ctx.width = 200
                ctx.height = 400
                new Chart(ctx, options('x'))
            }
            else {
                ctx.width = 585
                ctx.height = 128
                new Chart(ctx, options('y'))
            }
        }
    }
    //設備故障等級分布
    function ChartEquipmentLevelRate(data) {
        const container = document.getElementById('ChartEquipmentLevelRate');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#E77272"]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備故障等級分布',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                plugins: {
                    legend, tooltip,
                    htmlLegend: {
                        percentage: false,
                        value: false
                    }
                }
            },
            plugins: [
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //設備故障類型占比
    function ChartEquipmentTypeRate(data) {
        const container = document.getElementById('ChartEquipmentTypeRate');
        const ctx = getOrCreateElement(container, 'canvas')

        const backgroundColor = ["#9E66C1", "#2CB6F0", "#72E998", "#E9CD68", "#E77272"]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備故障類型占比',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: false,
                plugins: {
                    legend, tooltip,
                    htmlLegend: {
                        percentage: false,
                        value: false
                    }
                }
            },
            plugins: [
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    // #endregion

    // #region Qunit test
    /*QUnit.module('orderDate', function () {
        QUnit.test('same year and not same month', function (assert) {
            assert.deepEqual(
                orderDate({
                    year1: "112", month1: "1",
                    year2: "112", month2: "12",
                }),
                {
                    year1: "112", month1: "1",
                    year2: "112", month2: "12",
                });
        });
        QUnit.test('not same year', function (assert) {
            assert.deepEqual(
                orderDate({
                    year1: "112", month1: "1",
                    year2: "113", month2: "1",
                }),
                {
                    year1: "112", month1: "1",
                    year2: "113", month2: "1",
                });
        });
        QUnit.test('same year and month need order', function (assert) {
            assert.deepEqual(
                orderDate({
                    year1: "112", month1: "12",
                    year2: "112", month2: "1",
                }),
                {
                    year1: "112", month1: "1",
                    year2: "112", month2: "12",
                });
        });
        QUnit.test('year need order', function (assert) {
            assert.deepEqual(
                orderDate({
                    year1: "113", month1: "1",
                    year2: "112", month2: "6",
                }),
                {
                    year1: "112", month1: "6",
                    year2: "113", month2: "1",
                });
        });
        QUnit.test('not same year and not same month but no need to order', function (assert) {
            assert.deepEqual(
                orderDate({
                    year1: "112", month1: "6",
                    year2: "113", month2: "1",
                }),
                {
                    year1: "112", month1: "6",
                    year2: "113", month2: "1",
                });
        });
        QUnit.test('nothing to do', function (assert) {
            assert.deepEqual(
                orderDate({
                    year1: "", month1: "",
                    year2: "", month2: "",
                }),
                {
                    year1: "", month1: "",
                    year2: "", month2: "",
                });
        });
    });*/
    // #endregion
})