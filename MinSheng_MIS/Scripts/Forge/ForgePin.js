function ForgePin({ viewer, position, data = {}, img, id = Math.random().toString(36).slice(2, 7) }) {
    const $e = $(`<div class="pin"><img src="${img}" alt=""></div>`);
    const e = $e[0];
    e.id = "pin_" + id;
    $e.addClass(e.id);
    let offset = ["-50%", "-50%"];
    let zIndex = undefined;
    function get2DPosition(vPos) {
        const v = new THREE.Vector3(vPos.x, vPos.y, vPos.z);
        const c = viewer.worldToClient(v);
        return { x: c.x, y: c.y };
    }
    function getDistance(vPos) {
        const vector = new THREE.Vector3(vPos.x, vPos.y, vPos.z);
        return vector.distanceTo(viewer.getCamera().position);
    }
    function isCameraView(vPos) {
        const vector = new THREE.Vector3(vPos.x, vPos.y, vPos.z);
        const camera = viewer.getCamera();
        var frustum = new THREE.Frustum();
        frustum.setFromMatrix(
            new THREE.Matrix4().multiplyMatrices(
                camera.projectionMatrix,
                camera.matrixWorldInverse
            )
        );
        return frustum.containsPoint(vector);
    }
    this.e = e;
    this.id = e.id;
    this.position = position;
    this.data = data;
    this.popover = undefined;
    this.isCameraView = () => {
        return isCameraView(this.position);
    };
    this.position2d = () => {
        return get2DPosition(this.position);
    };
    this.zIndex = () => {
        return -Math.floor(getDistance(this.position));
    };
    this.setPosition = (newPos) => {
        this.position.copy(newPos)
    };
    this.setImg = (url) => {
        $e.find("img").attr("src", url);
    };
    this.setScale = (x = 1, y = x) => {
        $e.find("img").css({ transform: `scale(${x},${y})` });
    };
    this.setOffset = (x, y) => {
        offset = [x, y ?? x];
    };
    this.setZIndex = (z = undefined) => {
        zIndex = z;
        $e.css({ "z-index": z });
    };
    this.isShow = () => {
        return e.style.display == "block";
    };
    //開啟
    this.show = () => {
        e.style.display = "block";
        this.update();
    };
    //關閉
    this.hide = () => {
        e.style.display = "none";
    };
    //更新
    this.update = (enforce = false) => {
        if (!this.isShow() && !enforce) return;
        let current2Dpos = get2DPosition(position);

        let v = isCameraView(position) ? "visible" : "hidden";

        $e.css({
            left: current2Dpos.x,
            top: current2Dpos.y,
            "z-index": zIndex ? zIndex : -Math.floor(getDistance(position)),
            visibility: v,
        });

        this.popover && this.popover.update();
    };
    this.addPopover = (options = {}) => {
        this.popover = new PinPopover(this, options);
        if (options.hoverEvent) {
            e.addEventListener("mousemove", options.hoverEvent.bind(this, true));
            e.addEventListener("mouseout", options.hoverEvent.bind(this, false));
        }
        if (options.clickEvent) {
            e.addEventListener("click", options.clickEvent.bind(this));
        }
        if (options.touchEvent) {
            e.addEventListener("touchstart", options.touchEvent.bind(this));
        }
    };
    //console.log("viewer: ", viewer)
    $(`#${viewer.clientContainer.id} .pin-area`).append(e);

    return this;
}
function PinPopover(pin, options = {}) {
    const p = pin;
    const $e = $(
        options.html && options.html.length != 0
            ? options.html
            : `<div class="popover"></div>`
    );
    const e = $e[0];
    let offset = options.offset ?? [0, 0];
    e.id = p.id + "_popover";
    this.e = e;
    this.pin = p;
    this.setContent = options.setContent ?? function () { };
    this.isShow = () => e.hasAttribute("data-show");
    //開啟
    this.show = () => {
        e.setAttribute("data-show", "");
    };
    //關閉
    this.hide = () => {
        e.removeAttribute("data-show");
    };
    //更新
    this.update = () => {
        //this.setContent(pin.data);
        $e.css({
            left: `calc(50% - ${typeof offset[0] == "number" ? offset[0] + "px" : offset[0]})`,
            top: `calc(50% - ${typeof offset[1] == "number" ? offset[1] + "px" : offset[1]})`,
            visibility: pin.isCameraView() ? "visible" : "hidden",
        });
    };
    $(p.e).append(e);
    return this;
}
const defaultPopoverOptions = {
    offset: [0, 0], //also can use "100%"...css can read
    //a html that pin popover display for
    html: `<div class="pin-popover">
            <div class="popover-header">
            </div>
            <div class="popover-body">
            </div>
        </div>
        `,
    setContent: function (data) {
        //the function to set html content
        //"this" is popover
        const $e = $(this.e);
        $e.find("#eName").html(data.name);
        if (data.isDue) {
            $(this.pin.e).find("img").addClass("due");
        }
    },
    hoverEvent: function (isHover, event) {
        //"this" is pin
        isHover
            ? (this.popover.show(), this.setZIndex(100))
            : (this.popover.hide(), this.setZIndex());
    },
    clickEvent: function (event) {
        //"this" is pin
        this.popover.isShow()
            ? (this.popover.show(), this.setZIndex(100))
            : (this.popover.hide(), this.setZIndex());
    },
    touchEvent: function (event) {
        //"this" is pin
        this.popover.isShow()
            ? (this.popover.show(), this.setZIndex(100))
            : (this.popover.hide(), this.setZIndex());
    },
};

/* html:
    <div class="pin-area"></div>
*/

/* css :
    .pin-area {
        position: absolute;
        left: 0;
        top: 0;
        bottom: 0;
        right: 0;
        z-index: 100;
        overflow: hidden;
        pointer-events: none;
    }

    .pin {
        position: absolute;
        transform: translate(-50%, 0%);
        pointer-events: auto;
        cursor: pointer;
    }

    .pin-popover {
        position: absolute;
        font-size: 12px;
        background: #295c44;
        transform: translate(-50%, -50%);
        width: max-content;
        filter: drop-shadow(0 0 3px #000);
    }

    .pin-popover {
        display: none;
    }

    .pin-popover[data-show] {
        display: block;
    }

    .pin-popover::before {
        content: "";
        position: absolute;
        bottom: 0;
        left: 50%;
        width: 20px;
        height: 12px;
        background: #295c44;
        transform: translate(-50%, 100%);
        clip-path: polygon(50% 100%, 0 0, 100% 0);
    }

    .popover-header {
        background-color: #133a28;
        color: #fff;
        font-size: 16px;
        width: 100%;
        padding: 0.3em 0.6em;
    }

    .popover-body {
        width: 100%;
        color: lightgray;
        padding: 0.4em;
    }

    .popover-body table tr th {
        padding: 0.4em;
        text-align: center;
        color: lightgray;
    }

    .popover-body table tr td {
        padding: 0.4em;
        color: #e5fde8;
    }
*/
