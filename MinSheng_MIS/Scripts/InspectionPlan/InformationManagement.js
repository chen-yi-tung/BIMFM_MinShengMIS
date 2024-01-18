window.addEventListener('load',async () => {
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

    // #endregion

    // #region init
    const StartYear = $("#StartYear"),
        StartMonth = $("#StartMonth"),
        EndYear = $("#EndYear"),
        EndMonth = $("#EndMonth");

    search();
    $("#search").click(search)
    function search() {
        $(".info-area").addClass("loading")

        let data = {
            year1: StartYear.val(),
            month1: StartMonth.val(),
            year2: EndYear.val(),
            month2: EndMonth.val(),
        }

        if (parseInt(data.year1) > parseInt(data.year2)) {
            data = {
                year1: EndYear.val(),
                month1: EndMonth.val(),
                year2: StartYear.val(),
                month2: StartMonth.val(),
            }
        }
        else if (parseInt(data.month1) > parseInt(data.month2)) {
            data = {
                year1: StartYear.val(),
                month1: EndMonth.val(),
                year2: EndYear.val(),
                month2: StartMonth.val(),
            }
        }

        StartYear.val(data.year1)
        StartMonth.val(data.month1)
        EndYear.val(data.year2)
        EndMonth.val(data.month2)

        $.ajax({
            url: "/InspectionPlan_Management/GetIspectionPlanInformation",
            data,
            type: "GET",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: generate,
            error: generate
        })
    }
    function generate(res) {
        console.log(res)
        Inspection_Complete_State(res?.Inspection_Complete_State)
        Inspection_Equipment_State(res?.Inspection_Equipment_State)
        Inspection_All_Members(res?.Inspection_All_Members)
        Inspection_Aberrant_Level(res?.Inspection_Aberrant_Level)
        Inspection_Aberrant_Resolve(res?.Inspection_Aberrant_Resolve)
        Equipment_Maintain_And_Repair_Statistics(res?.Equipment_Maintain_And_Repair_Statistics)
        Equipment_Level_Rate(res?.Equipment_Level_Rate)
        Equipment_Type_Rate(res?.Equipment_Type_Rate)

        $(".info-area").removeClass("loading")
    }
    // #endregion

    // #region chart function
    //巡檢總計畫完成狀態
    function Inspection_Complete_State(res) {
        const container = document.getElementById('Inspection_Complete_State');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0", "#E77272"]
        const data = res || [
            { label: "待執行", value: 15 },
            { label: "巡檢中", value: 10 },
            { label: "巡檢完成", value: 16 },
            { label: "巡檢未完成", value: 16 },
        ]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '巡檢總計畫完成狀態',
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
                    legend, tooltip, shadowPlugin,
                    centerText: {
                        text: [
                            {
                                string: (() => {
                                    let total = data.reduce((t, e) => t + e.value, 0)
                                    let value = data.find(x => x.label == "巡檢完成").value
                                    return ((Math.floor(value / total * 1000) / 10) || 0) + "%"
                                })(),
                                color: "#fff",
                                font: { family, weight: 500, size: 20 }
                            }
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: data.reduce((t, e) => t + e.value, 0),
                            unit: "總計畫數"
                        },
                        percentage: false
                    }
                }
            },
            plugins: [
                chartPlugins.shadowPlugin,
                chartPlugins.centerText,
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //巡檢設備統計
    function Inspection_Equipment_State(res) {
        const container = document.getElementById('Inspection_Equipment_State');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        const data = res || [
            { label: "維修+保養", value: 128 },
            { label: "維修", value: 19 },
            { label: "保養", value: 15 }
        ]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '巡檢總設備狀態',
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
                    legend, tooltip, shadowPlugin,
                    centerText: {
                        text: [
                            {
                                string: (() => {
                                    let total = data.reduce((t, e) => t + e.value, 0)
                                    let value = data.find(x => x.label.includes("保養+維修")).value
                                    return ((Math.floor(value / total * 1000) / 10) || 0) + "%"
                                })(),
                                color: "#fff",
                                font: { family, weight: 500, size: 20 }
                            }
                        ]
                    },
                    htmlLegend: {
                        statistics: {
                            value: data.reduce((t, e) => t + e.value, 0),
                            unit: "總設備數"
                        },
                        percentage: false
                    }
                }
            },
            plugins: [
                chartPlugins.shadowPlugin,
                chartPlugins.centerText,
                chartPlugins.htmlLegend,
                chartPlugins.emptyDoughnut
            ]
        })
    }
    //巡檢人員表格
    function Inspection_All_Members(res) {
        const row = $("#Inspection_All_Members .row").first()
        const list = $("#Inspection_All_Members .simplebar-content")
        list.empty()

        if (!res) {
            for (let i = 0; i < 20; i++) { list.append(row.clone()) }
            return
        }

        res.forEach((e) => {
            let item = row.clone()
            item.find("#MyName").text(e.MyName)
            item.find("#PlanNum").text(e.PlanNum)
            item.find("#MaintainNum").text(e.MaintainNum)
            item.find("#RepairNum").text(e.RepairNum)

            item.find("#CompleteNum").text(e.CompleteNum)
            item.find("#CompletionRate").text(Math.floor(e.CompletionRate * 100 * 100) / 100 + "%")
            if (e.CompletionRate < 0.5) {
                item.find("[data-complete]").attr("data-complete", false)
            }
            else {
                item.find("[data-complete]").attr("data-complete", true)
            }

            list.append(item)
        })
    }
    //緊急事件 等級占比
    function Inspection_Aberrant_Level(res) {
        const container = document.getElementById('Inspection_Aberrant_Level');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#2CB6F0", "#E77272"]
        const data = res || [
            { label: "一般", value: 20 },
            { label: "緊急", value: 15 }
        ]

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
    function Inspection_Aberrant_Resolve(res) {
        const container = document.getElementById('Inspection_Aberrant_Resolve');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E77272", "#4269AC"]
        const data = res || [
            { label: "待處理", value: 53 },
            { label: "處理中", value: 53 },
            { label: "處理完成", value: 15 }
        ]

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
                                string: (() => {
                                    let total = data.reduce((t, e) => t + e.value, 0)
                                    let value = data.find(x => x.label == "處理完成").value
                                    return ((Math.floor(value / total * 1000) / 10) || 0) + "%"
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
                            unit: "緊急事件"
                        },
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
    function Equipment_Maintain_And_Repair_Statistics(res) {
        const container = document.getElementById('Equipment_Maintain_And_Repair_Statistics');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#4269AC", "#72BEE9", "#BC72E9", "#FFAB2E", "#B7B7B7", "#72E998", "#E77272"]
        const data = res || [
            { label: "已派工", value: { Maintain: 7, Repair: 8 } },
            { label: "施工中", value: { Maintain: 8, Repair: 5 } },
            { label: "待審核", value: { Maintain: 12, Repair: 8 } },
            { label: "未完成", value: { Maintain: 15, Repair: 10 } },
            { label: "待補件", value: { Maintain: 3, Repair: 1 } },
            { label: "完成", value: { Maintain: 20, Repair: 12 } },
            { label: "審核未過", value: { Maintain: 2, Repair: 3 } }
        ]
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
                            font: { family, size: 14 }
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
                            font: { family, size: 14 }
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
                    legend, tooltip,
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
                ctx.height = 100
                new Chart(ctx, options('y'))
            }
        }
    }
    //設備故障等級分布
    function Equipment_Level_Rate(res) {
        const container = document.getElementById('Equipment_Level_Rate');
        const ctx = getOrCreateElement(container, 'canvas')

        const backgroundColor = ["#72E998", "#E9CD68", "#E77272"]
        const data = res || [
            { label: "一般", value: 15 },
            { label: "緊急", value: 7 },
            { label: "最速件", value: 3 },
        ]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
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
                //layout: { padding: 4 },
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
    function Equipment_Type_Rate(res) {
        const container = document.getElementById('Equipment_Type_Rate');
        const ctx = getOrCreateElement(container, 'canvas')

        const backgroundColor = ["#9E66C1", "#2CB6F0", "#72E998", "#E9CD68", "#E77272"]
        const data = res || [
            { label: "類型一", value: 2 },
            { label: "類型二", value: 3 },
            { label: "類型三", value: 3 },
            { label: "類型四", value: 1 },
            { label: "類型五", value: 3 },
        ]

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        //ctx.width = 160
        ctx.height = 160
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.label),
                datasets: [{
                    label: '設備故障類型占比',
                    data: data.map(x => x.value),
                    backgroundColor,
                    borderWidth: 0,
                    cutout: "60%"
                }]
            },
            options: {
                responsive: false,
                //layout: { padding: 4 },
                plugins: {
                    legend, tooltip,
                }
            },
            plugins: [chartPlugins.emptyDoughnut]
        })
    }
    // #endregion
})