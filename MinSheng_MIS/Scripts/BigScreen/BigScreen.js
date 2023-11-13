window.addEventListener('load', () => {
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
    Electricity_Usage_Information()

    Inspection_Aberrant_Level()
    Inspection_Aberrant_Resolve()

    Equipment_Availability_Rate()
    // #endregion

    // #region chart create function
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
    GIS()
    function GIS() {

    }
    // #endregion

})