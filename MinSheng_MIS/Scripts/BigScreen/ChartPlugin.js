// #region plugins
const chartPlugins = {
    pieBackground: {
        id: "pieBackground",
        beforeDatasetsDraw(chart, args, options) {
            const {
                chartArea: { left, top, right, bottom },
                ctx,
                data: { datasets },
            } = chart;
            const { backgroundColor, borderWidth, borderColor, shadow = false, drawWhenEmpty = false } = options;

            if (!drawWhenEmpty && !hasDataInDatasets(datasets)) {
                return;
            }

            const x = (left + right) / 2;
            const y = (top + bottom) / 2;
            const r = Math.min(right - left, bottom - top) / 2;
            ctx.save();

            ctx.beginPath();
            ctx.arc(x, y, r - 1, 0, 2 * Math.PI);
            ctx.lineWidth = borderWidth || 0;
            ctx.strokeStyle = borderColor || "#fff";
            ctx.lineWidth > 0 ? ctx.stroke() : void 0;
            ctx.fillStyle = backgroundColor || "#fff";
            if (shadow) {
                const { color = "rgba(0,0,0,0.25)", blur = 4, offset = { x: 4, y: 4 } } = shadow;
                ctx.shadowColor = color;
                ctx.shadowBlur = blur;
                ctx.shadowOffsetX = offset.x || 0;
                ctx.shadowOffsetY = offset.y || 0;
            }
            ctx.fill();

            ctx.restore();
        },
    },
    shadowPlugin: {
        id: "shadowPlugin",
        beforeDraw(chart, args, options) {
            const {
                chartArea: { left, top, right, bottom },
                ctx,
                data: { datasets },
            } = chart;
            const { color = "rgba(0,0,0,0.25)", blur = 4, offset = { x: 4, y: 4 }, drawWhenEmpty = false } = options;

            if (!drawWhenEmpty && !hasDataInDatasets(datasets)) {
                return;
            }

            const x = (left + right) / 2;
            const y = (top + bottom) / 2;
            const r = Math.min(right - left, bottom - top) / 2;
            let cutout = 1;
            if (chart.config.type == "doughnut") {
                cutout = chart.config.data.datasets[0].cutout;
                if (typeof cutout == "string") {
                    cutout = cutout.match(/(\d)+(?=%)/g)?.[0] / 100;
                    cutout = r * cutout;
                }
            }
            ctx.save();
            ctx.beginPath();
            ctx.arc(x + offset.x, y + offset.y, r, 0, Math.PI * 2, false);
            ctx.arc(x + offset.x, y + offset.y, cutout, 0, Math.PI * 2, true);
            ctx.fillStyle = color;
            ctx.filter = `blur(${blur}px)`;
            ctx.fill();
            ctx.restore();
        },
    },
    centerText: {
        id: "centerText",
        beforeDatasetsDraw(chart, args, options) {
            const {
                chartArea: { left, top, right, bottom },
                ctx,
                data: { datasets },
            } = chart;
            const { text, inline = false, verticalAlign = 'baseline', drawWhenEmpty = false } = options;
    
            if (!drawWhenEmpty && !hasDataInDatasets(datasets)) {
                return;
            }
    
            const lines = Array.isArray(text) ? text : [text];
            let x = (left + right) / 2;
            let y = (top + bottom) / 2;
    
            let va = void 0;
            switch (verticalAlign) {
                case 'baseline':
                    break;
                case 'text-top':
                case 'top':
                    va = 'actualBoundingBoxAscent';
                    break;
                case 'middle':
                    va = 'fontBoundingBoxDescent';
                    break;
                case 'text-bottom':
                case 'bottom':
                    va = 'actualBoundingBoxDescent';
                    break;
            }
    
            let _x = x;
            let _y = y;
            let maxSize = 0;
    
            lines.forEach((options, i) => {
                if (typeof options === 'string') {
                    renderText({ string: options }, true);
                } else {
                    renderText(options, true);
                }
            });
    
            if (inline) {
                _x = x - (_x - x) / 2;
                y = y + maxSize / 2;
                _y = y;
            } else {
                _y = y - (_y - y) / 2;
            }
    
            lines.forEach((options) => {
                if (typeof options === 'string') {
                    renderText({ string: options });
                } else {
                    renderText(options);
                }
            });
    
            function renderText(options, measure = false) {
                const { string = '', color = '#000', font: { family = "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif", size = 12, style = 'normal', weight = 'normal', lineHeight = 1 } = {} } = options;
                ctx.save();
                ctx.font = `${style} ${weight} ${size}px ${family}`;
                ctx.fillStyle = color;
                ctx.textAlign = inline ? 'left' : 'center';
                ctx.textBaseLine = 'middle';
    
                let text = toChartValue(string, chart.data);
    
                if (inline) {
                    let m = ctx.measureText(text);
                    _y += m?.[va] ?? 0;
                    measure && (maxSize = Math.max(maxSize, m.actualBoundingBoxAscent));
                    !measure && ctx.fillText(text, _x, _y);
                    _x += m.width;
                    _y = y;
                } else {
                    _y += lineHeight * size;
                    !measure && ctx.fillText(text, _x, _y);
                }
                ctx.restore();
            }
        },
    },
    htmlLegend: {
        id: "htmlLegend",
        afterUpdate(chart, args, { selector, percentage = false, value = false, statistics = false, length = false }) {
            const container = (() => {
                if (typeof selector === "string") {
                    return document.querySelector(selector);
                }
                if (selector instanceof HTMLElement) {
                    return selector;
                }
                return chart.canvas.parentElement;
            })();
            const legend = getOrCreateElement(container, ".legend-container");

            // Reuse the built-in legendItems generator
            const data = chart.data?.datasets?.[0]?.data;
            if (!data) {
                return;
            }
            const total = data.reduce((t, e) => t + e, 0);
            const items = chart.options.plugins.legend.labels.generateLabels(chart);
            const ul = getOrCreateElement(legend, "ul.legend-catalog");
            ul.replaceChildren();

            if (statistics) {
                const { value, unit } = statistics;
                const e = getOrCreateElement(legend, ".legend-statistics");
                if (value != void 0) {
                    const v = getOrCreateElement(e, "span.value");
                    v.textContent = toChartValue(value, data);
                }
                if (unit != void 0) {
                    const u = getOrCreateElement(e, "span.unit");
                    u.textContent = toChartValue(unit);
                }
            }

            if (length) items.length = length;
            items.forEach((item) => {
                const li = document.createElement("li");

                li.onclick = () => {
                    const { type } = chart.config;
                    if (type === "pie" || type === "doughnut") {
                        // Pie and doughnut charts only have a single dataset and visibility is per item
                        chart.toggleDataVisibility(item.index);
                    } else {
                        chart.setDatasetVisibility(item.datasetIndex, !chart.isDatasetVisible(item.datasetIndex));
                    }
                    chart.update();
                };

                li.style.opacity = item.hidden ? 0.4 : 1;

                // Color box
                const boxSpan = document.createElement("span");
                boxSpan.className = "box";
                boxSpan.style.background = item.fillStyle;
                boxSpan.style.borderColor = item.strokeStyle;
                li.appendChild(boxSpan);

                // Text
                const textContainer = document.createElement("p");
                textContainer.className = "label";
                textContainer.textContent = item.text;
                li.appendChild(textContainer);

                // Percentage
                if (value) {
                    const output = typeof value == "function" ? value(data[item.index]) : data[item.index];
                    const valueDiv = document.createElement("div");
                    valueDiv.className = "value";
                    valueDiv.textContent = output;
                    li.appendChild(valueDiv);
                }

                if (percentage) {
                    const p = Math.floor((data[item.index] / total) * 100) || 0;
                    const output = typeof percentage == "function" ? percentage(p) : p + "%";
                    const percentageDiv = document.createElement("div");
                    percentageDiv.className = "percentage";
                    percentageDiv.textContent = output;
                    li.appendChild(percentageDiv);
                }

                ul.appendChild(li);
            });
        },
    },
    emptyDoughnut: {
        id: "emptyDoughnut",
        afterDraw(chart, args, options) {
            const { datasets } = chart.data;
            const {
                color,
                backgroundColor = 'rgba(0, 0, 0, 0)',
                borderColor,
                width = 2,
                radiusDecrease = 1,
                string = "無資料",
                font: {
                    family = 'Noto Sans TC, sans-serif',
                    weight = 500,
                    size = 16,
                } = {}
            } = options;
            if (!hasDataInDatasets(datasets)) {
                const { chartArea: { left, top, right, bottom }, ctx } = chart;
                const centerX = (left + right) / 2;
                const centerY = (top + bottom) / 2;
                const r = Math.min(right - left, bottom - top) / 2;
    
                let textColor = Chart.helpers.color(color)
                if (!textColor.valid) {
                    textColor = Chart.helpers.color(backgroundColor)
                    if (textColor.rgb.a !== 0) {
                        textColor.negate().greyscale().alpha(1)
                    }
                    else {
                        let container = chart.canvas
                        textColor = Chart.helpers.color(getComputedStyle(container).backgroundColor)
                        while (textColor.rgb.a == 0) {
                            container = container.parentElement
                            textColor = Chart.helpers.color(getComputedStyle(container).backgroundColor)
                        }
                        textColor.negate().greyscale().alpha(1)
                    }
                }
    
                ctx.beginPath();
                ctx.lineWidth = width;
                ctx.strokeStyle = borderColor || textColor.clone().alpha(0.5).rgbString();
                ctx.arc(centerX, centerY, (r - radiusDecrease), 0, 2 * Math.PI);
                ctx.fillStyle = backgroundColor
                ctx.fill();
                ctx.stroke();
    
                chartPlugins.centerText.beforeDatasetsDraw(chart, args, {
                    text: [
                        {
                            string,
                            color: textColor.rgbString(),
                            font: { family, weight, size }
                        }
                    ],
                    drawWhenEmpty: true
                })
            }
        }
    },
};

// #endregion

// #region chart create helper function
function calcLegendData(chart) {
    const _items = chart.options.plugins.legend.labels.generateLabels(chart);
    const data = chart.data?.datasets?.[0]?.data;
    if (!data) {
        return { total: 0, items: [] };
    }
    const total = data.reduce((t, e) => t + e, 0);
    const items = _items.map((item) => {
        return {
            label: item.text,
            value: data[item.index],
            percentage: Math.floor((data[item.index] / total) * 100) || 0,
            color: item.fillStyle,
            hidden: getHidden(),
            onClick: () => {
                const { type } = chart.config;
                if (type === "pie" || type === "doughnut") {
                    // Pie and doughnut charts only have a single dataset and visibility is per item
                    chart.toggleDataVisibility(item.index);
                } else {
                    chart.setDatasetVisibility(item.datasetIndex, !chart.isDatasetVisible(item.datasetIndex));
                }
                chart.update();
            },
        };

        function getHidden() {
            const { type } = chart.config;
            if (type === "pie" || type === "doughnut") {
                return !chart.getDataVisibility(item.index);
            } else {
                return !chart.isDatasetVisible(item.datasetIndex);
            }
        }
    });
    return {
        total,
        items,
    };
}
function getOrCreateElement(container, selector, tag = null) {
    let element = container.querySelector(selector);
    if (element) {
        return element;
    }
    if (tag == null) {
        tag = selector.match(/^[^.|#][a-zA-Z0-9_-]+/g)?.[0] ?? "div";
    }
    element = document.createElement(tag);
    selector.match(/[.][a-zA-Z0-9_-]+/g)?.forEach((className) => {
        element.classList.add(className.substring(1));
    });
    let id = selector.match(/#[a-zA-Z0-9_-]+/g)?.[0].substring(1);
    if (id) {
        element.id = id;
    }

    container.appendChild(element);
    return element;
}
function hasDataInDatasets(datasets) {
    let hasData = false;
    for (let i = 0; i < datasets.length; i++) {
        const data = datasets[i].data;
        //hasData |= dataset.data.length > 0;
        for (let j = 0; j < data.length; j++) {
            //console.log(chart.canvas.parentNode.id, data[j], data[j] > 0)
            hasData |= data[j] > 0;
        }
    }
    return hasData;
}
function toChartValue(ref, ...args) {
    if (typeof ref === "function") {
        return ref(...args);
    }
    return ref;
}
// #endregion
