'use strict';
/**
 * @typedef selectWidgetOptions
 * @property {Arcgis.SceneLayer} targetLayer
 * @property {{label:string, color:string}[]} statisticsList
 * @property {string} statisticsField
 * @property {Arcgis.FeatureTable} featureTable
 * 
 * @returns {Arcgis.Expand}
 */
async function selectWidget(view, options = {}, position = "top-left", index = -1) {
    await loadArcgis([
        "esri/rest/support/Query",
        "esri/widgets/Expand",
        "esri/widgets/Sketch/SketchViewModel",
        "esri/geometry/geometryEngineAsync",
        "esri/layers/GraphicsLayer"
    ])

    let highlightHandle = null;
    let selectResult = null;
    const graphicsLayer = new Arcgis.GraphicsLayer({ title: "nopopup", listMode: "hide" });
    view.map.add(graphicsLayer);

    const polygonSymbol = {
        type: "simple-fill",
        style: "backward-diagonal",
        outline: { cap: "round", join: "round", width: 1, color: [0, 0, 0, 0.4] },
        color: [0, 0, 0, 0.2]
    };

    const sketchViewModel = new Arcgis.SketchViewModel({
        view,
        layer: graphicsLayer,
        polygonSymbol: polygonSymbol,
        updateOnGraphicClick: false
    });

    sketchViewModel.on("create", async (event) => {
        if (event.state === "complete") {
            // this polygon will be used to query features that intersect it
            const geometries = graphicsLayer.graphics.map((graphic) => graphic.geometry);
            const queryGeometry = await Arcgis.geometryEngineAsync.union(geometries.toArray());

            expand.statisticsList && selectFeatures(queryGeometry);
        }
    });

    view.on("click", async (event) => {
        if (!$("#select-by-rectangle").hasClass("active")) return;
        let test = await view.hitTest(event);
        const graphicHits = test.results.find((hitResult) => hitResult.type === "graphic" && hitResult.graphic.layer.id === graphicsLayer.id);
        if (graphicHits) {
            const geometries = graphicsLayer.graphics.map((graphic) => graphic.geometry);
            const queryGeometry = await Arcgis.geometryEngineAsync.union(geometries.toArray());

            expand.statisticsList && selectFeatures(queryGeometry);
        }
    })

    const node = document.createElement("div");
    node.classList.add("p-2", "bg-light");
    node.appendChild(Arcgis.customWidgetUtils.customWidgetButton({
        id: "select-by-rectangle",
        icon: "esri-icon-cursor-marquee",
        title: "框擇標記點",
        onClick: (e) => {
            Arcgis.customWidgetUtils.activeButtonEvent(view, e.target, () => {
                Arcgis.customWidgetUtils.activeWidget = e.target
                view.closePopup();
                sketchViewModel.create("rectangle");
            }, () => {
                clearSelection(false);
            })
        }
    }))
    node.appendChild(Arcgis.customWidgetUtils.customWidgetButton({
        id: "clear-selection",
        icon: "esri-icon-erase",
        title: "清除選擇",
        onClick: (e) => {
            clearSelection(false);
            if (node.querySelector("#select-by-rectangle").classList.contains('active')) {
                view.closePopup();
                sketchViewModel.create("rectangle");
            }
        }
    }))

    const expand = new Arcgis.Expand({
        view: view,
        id: "select",
        content: node,
        expandIconClass: "esri-icon-cursor-marquee",
        expandTooltip: "框選標記點",
        mode: "floating",
        group: "1",
        ...options
    });
    expand.when(() => {
        expand.container.addEventListener("click", (e) => { if (!expand.expanded) { clearSelection(true); } })
    });

    view.ui.add({ component: expand, position, index });
    return expand;

    async function selectFeatures(geometry) {
        const { featureTable, targetLayer: layer, statisticsList, statisticsField } = expand
        let typeList = statisticsList.map(e => e.label);
        let colorList = statisticsList.map(e => e.color);

        clearHighlight();

        const layerView = await view.whenLayerView(layer);

        if (!selectResult) {
            selectResult = await Promise.all([
                layer.queryFeatures(new Arcgis.Query({
                    geometry: geometry,
                    outFields: ["objectid"]
                })),
                layerView.queryFeatures(new Arcgis.Query({
                    geometry: geometry,
                    outStatistics: [
                        {
                            onStatisticField: "objectid",
                            outStatisticFieldName: "count",
                            statisticType: "count"
                        },
                        ...createStatistics(typeList)
                    ]
                }))
            ])
        }
        let [q, results] = selectResult;

        function createStatistics(list) {
            return list.map((li, i) => {
                if (li == "其他") {
                    return {
                        onStatisticField: `CASE WHEN ${statisticsField} IN (${list.map(e => `N'${e}'`).join(", ")}) THEN 0 ELSE 1 END`,
                        outStatisticFieldName: "name_" + i,
                        statisticType: "sum"
                    }
                }
                return {
                    onStatisticField: `CASE WHEN (${statisticsField} = N'${li}') THEN 1 ELSE 0 END`,
                    outStatisticFieldName: "name_" + i,
                    statisticType: "sum"
                }
            })
        }

        highlightHandle = layerView.highlight(q.features.map(e => e.attributes.objectid));

        view.openPopup({
            location: geometry.extent.center,
            title: "統計資訊",
            content: (() => {
                const data = results.features[0].attributes;
                const node = document.createElement("div");

                node.innerHTML = `<strong>總數：${data.count}</strong>`;

                const canvas = document.createElement("canvas");
                canvas.height = 300;
                canvas.style.margin = "auto";
                node.append(canvas);

                new Chart(canvas.getContext("2d"), {
                    type: "doughnut",
                    data: {
                        labels: typeList,
                        datasets: [
                            {
                                backgroundColor: colorList,
                                borderWidth: 0,
                                data: typeList.map((e, i) => data["name_" + i])
                            }
                        ]
                    },
                    options: {
                        responsive: false,
                        cutoutPercentage: 35,
                        legend: {
                            position: "bottom",
                            labels: {
                                boxWidth: 12,
                            }
                        },
                        title: {
                            display: true,
                            text: "分類"
                        }
                    }
                });
                return node;
            })()
        });

        if (featureTable && !(
            featureTable.layer?.id == layer.id &&
            featureTable.filterGeometry == geometry &&
            featureTable.visible == true
        )) {
            featureTable.layer = layer;
            setTimeout(() => {
                featureTable.filterGeometry = geometry;
            }, 300);
            view.ui.add(featureTable, "bottom-right");
        }
    }

    function clearSelection(clearActive = false) {
        const { featureTable } = expand
        clearHighlight();
        sketchViewModel.cancel();
        graphicsLayer.removeAll();
        if (featureTable) {
            view.ui.remove(featureTable);
            featureTable.filterGeometry = null;
        }
        selectResult = null;
        clearActive && Arcgis.customWidgetUtils.activeButtonEvent(view, null);
    }

    function clearHighlight() {
        if (highlightHandle) {
            highlightHandle.remove();
            highlightHandle = null;
        }
    }
}