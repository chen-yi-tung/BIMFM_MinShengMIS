'use strict';
async function searchWidget(view, options = {}, position = "top-left", index = -1) {
    await loadArcgis("esri/widgets/Search")
    const component = new Arcgis.Search({ view, ...options });
    const expand = await Arcgis.customWidgetUtils.expandWidget(view, component, position, { id: "search" })
    view.ui.add({ component: expand, position, index });
    return component;
}