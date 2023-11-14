async function measureExpandWidget(view, position = "top-left", index = -1) {
    await loadArcgis([
        "esri/widgets/DirectLineMeasurement3D",
        "esri/widgets/AreaMeasurement3D",
        "esri/widgets/Slice",
        "esri/core/promiseUtils",
    ])
    document.body.insertAdjacentHTML('beforeend', temp())
    const node = document.querySelector("#MeasureTool");

    const lineWidget = LineWidget();
    const areaWidget = AreaWidget();
    const sliceWidget = SliceWidget();

    const expand = Arcgis.customWidgetUtils.customWidgetButton({
        id: "measure",
        icon: "esri-icon-measure",
        title: "測量",
        onClick: (e) => {
            Arcgis.customWidgetUtils.activeButtonEvent(view, e.target, () => {
                Arcgis.customWidgetUtils.activeWidget = node;
                view.ui.add(Arcgis.customWidgetUtils.activeWidget, "bottom-right");
                view.hitTestEnable = false;
            }, () => {
                view.hitTestEnable = true;
            })
        }
    });
    expand.setup = function () {
        node.removeAttribute('style');
        lineWidget.setup();
        expand.removeEventListener("click", expand.setup);

        expand.addEventListener("click", () => {
            let active = expand.classList.contains("active");

            lineWidget.visible = active;
            areaWidget.visible = active;
            sliceWidget.visible = active;
        });
    }
    node.destroy = function () {
        lineWidget.visible = false;
        areaWidget.visible = false;
        sliceWidget.visible = false;
    }
    view.ui.add({ component: expand, position, index });
    expand.addEventListener("click", expand.setup);

    return expand;

    function LineWidget() {
        const tab = document.querySelector("#measure-line-tab");
        const container = document.querySelector("#measure-line");
        const w = new Arcgis.DirectLineMeasurement3D({ view, container });
        const vm = w.viewModel;
        w.setup = function () {
            areaWidget.cancel();
            sliceWidget.cancel();
            w.visible = true;
            if (vm.state != "measured") {
                vm.start().catch((error) => {
                    if (Arcgis.promiseUtils.isAbortError(error)) { return; }
                    throw error;
                });
            }
        }
        w.cancel = function () {
            w.visible = vm.state == "measured";
        }
        w.tab = tab;
        tab.addEventListener("click", w.setup);
        return w;
    }

    function AreaWidget() {
        const tab = document.querySelector("#measure-area-tab");
        const container = document.querySelector("#measure-area");
        const w = new Arcgis.AreaMeasurement3D({ view, container });
        const vm = w.viewModel;
        w.setup = function () {
            lineWidget.cancel();
            sliceWidget.cancel();
            w.visible = true;
            if (vm.state != "measured") {
                vm.start().catch((error) => {
                    if (Arcgis.promiseUtils.isAbortError(error)) { return; }
                    throw error;
                });
            }
        }
        w.cancel = function () {
            w.visible = vm.state == "measured";
        }
        w.tab = tab;
        tab.addEventListener("click", w.setup);
        return w;
    }

    function SliceWidget() {
        const tab = document.querySelector("#measure-slice-tab");
        const container = document.querySelector("#measure-slice");
        const w = new Arcgis.Slice({ view, container })
        const vm = w.viewModel;
        vm.tiltEnabled = true;
        vm.excludeGroundSurface = false;
        w.setup = function () {
            lineWidget.cancel();
            areaWidget.cancel();
            w.visible = true;
            if (vm.state != "sliced") {
                vm.start().catch((error) => {
                    if (Arcgis.promiseUtils.isAbortError(error)) { return; }
                    throw error;
                });
            }
        }
        w.cancel = function () {
            w.visible = vm.state == "sliced";
        }
        w.tab = tab;
        tab.addEventListener("click", w.setup);
        return w;
    }
    function temp() {
        return `<div class="measure-tool" style="display:none;" id="MeasureTool">
            <ul class="nav nav-tabs" id="MeasureTab" role="tablist">
                <li class="nav-item" role="presentation">
                    <button class="nav-link active" id="measure-line-tab" data-bs-toggle="tab" data-bs-target="#measure-line"
                        type="button" role="tab" aria-controls="measure-line" aria-selected="true">
                        <span aria-hidden="true" class="esri-icon esri-icon-measure-line pe-none"></span>
                    </button>
                </li>
                <li class="nav-item" role="presentation">
                    <button class="nav-link" id="measure-area-tab" data-bs-toggle="tab" data-bs-target="#measure-area"
                        type="button" role="tab" aria-controls="measure-area" aria-selected="false">
                        <span aria-hidden="true" class="esri-icon esri-icon-measure-area pe-none"></span>
                    </button>
                </li>
                <li class="nav-item" role="presentation">
                    <button class="nav-link" id="measure-slice-tab" data-bs-toggle="tab" data-bs-target="#measure-slice" type="button"
                        role="tab" aria-controls="measure-slice" aria-selected="false">
                        <span aria-hidden="true" class="esri-icon esri-icon-slice pe-none"></span>
                    </button>
                </li>
            </ul>
            <div class="tab-content" id="MeasureTabContent">
                <div class="tab-pane show active" id="measure-line" role="tabpanel" aria-labelledby="measure-line-tab"></div>
                <div class="tab-pane" id="measure-area" role="tabpanel" aria-labelledby="measure-area-tab"></div>
                <div class="tab-pane" id="measure-slice" role="tabpanel" aria-labelledby="measure-slice-tab"></div>
            </div>
        </div>`
    }
}