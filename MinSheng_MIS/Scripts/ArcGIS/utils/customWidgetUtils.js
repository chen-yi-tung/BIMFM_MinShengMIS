'use strict';
(function (A) {
    Object.defineProperty(A, "customWidgetUtils", {
        value: {
            /**
             * @typedef customWidgetOptions
             * @property {string} id
             * @property {string} icon
             * @property {string} title
             * @property {Function} onClick
             * 
             * @param {customWidgetOptions} options 
             * @returns {HTMLElement}
             */
            customWidgetButton(options) {
                let node = document.createElement("div");
                options.id && (node.id = options.id);
                node.setAttribute('type', 'button');
                node.setAttribute('title', options.title ?? '');
                node.classList.add("esri-widget--button", "esri-widget", "esri-interactive");
                node.innerHTML = `<span aria-hidden="true" role="presentation" class="esri-icon ${options.icon ?? ''} pe-none"></span>`;
                options.onClick && node.addEventListener('click', options.onClick)
                return node;
            },
            activeButtonEvent(view, target, callback = () => { }, cancelback = () => { }) {
                //console.log("activeButtonEvent",this.activeWidget)
                if (this.activeWidget) {
                    view.ui.remove(this.activeWidget);
                    if (this.activeWidget?.id == "FeatureTable") {
                        this.activeWidget = null;
                    }
                    else {
                        this.activeWidget?.destroy?.();
                        this.activeWidget = null;
                    }
                }
                if (target && !target.classList.contains("active")) {
                    callback();
                    this.setActiveButton(target);
                    return;
                }
                this.setActiveButton(null);
                cancelback();
            },
            setActiveButton(selected) {
                // focus the view to activate keyboard shortcuts for sketching
                //view.focus();
                const elems = document.querySelectorAll(".esri-widget--button.active");
                for (let i = 0; i < elems.length; i++) {
                    elems[i].classList.remove("active");
                }
                if (selected) {
                    selected.classList.add("active");
                }
            },
            async expandWidget(view, content, position = "top-left", expandOption = {}) {
                await loadArcgis("esri/widgets/Expand")
                const expand = new Arcgis.Expand(Object.assign({ view, content, group: "1", mode: "floating" }, expandOption));
                content.when(() => { expand.expandTooltip = content.label; })
                view.ui.add(expand, position);
                return expand;
            }
        }
    })
    A.customWidgetUtils.activeWidget = null;
})(window.Arcgis)
