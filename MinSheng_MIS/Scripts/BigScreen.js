// #region plugins
const chartPlugins = {
    pieBackground: {
        id: "pieBackground",
        beforeDatasetsDraw(chart, args, options) {
            const { chartArea: { left, top, right, bottom }, ctx } = chart;
            const { backgroundColor, borderWidth, borderColor, shadow = false } = options;
            const x = (left + right) / 2;
            const y = (top + bottom) / 2;
            const r = Math.min(right - left, bottom - top) / 2;
            ctx.save()

            ctx.beginPath()
            ctx.arc(x, y, r - 1, 0, 2 * Math.PI);
            ctx.lineWidth = borderWidth || 0;
            ctx.strokeStyle = borderColor || '#fff';
            ctx.lineWidth > 0 ? ctx.stroke() : void 0;
            ctx.fillStyle = backgroundColor || "#fff";
            if (shadow) {
                const { color = 'rgba(0,0,0,0.25)', blur = 4, offset = { x: 4, y: 4 } } = shadow;
                ctx.shadowColor = color;
                ctx.shadowBlur = blur;
                ctx.shadowOffsetX = offset.x || 0;
                ctx.shadowOffsetY = offset.y || 0;
            }
            ctx.fill()

            ctx.restore()
        }
    },
    centerText: {
        id: "centerText",
        beforeDatasetsDraw(chart, args, options) {
            const { chartArea: { left, top, right, bottom }, ctx } = chart;
            const { text, inline = false, verticalAlign = 'baseline' } = options;
            const lines = Array.isArray(text) ? text : [text];
            let x = (left + right) / 2;
            let y = (top + bottom) / 2;

            let va = void 0;
            switch (verticalAlign) {
                case 'baseline': break;
                case 'text-top':
                case 'top': va = 'actualBoundingBoxAscent'; break;
                case 'middle': va = 'fontBoundingBoxDescent'; break;
                case 'text-bottom':
                case 'bottom': va = 'actualBoundingBoxDescent'; break;
            }

            let _x = x
            let _y = y
            let maxSize = 0

            lines.forEach((options, i) => {
                renderText(options, true)
            })

            if (inline) {
                _x = x - ((_x - x) / 2)
                y = y + (maxSize / 2)
                _y = y
            }
            else {
                _y = y - ((_y - y) / 2)
            }

            lines.forEach((options) => {
                renderText(options)
            })

            function renderText(options, measure = false) {
                const { string = '', color = '#000', font: { family, size = 12, style = 'normal', weight = 'normal', lineHeight = 1 } } = options;
                ctx.save()
                ctx.font = `${style} ${weight} ${size}px ${family || "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif"}`
                ctx.fillStyle = color
                ctx.textAlign = inline ? 'left' : 'center'
                ctx.textBaseLine = 'middle'

                if (inline) {
                    let m = ctx.measureText(string)
                    _y += m?.[va] ?? 0
                    measure && (maxSize = Math.max(maxSize, m.actualBoundingBoxAscent))
                    !measure && ctx.fillText(string, _x, _y)
                    _x += m.width
                    _y = y
                }
                else {
                    _y += lineHeight * size
                    !measure && ctx.fillText(string, _x, _y)
                }
                ctx.restore()
            }
        }
    },
    htmlLegend: {
        id: "htmlLegend",
        afterUpdate(chart, args, options) {
            const { chartArea: { left, top, right, bottom }, ctx } = chart;
            const container = chart.canvas.parentElement
            const legend = getOrCreateElement(container, ".legend-container")
            const { statistics = false, percentage = false } = options
            if (statistics) {
                const { value, unit } = statistics
                const e = getOrCreateElement(legend, ".legend-statistics")
                if (value) {
                    const v = getOrCreateElement(e, "span.value")
                    v.textContent = value
                }
                if (unit) {
                    const u = getOrCreateElement(e, "span.unit")
                    u.textContent = unit
                }
            }
            const data = chart.data.datasets[0].data
            const total = data.reduce((t, e) => t + e)
            const items = chart.options.plugins.legend.labels.generateLabels(chart)
            const ul = getOrCreateElement(legend, "ul.legend-catalog")
            ul.replaceChildren()

            items.forEach((s, i) => {
                const li = document.createElement('li')
                li.style.setProperty("--c", s.fillStyle)
                li.appendChild(document.createElement('i'))

                li.ariaDisabled = s.hidden

                const label = document.createElement('div')
                label.className = 'label'
                label.textContent = s.text
                li.appendChild(label)

                const value = document.createElement('div')
                value.className = 'value'
                value.textContent = data[s.index]
                li.appendChild(value)

                if (percentage) {
                    const percentage = document.createElement('div')
                    percentage.className = 'percentage'
                    percentage.textContent = Math.floor(data[s.index] / total * 100)
                    li.appendChild(percentage)
                }

                li.onclick = () => {
                    const { type } = chart.config;
                    if (type === 'pie' || type === 'doughnut') {
                        chart.toggleDataVisibility(s.index)
                    }
                    else {
                        chart.setDataVisibility(s.datasetIndex, !chart.isDatasetVisible(s.datasetIndex))
                    }
                    chart.update()
                }

                ul.appendChild(li)
            })

            //legend.insertAdjacentHTML(``)
        }
    }
}

// #endregion

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


})

// #region chart create helper function
function getOrCreateElement(container, selector, tag = null) {
    let element = container.querySelector(selector)
    if (element) { return element }
    if (tag == null) {
        tag = selector.match(/^[^.|#][a-zA-Z0-9_-]+/g)?.[0] ?? 'div'
    }
    element = document.createElement(tag)
    selector.match(/[.][a-zA-Z0-9_-]+/g)?.forEach((className) => {
        element.classList.add(className.substring(1))
    })
    let id = selector.match(/#[a-zA-Z0-9_-]+/g)?.[0].substring(1)
    if (id) { element.id = id }

    container.append(element)
    return element
}
// #endregion