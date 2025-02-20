(() => {
    class Loading extends Autodesk.Viewing.Extension {
        constructor(viewer, options) {
            super(viewer, options);
            this.name = "Viewer.Loading";
            this.container = viewer.container;
            this.container.parentElement.insertAdjacentHTML("afterbegin", options.loader);
            this.loader = viewer.container.parentElement.firstElementChild;
            this.loader.classList.add("fade");
            this.show = () => {
                this.loader.classList.add("show");
                this.container.style.visibility = "hidden";
                this.loader.dispatchEvent(new CustomEvent("show"));
            };
            this.hide = () => {
                this.loader.classList.remove("show");
                this.container.style.visibility = "visible";
                this.loader.dispatchEvent(new CustomEvent("hide"));
            };
        }
        get loading() {
            return this.loader.classList.contains("show");
        }
        load() {
            delete this.activate;
            delete this.activeStatus;
            delete this.deactivate;
            delete this.mode;
            delete this.modes;
            this.viewer.loading = this;
            this.show();
            return this.viewer && !0;
        }
        unload() {
            this.loader.remove();
            this.loader = null;
            return true;
        }
        addEventListener(type, listener) {
            this.loader.addEventListener(type, listener);
        }
    }
    Autodesk.Viewing.theExtensionManager.registerExtension("Viewer.Loading", Loading);
})();