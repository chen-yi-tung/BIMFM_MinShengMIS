'use strict';
async function elevationProfileWidget(view, options = {}, position = "top-left", index = -1) {
    await loadArcgis("esri/widgets/ElevationProfile")
    const component = new Arcgis.ElevationProfile({
        view,
        profiles: [{ type: "ground" }, { type: "view" }],
        visibleElements: { selectButton: false },
        ...options
    });
    const expand = await Arcgis.customWidgetUtils.expandWidget(view, component, position, { id: "elevationProfile" })
    view.ui.add({ component: expand, position, index });
    return [component, expand];
}