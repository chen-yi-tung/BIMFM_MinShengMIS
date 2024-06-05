'use strict';
async function daylightWidget(view, options = {}, position = "top-left", index = -1) {
    await loadArcgis("esri/widgets/Daylight")
    const component = new Arcgis.Daylight({
        view,
        playSpeedMultiplier: 2,
        visibleElements: { timezone: false },
        ...options
    });
    const expand = await Arcgis.customWidgetUtils.expandWidget(view, component, position, { id: "daylight", mode: "auto" })
    view.ui.add({ component: expand, position, index });
    return [component, expand];
}