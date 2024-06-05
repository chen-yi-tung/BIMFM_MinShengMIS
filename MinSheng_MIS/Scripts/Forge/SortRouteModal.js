//import "https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js"

/**
 * @typedef SortRouteModalOptions
 * @property {string} control - default is "#path-auto"
 * @property {function(string[])} onSave - arg0 is route
 * 
 * @param {SortRouteModalOptions} options 
 */
function SortRouteModal(options = {}) {
    this.autoCalcRoute = true;
    this.ModalJQ = $(`
    <div class="modal fade sort-list-modal" tabindex="-1" id="sortRouteModal">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">修改路徑</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="sort-list-area"></div>
                </div>
                <div class="modal-footer justify-content-center">
                    <button type="button" class="btn btn-back" data-bs-dismiss="modal">取消</button>
                    <button type="button" class="btn btn-export" onclick="sortRouteModal.save()">儲存變更</button>
                </div>
            </div>
        </div>
    </div>
    `);
    this.ModalBs = new bootstrap.Modal(this.ModalJQ[0]);

    let sortlist = $(`<ul class="sortlist" id="sortlist"></ul>`);
    this.create = function (selector) {
        let $area = this.ModalJQ.find(".sort-list-area");
        createRouteItems();
        let sortable = Sortable.create(sortlist[0], {
            swapThreshold: 0.45,
            animation: 150,
            ghostClass: "ghost",
            dragClass: "drag",
            direction: 'horizontal',
            filter: '.disable',
            draggable: ".item"
        });
        $area.empty();
        $area.append(sortlist);
        this.ModalBs.show();

        function createRouteItems() {
            sortlist.empty();
            let devicelist = ForgeDraw.devices;
            //let itemSelector = ".breadcrumb-item:not(:first-child):not(:last-child)";
            let itemSelector = ".breadcrumb-item";
            let pathList = $(selector ? `${selector} ${itemSelector}` : itemSelector);

            console.log(devicelist);
            console.log(pathList);
            //sortlist.append(`<li class="disable">起點</li>`);

            pathList.each((i, e) => {
                let type = devicelist.find(dp => dp.name == e.innerHTML).type;
                switch (type) {
                    case "藍芽":
                        type = "cube-bluetooth";
                        break;
                    case "BothDevice":
                        type = "cube-both";
                        break;
                    case "Maintain":
                        type = "cube-maintain";
                        break;
                    case "Repair":
                        type = "cube-repair";
                        break;
                }
                let li = `<li class="item ${type}">${e.innerHTML}</li>`;
                sortlist.append(li);
            })
            //sortlist.append(`<li class="disable">終點</li>`);
        }
    }
    this.save = function () {
        this.autoRouteToggle(false);

        let route = sortlist.find("li").toArray().map((e) => {
            return $(e).text();
        })

        options.onSave && options.onSave(route);

        this.ModalBs.hide();
    }
    this.autoRouteToggle = function (bool = undefined) {
        let btn = $(options.control ?? "#path-auto");
        if (bool != undefined) {
            this.autoCalcRoute = bool;
            btn.toggleClass("active", bool);
            return;
        }
        this.autoCalcRoute = !this.autoCalcRoute;
        btn.toggleClass("active", this.autoCalcRoute);
    }

    return this;
}