//import "https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js"

/**
 * @typedef CreateDeviceModalOptions
 * @property {string} control - default is "#path-auto"
 * @property {function(string[])} onSave - arg0 is route
 * 
 * @param {CreateDeviceModalOptions} options 
 */
function CreateDeviceModal(options = {}) {
    this.autoCalcRoute = true;
    this.ModalJQ = $(`
    <div class="modal fade sort-list-modal" tabindex="-1" id="createDeviceModal">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">新增自訂設備</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <label for="deviceType" class="mb-2">設備類別</label>
                    <input class="form-control" type="text" id="deviceType">
                    <label for="deviceName" class="mb-2 mt-3">設備名稱</label>
                    <input class="form-control" type="text" id="deviceName">
                </div>
                <div class="modal-footer justify-content-center">
                    <button type="button" class="btn btn-back" data-bs-dismiss="modal">取消</button>
                    <button type="button" class="btn btn-export" onclick="createDeviceModal.save()">新增設備</button>
                </div>
            </div>
        </div>
    </div>
    `);
    this.ModalBs = new bootstrap.Modal(this.ModalJQ[0]);

    this.detail = undefined;

    this.devicePointOptions = {
        color: 0x3ac0ff,
        contextMenu: {
            html: `<div class="contextMenu"><ul></ul></div>`,
            button: [
                {
                    name: "詳細",
                    onClick: function (event, point) {
                        console.log(`devicePoint ${point.dbId} => 詳細`);
                        view.dispatchEvent(new CustomEvent('fd.linedata.detail', { 'detail': point }));
                    }
                },
                {
                    name: "忽略",
                    onClick: function (event, point) {
                        console.log(`devicePoint ${point.dbId} => 忽略`);
                        point.isUpdate = !point.isUpdate;
                        event.target.innerHTML = point.isUpdate ? "忽略" : "取消忽略";
                        point.update();
                        view.dispatchEvent(new CustomEvent('fd.devicepoint.ignore', { 'detail': point }));
                    }
                },
                {
                    name: "刪除",
                    onClick: function (event, point) {
                        console.log(`point ${point.index} => 刪除`);
                        point.remove();
                    }
                },
            ]
        }
    }

    this.create = function (detail) {
        this.ModalBs.show();
        this.detail = detail;
    }

    this.save = function () {
        let data = {
            position: this.detail.position,
            deviceType: this.ModalJQ.find("#deviceType").val(),
            deviceName: this.ModalJQ.find("#deviceName").val()
        }
        let dp = new ForgeDraw.DevicePoint(data, this.devicePointOptions);
        this.ModalBs.hide();
    }

    this.ModalJQ.on("hide.bs.modal", function () {
        self.detail && self.detail.callback();
    })

    return this;
}