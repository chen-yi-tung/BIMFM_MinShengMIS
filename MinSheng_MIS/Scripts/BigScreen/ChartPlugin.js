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
            const { statistics = false, percentage = false, value = true } = options
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

                if (value) {
                    const value = document.createElement('div')
                    value.className = 'value'
                    value.textContent = data[s.index]
                    li.appendChild(value)
                }

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