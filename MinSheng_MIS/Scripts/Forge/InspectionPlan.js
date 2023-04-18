var view = document.querySelector("#PathCanvas");
var app;
var stage;

const sortRouteModal = new SortRouteModal({ onSave: updatePathDisplay });

function initializeDrawer() {
    app = ForgeDraw.init(view, viewer, function () {
        view.addEventListener("fd.point.detecterror", function (event) {
            console.log("view => fd.point.detecterror");
            createDialogModal({ id: "DialogModal-Error", inner: "座標點位於模型外，無法定位！" })
        });
        view.addEventListener("fd.linedata.change", function (event) {
            console.log("view => fd.linedata.change");
            updatePathDisplay(calcPath());
        });
        view.addEventListener("fd.linedata.detail", function (event) {
            console.log("view => fd.linedata.detail", event.detail);
            if (event.detail instanceof ForgeDraw.DevicePoint) {
                createDevicePointDetailModal(event.detail);
            }
            else {
                createPointDetailModal(event.detail);
            }
        });
        view.addEventListener("fd.devicepoint.ignore", function (event) {
            console.log("view => fd.devicepoint.ignore");
            updatePathDisplay(calcPath());
        });

        let pathData = JSON.parse(sessionStorage.getItem(`P1_pathData`))

        createBeacons(pathData.PathSample.Beacon);
        createDevices(1);
        createLinePath(pathData.PathSampleRecord);
        updatePathDisplay(pathData.PathSampleOrder, 1);

        addToolbarEvent();
        addTooltip(true);
        togglePointerEvent(true);
    });
}

window.addEventListener('load', function () {
    pathGroup.setSetting({
        delete: true,
        sortable: true,
        startButton: {
            id: "path-handle",
            icon: "icon-handle",
            tooltip: "拖曳順序",
        },
        endButton: {
            id: "path-search",
            icon: "icon-search",
            tooltip: "查看此路線",
            onClick: (e) => {

                checkNeedSaveCurrentPath(callback);

                function callback() {
                    let pathID = $(e.target).closest(".sample-path-group").attr("data-path-id");
                    let pathData = JSON.parse(sessionStorage.getItem(`P${pathID}_pathData`))
                    $("#current-path-title").val(pathData.PathSample.PathTitle);
                    sortRouteModal.autoRouteToggle(false);
                    loadModel(
                        window.location.origin + pathData.PathSample.BIMPath,
                        pathID,
                        loadPath)
                }

            }
        }
    })
})