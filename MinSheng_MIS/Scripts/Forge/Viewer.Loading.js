AutodeskNamespace("Autodesk.Viewer.Extension");
Autodesk.Viewer.Extension.Loading = function (viewer, options) {
    Autodesk.Viewing.Extension.call(this, viewer, options);
    this.name = "Viewer.Loading";
    this.clientContainer = viewer.clientContainer
    this.clientContainer.parentElement.insertAdjacentHTML('afterbegin', options.loader);
    this.loader = viewer.clientContainer.parentElement.firstElementChild;
    this.loader.classList.add('fade')
    this.show = function () {
        this.loader.classList.add('show')
        this.clientContainer.style.visibility = 'hidden'
    }
    this.hide = function () {
        this.loader.classList.remove('show')
        this.clientContainer.style.visibility = 'visible'
    }
}
Autodesk.Viewer.Extension.Loading.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
Autodesk.Viewer.Extension.Loading.prototype.constructor = Autodesk.Viewer.Extension.Loading;
Autodesk.Viewer.Extension.Loading.prototype.load = function () {
    delete this.activate;
    delete this.activeStatus;
    delete this.deactivate;
    delete this.mode;
    delete this.modes;
    this.viewer.loading = this;
    this.show();
    return this.viewer && !0;
};
Autodesk.Viewer.Extension.Loading.prototype.unload = function () {
    this.loader.remove()
    this.loader = null
    return true;
}
Autodesk.Viewing.theExtensionManager.registerExtension('Viewer.Loading', Autodesk.Viewer.Extension.Loading);

/*var LoadingOptions = {
    loader: `<div class="lds-default"><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div></div>`,
}*/