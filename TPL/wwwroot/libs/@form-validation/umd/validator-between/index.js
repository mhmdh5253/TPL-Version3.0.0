(function(global, factory) {
    typeof exports === "object" && typeof module !== "undefined"
        ? module.exports = factory(require("@form-validation/core"))
        : typeof define === "function" && define.amd
        ? define(["@form-validation/core"], factory)
        : (global = typeof globalThis !== "undefined" ? globalThis : global || self, (global.FormValidation =
            global.FormValidation || {}, global.FormValidation.validators =
            global.FormValidation.validators || {}, global.FormValidation.validators.between =
            factory(global.FormValidation)));
})(this,
    (function(core) {
        "use strict";

        /**
         * FormValidation (https://formvalidation.io)
         * The best validation library for JavaScript
         * (c) 2013 - 2023 Nguyen Huu Phuoc <me@phuoc.ng>
         */
        var format = core.utils.format, removeUndefined = core.utils.removeUndefined;

        function between() {
            var formatValue = function(value) {
                return parseFloat("".concat(value).replace(",", "."));
            };
            return {
                validate: function(input) {
                    const value = input.value;
                    if (value === "") {
                        return { valid: true };
                    }
                    const opts = Object.assign({}, { inclusive: true, message: "" }, removeUndefined(input.options));
                    const minValue = formatValue(opts.min);
                    const maxValue = formatValue(opts.max);
                    return opts.inclusive
                        ? {
                            message: format(input.l10n ? opts.message || input.l10n.between.default : opts.message,
                                [
                                    "".concat(minValue),
                                    "".concat(maxValue),
                                ]),
                            valid: parseFloat(value) >= minValue && parseFloat(value) <= maxValue,
                        }
                        : {
                            message: format(input.l10n ? opts.message || input.l10n.between.notInclusive : opts.message,
                                [
                                    "".concat(minValue),
                                    "".concat(maxValue),
                                ]),
                            valid: parseFloat(value) > minValue && parseFloat(value) < maxValue,
                        };
                },
            };
        }

        return between;

    }));