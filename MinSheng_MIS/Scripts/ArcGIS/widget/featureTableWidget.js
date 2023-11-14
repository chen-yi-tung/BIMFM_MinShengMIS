async function featureTableWidget(view, options = {}, position = "top-left", index = -1) {
    await loadArcgis([
        "esri/core/reactiveUtils",
        "esri/rest/support/Query",
        "esri/widgets/FeatureTable"
    ])
    document.body.insertAdjacentHTML('beforeend', temp())
    const container = document.querySelector("#FeatureTable");
    const widget = new Arcgis.FeatureTable({
        view,
        container,
        multiSortEnabled: true,
        highlightEnabled: true,
        visibleElements: {
            header: true,
            menu: true,
            selectionColumn: true,
            columnMenus: false, //4.23
            menuItems: {
                clearSelection: true,
                refreshData: true,
                toggleColumns: true,
                selectedRecordsShowAllToggle: true, //4.23
                showSelectedToggle: true, //4.23
                zoomToSelection: true //4.23
            },
        },
        ...options
    });
    const expand = await Arcgis.customWidgetUtils
        .customWidgetButton({
            id: "featureTableButton",
            icon: "esri-icon-table",
            title: "模型詳細資料",
            onClick: (e) => {
                Arcgis.customWidgetUtils.activeButtonEvent(view, e.target, () => {
                    Arcgis.customWidgetUtils.activeWidget = widget;
                    view.ui.add(Arcgis.customWidgetUtils.activeWidget, "bottom-right");
                })
            }
        })
    widget.when(async () => {
        //view.ui.add(widget, "bottom-right");
        //view.ui.remove(widget);

        let node = widget.container.querySelector(".layer-data-select-area");
        let select = node.querySelector("#SelectLayerData");
        let search = node.querySelector("#SearchLayerData");
        let allCount = node.querySelector("#all-count");
        let selectCount = node.querySelector("#select-count");
        node.remove();
        node.removeAttribute("style");
        widget.container.querySelector(".esri-feature-table__title").remove();
        widget.container.querySelector(".esri-feature-table__header").append(node);
        widget.layer && (allCount.innerText = await widget.layer.queryFeatureCount?.() ?? NaN)

        view.when(() => {
            //select.innerHTML = "";
            view.map?.layers?.forEach?.((layer) => {
                //console.log(layer.title, layer.listMode)
                if (layer.listMode == "show") {
                    let option = document.createElement("option")
                    option.value = layer.id;
                    option.innerText = layer.title;
                    select.append(option);
                }
            })
        })

        select.addEventListener('change', async function (e) {
            widget.layer = view.map?.layers?.find?.((layer) => layer.id == this.value);
        })

        let isSearch = false;
        search.querySelector("input[type=text]").addEventListener('change', onSearch);
        search.querySelector("button").addEventListener('click', onSearch);
        async function onSearch() {
            widget.highlightIds.removeAll();
            widget.clearSelectionFilter();
            let text = search.querySelector("input[type=text]")?.value?.trim();
            let isString = true;

            if (text == "") {
                allCount.innerText = await widget.layer.queryFeatureCount();
                return;
            }
            if (!isNaN(text)) {
                text = parseInt(text)
                isString = false
            }

            isSearch = true;
            let oidFieldName = widget.layer.fields.find(f => f.type == "oid").name;
            let where = widget.layer.fields.reduce((total, f) => {
                if (f.type == "oid") { return total; }
                switch (f.type) {
                    case "small-integer":
                        !isString && total.push(`CAST(${f.name} AS varchar(4)) LIKE N'%${text}%'`);
                        break;
                    case "integer":
                        !isString && total.push(`CAST(${f.name} AS varchar(11)) LIKE N'%${text}%'`);
                        break;
                    case "single":
                        !isString && total.push(`CAST(${f.name} AS varchar(8)) LIKE N'%${text}%'`);
                        break;
                    case "double":
                        !isString && total.push(`CAST(${f.name} AS varchar(16)) LIKE N'%${text}%'`);
                        break;
                    case "long":
                        !isString && total.push(`CAST(${f.name} AS varchar(21)) LIKE N'%${text}%'`);
                        break;
                    default:
                        total.push(`${f.name} LIKE N'%${text}%'`);
                        break;
                }
                return total;
            }, []).join(" OR ");
            let q;
            if (where !== "") {
                if (widget.layer.type == "scene") {
                    q = await widget.layer?.associatedLayer?.queryFeatures?.(new Arcgis.Query({ outFields: [oidFieldName], where }))
                }
                else {
                    q = await widget.layer?.queryFeatures?.(new Arcgis.Query({ outFields: [oidFieldName], where }))
                }
            }

            if (!q) {
                console.log("無法搜尋")
                return
            }
            console.log(where)
            let resultOids = q.features.map(f => f.attributes[oidFieldName])
            widget.highlightIds.push(...resultOids);
            allCount.innerText = resultOids.length;
            widget.filterBySelection();
            widget.highlightIds.removeAll();
            isSearch = false;
        }

        const handleChangeLayer = Arcgis.reactiveUtils.watch(
            () => widget.layer,
            async (layer) => {
                if (!layer) { return }
                console.log(`current ft layer : ${layer.title}`);
                select.value = layer.id;
                search.querySelector("input[type=text]").value = "";
                widget.clearSelectionFilter();
                widget.filterGeometry = null;
                widget.tableTemplate = layer.tableTemplate ?? null;
                //layer.loaded && widget.refresh();
                widget.scrollToIndex(0);
                widget.highlightIds.removeAll();
                allCount.innerText = await widget.layer.queryFeatureCount?.() ?? NaN;
            }
        );
        const handleChangeFilterGeometry = Arcgis.reactiveUtils.watch(
            () => widget.filterGeometry,
            async (geometry) => {
                if (geometry != null) {
                    allCount.innerText = await widget.layer?.queryFeatureCount?.(new Arcgis.Query({
                        geometry: geometry
                    }));
                }
                else {
                    allCount.innerText = await widget.layer?.queryFeatureCount?.() ?? NaN;
                }
                console.log(`current ft all count : ${allCount.innerText}`);
            },
            { initial: true }
        );

        widget.addHandles(handleChangeLayer);
        widget.addHandles(handleChangeFilterGeometry);

        widget.highlightIds.on("before-add", async (event) => {
            if (isSearch) return;
            selectCount.innerText = widget.highlightIds.length;
            widget.highlightIds.removeAll();
            view.closePopup();


            if (event.item) {
                let objectId = event.item;
                if (widget.layer.type == "scene") {
                    Arcgis.sceneLayerUtils.fitByObjectId({ view, layer: widget.layer, objectId });
                }
                else {
                    let q = await widget.layer.queryFeatures(new Arcgis.Query({
                        outFields: ['*'],
                        returnGeometry: true,
                        outSpatialReference: view.spatialReference,
                        objectIds: [objectId],
                    }));
                    view.goTo(q.features[0].geometry).then(() => {
                        view.openPopup({
                            features: q.features,
                            updateLocationEnabled: true
                        })
                    })
                }
            }
        });
    })
    view.ui.add({ component: expand, position, index });
    return widget;

    function temp() {
        return `
        <div class="feature-table" id="FeatureTable">
            <div class="layer-data-select-area" style="display:none;">
                <div class="form-group">
                    <label for="SelectLayerData">選擇圖層</label>
                    <select class="esri-select form-select" id="SelectLayerData">
                        <option value="" selected disabled hidden>請選擇</option>
                    </select>
                </div>
                <div id="SearchLayerData" class="esri-search esri-widget">
                    <div tabindex="-1" class="esri-search__container">
                        <div class="esri-search__input-container">
                            <input type="text" aria-autocomplete="list" aria-expanded="false"
                                aria-controls="SearchLayerData-suggest-menu" aria-haspopup="listbox" aria-label="搜尋"
                                placeholder="搜尋" autocomplete="off" class="esri-input esri-search__input"
                                id="SearchLayerData-input" role="combobox" data-node-ref="_inputNode" title="搜尋">
                        </div>
                        <button aria-label="搜尋" class="esri-search__submit-button esri-widget--button" title="搜尋" type="button">
                            <span aria-hidden="true" class="esri-icon-search"></span>
                        </button>
                    </div>
                </div>

                <div class="text-end">
                    已選擇 <span id="select-count">0</span> / <span id="all-count">NaN</span>
                </div>
            </div>
        </div>`
    }
}