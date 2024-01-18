window.addEventListener('load', async () => {
    // #region init
    generate();
    //selectIPlan();
    //await pushSelect("SelectIPlan", "/DropDownList/InspectionPlan")
    $("#SelectIPlan").change(selectIPlan)
    function selectIPlan() {
        $("#Plan_People_List").addClass("loading")

        let data = { IPSN: this?.value || null }
        $.ajax({
            url: "/InspectionPlan_Management/GetPlan_People_List",
            data,
            type: "GET",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: Plan_People_List,
            error: () => { Plan_People_List([]) }
        })
    }
    function generate(res) {
        $(".info-area").addClass("loading")

        $.ajax({
            url: "/InspectionPlan_Management/GetCurrentInformation",
            type: "GET",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: generate,
            error: generate
        })
        function generate(res) {
            console.log(res)
            Inspection_Complete_State(res?.Inspection_Complete_State)
            Inspection_Members(res?.Inspection_Members)
            Inspection_Plan_List(res?.Inspection_Plan_List)
            Inspection_Aberrant_Level(res?.Inspection_Aberrant_Level)
            Inspection_Aberrant_Resolve(res?.Inspection_Aberrant_Resolve)
            Equipment_Maintain_And_Repair_Statistics(res?.Equipment_Maintain_And_Repair_Statistics)
            Equipment_Level_Rate(res?.Equipment_Level_Rate)
            Equipment_Type_Rate(res?.Equipment_Type_Rate)

            $(".info-area").removeClass("loading")
        }
    }
    // #endregion

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
        offset: { x: 0, y: 4 }
    }

    // #endregion

    // #region chart function
    //本日巡檢人員列表
    function Plan_People_List(res) {
        const row = $("#Plan_People_List .plan-people").first()
        const list = $("#Plan_People_List .simplebar-content")
        list.empty()

        if (!res) {
            for (let i = 0; i < 20; i++) { list.append(row.clone()) }
            $("#Plan_People_List .plan-people").attr("href", `/InspectionPlan_Management/CurrentPosition`)
            return
        }
        res.forEach((e) => {
            let item = row.clone()
            item.find("#MyName").text(e.IPSN + ' ' + e.MyName)
            item.find("#Location").text(e.Area + e.Floor)
            item.find("#HeartBeat").text(e.HeartBeat)
            item.attr("href", `/InspectionPlan_Management/CurrentPosition/${e.PMSN}`)

            list.append(item)
        })

        $("#Plan_People_List").removeClass("loading")
    }
    //本日巡檢計畫進度
    function Inspection_Complete_State(res) {
        const container = document.getElementById('Inspection_Complete_State');
        const ctx = getOrCreateElement(container, 'canvas')
        const backgroundColor = ["#72E998", "#E9CD68", "#2CB6F0"]
        const data = res || [
            { label: "待執行", value: 15 },
            { label: "巡檢中", value: 10 },
            { label: "巡檢完成", value: 16 },
            { label: "巡檢未完成", value: 16 },
        ]
        ctx.width = 160
        ctx.height = 160
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
    //當前巡檢狀況
    function Inspection_Members(res) {
        $("#Inspection_Members_All").text(res?.Inspection_Members_All || 0)
        $("#Inspection_Members_Notice").text(res?.Inspection_Members_Notice || 0)
        $("#Inspection_Members_Alert").text(res?.Inspection_Members_Alert || 0)
    }
    //本日巡檢計畫列表
    function Inspection_Plan_List(res) {
        const row = $("#Inspection_Plan_List .row").first()
        const list = $("#Inspection_Plan_List .simplebar-content")
        list.empty()

        if (!res) {
            for (let i = 0; i < 3; i++) { list.append(row.clone()) }
            return
        }

        res.forEach((e) => {
            let item = row.clone()
            item.find("#PlanState").text(e.PlanState)
            item.find("#IPName").text(e.IPName)
            item.find("#Shift").text(e.Shift)

            switch (e.PlanState) {
                case "待執行": item.find("#PlanState").addClass("text-info"); break;
                case "執行中": item.find("#PlanState").addClass("text-warning"); break;
                case "已完成": item.find("#PlanState").addClass("text-success"); break;
            }

            list.append(item)
        })
    }
    //本日緊急事件 等級占比
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
    //本日緊急事件 處理狀況
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
    //本日設備保養及維修進度統計
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
                        barPercentage: 0.3,
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
                            color: "#DADADA",
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
                            color: "#DADADA",
                            font: { family, size: 14 }
                        },
                        border: {
                            color: "#DADADA"
                        },
                        grid: {
                            color: "#DADADA",
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

        let chart = Chart.getChart(ctx)
        if (chart) { chart.destroy() }

        ctx.width = 200
        ctx.height = 400
        new Chart(ctx, options('x'))
    }
    //本日設備故障等級分布
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
    //本日設備故障類型占比
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

        ctx.width = 160
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
                plugins: { legend, tooltip }
            },
            plugins: [chartPlugins.emptyDoughnut]
        })
    }
    // #endregion
})