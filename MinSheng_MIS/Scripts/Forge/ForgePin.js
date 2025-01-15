class PinPopover {
    #options = null;
    #formatAsPx(v) {
        return typeof v == "number" ? v + "px" : v;
    }
    #getElement() {
        if (this.#options.element) {
            const c = this.#options.element.cloneNode(true);
            this.#options.element.remove();
            return c;
        }
        const htmlContent = this.#options.html && this.#options.html.length != 0 ? this.#options.html : `<div class="popover"></div>`;
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = htmlContent;
        return tempDiv.firstChild;
    }
    constructor(pin, options = {}) {
        this.pin = pin;
        this.#options = Object.assign(
            {
                offset: [0, 0],
                setContent: function () { },
            },
            options
        );
        this.viewer = this.pin.viewer;
        this.setContent = this.#options.setContent;
        this.offset = this.#options.offset;
        this.id = this.pin.id + "_popover";
        this.element = this.#getElement();
        this.pin.element.appendChild(this.element);
    }
    get visible() {
        return this.element.hasAttribute("data-show");
    }
    set visible(v) {
        if (v) {
            this.element.setAttribute("data-show", "");
            this.update();
        } else {
            this.element.removeAttribute("data-show");
        }
    }
    show() {
        this.visible = true;
        return this;
    }
    hide() {
        this.visible = false;
        return this;
    }
    update() {
        this.element.style.visibility = this.pin.isCameraView() ? "visible" : "hidden";
        if (Array.isArray(this.offset)) {
            this.element.style.left = this.#formatAsPx(this.offset[0]);
            this.element.style.top = this.#formatAsPx(this.offset[1]);
        } else if (typeof offset == "object") {
            const { top, left, right, bottom } = offset;
            this.element.style.top = top != void 0 ? this.#formatAsPx(top) : "";
            this.element.style.left = left != void 0 ? this.#formatAsPx(left) : "";
            this.element.style.right = right != void 0 ? this.#formatAsPx(right) : "";
            this.element.style.bottom = bottom != void 0 ? this.#formatAsPx(bottom) : "";
        }
    }
}

class ForgePin {
    #options = null;
    #position = null;
    #zIndex = 0;
    #offset = [0, 0];
    #scale = 1;
    #formatAsPx(v) {
        return typeof v == "number" ? v + "px" : v;
    }
    #getRandomId() {
        return Math.random().toString(36).slice(2, 7);
    }
    #calcPosition() {
        if (this.#options?.position) {
            const v = this.#options?.position;
            return new THREE.Vector3(v.x, v.y, v.z);
        }
        if (this.#options.dbId) {
            const result = this.viewer.toolkit.getCenter(this.#options.dbId, this.#options.model);
            if (!result) { console.error(`[ForgePin] The DBID: ${this.#options.dbId}, Position is not set.`); }
            return result
        }
        return new THREE.Vector3(0, 0, 0);
    }
    #calcZIndex() {
        return this.zIndex ? this.zIndex : -Math.floor(this.viewer.toolkit.getDistance(this.#position));
    }
    #getContainer(selector) {
        let c = document.getElementById(selector);
        if (!(c instanceof HTMLElement)) {
            c = document.querySelector(selector);
        }
        if (!(c instanceof HTMLElement)) {
            c = document.getElementById("pin-area");
        }
        if (!(c instanceof HTMLElement)) {
            this.viewer.container.insertAdjacentHTML("beforeend", '<div class="pin-area" id="pin-area"></div>');
            c = document.getElementById("pin-area");
        }
        return c;
    }
    constructor(options = {}) {
        this.#options = Object.assign(
            {
                viewer: NOP_VIEWER,
                id: this.#getRandomId(),
                offset: ["-50%", "-50%"],
            },
            options
        );
        this.viewer = this.#options.viewer;
        this.element = document.createElement("div");
        this.imgElement = document.createElement("img");
        this.imgElement.src = options.img;
        this.element.appendChild(this.imgElement);
        this.id = "pin_" + this.#options.id;
        this.element.id = this.id;
        this.element.classList.add('pin');
        this.element.classList.add(this.id);
        this.#position = this.#calcPosition();
        //console.log(this.#position);

        this.#offset = this.#options.offset;
        this.data = this.#options.data;

        this.popover = undefined;
        this.#options.popoverOptions && this.addPopover(this.#options.popoverOptions);
        this.container = this.#getContainer(this.#options.container);
        this.container.appendChild(this.element);

        if (this.#options.onHover) {
            this.on("mousemove", this.#options.onHover.bind(this, true));
            this.on("mouseout", this.#options.onHover.bind(this, false));
        }
        if (this.#options.onClick) {
            this.on("click", this.#options.onClick.bind(this));
        }
        if (this.#options.onTouchStart) {
            this.on("touchstart", this.#options.onTouchStart.bind(this));
        }
    }
    get visible() {
        return this.element.style.display == "inline-block";
    }
    set visible(v) {
        if (v) {
            this.element.style.display = "inline-block";
            this.update();
        } else {
            this.element.style.display = "none";
        }
    }
    get position() {
        return this.#position;
    }
    set position(v) {
        this.#position = new THREE.Vector3(v.x, v.y, v.z);
    }
    get img() {
        return this.imgElement.src;
    }
    set img(v) {
        this.imgElement.src = v;
    }
    get offset() {
        return this.#offset;
    }
    set offset(v) {
        this.#offset = v;
    }
    get zIndex() {
        return this.#zIndex;
    }
    set zIndex(v) {
        this.#zIndex = v;
        this.element.style.zIndex = v;
    }
    get scale() {
        return this.#scale;
    }
    set scale(v) {
        this.#scale = v;
        if (Array.isArray(v)) {
            this.imgElement.style.transform = `scale(${v[0]},${v[1]})`;
        } else {
            this.imgElement.style.transform = `scale(${v},${v})`;
        }
    }
    update(force = false) {
        if (!force && !this.visible) {
            return;
        }
        if (!this.#position) {
            this.hide();
            return;
        }
        const current2Dpos = this.get2DPosition(this.#position);
        const ox = this.#formatAsPx(this.#offset[0]);
        const oy = this.#formatAsPx(this.#offset[1]);
        const px = this.#formatAsPx(current2Dpos.x);
        const py = this.#formatAsPx(current2Dpos.y);
        const x = `calc(${ox} + ${px})`;
        const y = `calc(${oy} + ${py})`;
        this.element.style.zIndex = this.#calcZIndex();
        this.element.style.transform = `translate(${x}, ${y})`;
        this.element.style.visibility = this.isCameraView() ? "visible" : "hidden";

        this.popover && this.popover.update();
        return this;
    }
    show() {
        this.visible = true;
        return this;
    }
    hide() {
        this.visible = false;
        return this;
    }
    get2DPosition() {
        return this.viewer.toolkit.get2DPosition(this.#position);
    }
    isCameraView() {
        return this.viewer.toolkit.isCameraView(this.#position);
    }
    addPopover(options = {}) {
        this.popover = new PinPopover(this, options);
    }
    on(event, callback) {
        this.element.addEventListener(event, callback.bind(this));
    }
    addEventListener(event, callback) { this.on(event, callback); }
    off(event, callback) {
        this.element.removeEventListener(event, callback.bind(this));
    }
    removeEventListener(event, callback) { this.off(event, callback); }
    destroy() {
        this.element.remove();
        this.popover && this.popover.element.remove();
    }
    dispatchEvent(event) {
        this.element.dispatchEvent(event);
    }
}
