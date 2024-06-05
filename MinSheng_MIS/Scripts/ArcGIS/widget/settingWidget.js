'use strict';
async function settingWidget(view, options = {}, position = "top-left", index = -1) {
    await loadArcgis([
        "esri/widgets/Expand",
        "esri/widgets/Slider",
    ])
    const settingList = []
    const node = temp()
    if (options.i3sLayer) {
        const i3s_model = addSwitch({
            id: "i3s_model",
            label: "周邊模型",
            index: 0,
            setup: false,
            onChange: (checked = false) => {
                options.i3sLayer.visible = checked;
                checked ?
                    i3s_model_texture.input.removeAttribute("disabled") :
                    i3s_model_texture.input.setAttribute("disabled", "")
            }
        })
        const i3s_model_texture = addSwitch({
            id: "i3s_model_texture",
            label: "周邊模型紋理",
            index: 1,
            setup: false,
            onChange: (checked = false) => {
                checked ? (options.i3sLayer.renderer = null) : (options.i3sLayer.renderer = {
                    type: "simple", symbol: {
                        type: "mesh-3d",
                        symbolLayers: [{ type: "fill", material: { color: [255, 255, 255, 1], colorMixMode: 'replace' } }]
                    }
                })
            }
        })
        i3s_model.onChange()
        i3s_model_texture.onChange()
    }
    addHr()
    const opacitySlider = addSlider({
        id: "map-ground-opacity",
        label: "地圖不透明度",
        options: { values: [0.7] },
        onChange: (value) => { view.map.ground.opacity = value }
    })

    const expand = new Arcgis.Expand({
        view: view,
        id: "setting",
        content: node,
        expandIconClass: "esri-icon-settings",
        expandTooltip: "設定",
        group: "1",
        mode: "floating"
    });
    expand.when(() => {
        node.removeAttribute('style');
    })
    view.ui.add({ component: expand, position, index });

    expand.addSwitch = addSwitch
    expand.addSlider = addSlider
    expand.addHr = addHr
    expand.getSetting = get
    expand.getSettingValue = function (id) {
        let res = get(id)
        switch (res?.type) {
            case "switch":
                return res.input.checked
            case "slider":
                return res.widget.values[0]
            default:
                return undefined
        }
    }
    expand.setSettingValue = function (id, value) {
        let res = get(id)
        switch (res?.type) {
            case "switch":
                res.input.checked = value
                return res.onChange(res.input.checked)
            case "slider":
                res.widget.values[0] = value
                return res.onChange(res.widget.values[0])
            default:
                return false
        }
    }
    return expand;

    function temp() {
        let node = document.createElement("div")
        node.classList.add("setting-list")
        node.style.display = "none"
        node.id = "Setting"
        return node
    }
    function addSwitch({ id, label = "", index = -1, onChange = () => { }, defaultChecked = false, setup = true }) {
        const container = document.createElement("div")
        container.className = "form-check form-switch"
        container.insertAdjacentHTML('beforeend',
            `<label class="form-check-label" for="${id}">${label}</label>
            <input class="form-check-input" type="checkbox" role="switch" id="${id}" ${defaultChecked ? "checked" : ""}>`)
        const input = container.querySelector("#" + id)
        view.when(() => {
            setup && onChange(input.checked);
            input.onchange = function () { onChange(this.checked) }
        });
        return add({ container, input, type: "switch", onChange }, index)
    }
    function addSlider({ id, label = "", index = -1, options = {}, onChange = () => { }, setup = true }) {
        const container = document.createElement("div")
        container.classList.add("setting-slider-area")
        container.insertAdjacentHTML('beforeend', `${label} <div class="setting-slider" id="${id}"></div>`)
        const input = container.querySelector("#" + id)
        const widget = new Arcgis.Slider({
            container: input,
            min: 0,
            max: 1,
            values: [1],
            steps: 0.01,
            snapOnClickEnabled: true,
            visibleElements: {
                labels: false,
                rangeLabels: false
            },
            ...options
        });
        widget.when(() => {
            setup && onChange(widget.values[0])
            widget.on("thumb-drag", (event) => { onChange(event.value) });
        })
        return add({ container, input, widget, type: "slider", onChange }, index)
    }
    function addHr({ index = -1 } = {}) {
        return add(document.createElement("hr"), index)
    }
    function add(e, index = -1) {
        if (index === -1 || index === settingList.length - 1) {
            settingList.push(e)
            node.appendChild(e?.container ?? e)
        }
        else {
            if (index < 0) {
                settingList.splice(index + 1, 0, e)
                node.insertBefore(e?.container ?? e, node.children[index + 1])
            }
            else {
                settingList.splice(index, 0, e)
                node.insertBefore(e?.container ?? e, node.children[index])
            }
        }
        return e
    }
    function get(id) {
        return settingList.find(e => {
            return e?.input?.id === id
        })
    }
}