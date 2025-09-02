(function(global, factory) {
    typeof exports === "object" && typeof module !== "undefined"
        ? module.exports = factory(require("@form-validation/core"))
        : typeof define === "function" && define.amd
        ? define(["@form-validation/core"], factory)
        : (global = typeof globalThis !== "undefined" ? globalThis : global || self, (global.FormValidation =
            global.FormValidation || {}, global.FormValidation.validators =
            global.FormValidation.validators || {}, global.FormValidation.validators.step =
            factory(global.FormValidation)));
})(this,
    (function(core) {
        "use strict";

        /**
         * FormValidation (https://formvalidation.io)
         * The best validation library for JavaScript
         * (c) 2013 - 2023 Nguyen Huu Phuoc <me@phuoc.ng>
         */
        var format = core.utils.format;

        function step() {
            var round = function(input, precision) {
                const m = Math.pow(10, precision);
                const x = input * m;
                var sign;
                switch (true) {
                case x === 0:
                    sign = 0;
                    break;
                case x > 0:
                    sign = 1;
                    break;
                case x < 0:
                    sign = -1;
                    break;
                }
                const isHalf = x % 1 === 0.5 * sign;
                return isHalf ? (Math.floor(x) + (sign > 0 ? 1 : 0)) / m : Math.round(x) / m;
            };
            var floatMod = function(x, y) {
                if (y === 0.0) {
                    return 1.0;
                }
                const dotX = "".concat(x).split(".");
                const dotY = "".concat(y).split(".");
                const precision = (dotX.length === 1 ? 0 : dotX[1].length) + (dotY.length === 1 ? 0 : dotY[1].length);
                return round(x - y * Math.floor(x / y), precision);
            };
            return {
                validate: function(input) {
                    if (input.value === "") {
                        return { valid: true };
                    }
                    const v = parseFloat(input.value);
                    if (isNaN(v) || !isFinite(v)) {
                        return { valid: false };
                    }
                    const opts = Object.assign({},
                        {
                            baseValue: 0,
                            message: "",
                            step: 1,
                        },
                        input.options);
                    const mod = floatMod(v - opts.baseValue, opts.step);
                    return {
                        message: format(input.l10n ? opts.message || input.l10n.step.default : opts.message,
                            "".concat(opts.step)),
                        valid: mod === 0.0 || mod === opts.step,
                    };
                },
            };
        }

        return step;

    }));