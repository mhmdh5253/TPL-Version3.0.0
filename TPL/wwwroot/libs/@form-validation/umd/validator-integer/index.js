(function(global, factory) {
    typeof exports === "object" && typeof module !== "undefined"
        ? module.exports = factory(require("@form-validation/core"))
        : typeof define === "function" && define.amd
        ? define(["@form-validation/core"], factory)
        : (global = typeof globalThis !== "undefined" ? globalThis : global || self, (global.FormValidation =
            global.FormValidation || {}, global.FormValidation.validators =
            global.FormValidation.validators || {}, global.FormValidation.validators.integer =
            factory(global.FormValidation)));
})(this,
    (function(core) {
        "use strict";

        /**
         * FormValidation (https://formvalidation.io)
         * The best validation library for JavaScript
         * (c) 2013 - 2023 Nguyen Huu Phuoc <me@phuoc.ng>
         */
        var removeUndefined = core.utils.removeUndefined;

        function integer() {
            return {
                validate: function(input) {
                    if (input.value === "") {
                        return { valid: true };
                    }
                    const opts = Object.assign({},
                        {
                            decimalSeparator: ".",
                            thousandsSeparator: "",
                        },
                        removeUndefined(input.options));
                    const decimalSeparator = opts.decimalSeparator === "." ? "\\." : opts.decimalSeparator;
                    const thousandsSeparator = opts.thousandsSeparator === "." ? "\\." : opts.thousandsSeparator;
                    const testRegexp = new RegExp("^-?[0-9]{1,3}(".concat(thousandsSeparator, "[0-9]{3})*(")
                        .concat(decimalSeparator, "[0-9]+)?$"));
                    const thousandsReplacer = new RegExp(thousandsSeparator, "g");
                    var v = "".concat(input.value);
                    if (!testRegexp.test(v)) {
                        return { valid: false };
                    }
                    // Replace thousands separator with blank
                    if (thousandsSeparator) {
                        v = v.replace(thousandsReplacer, "");
                    }
                    // Replace decimal separator with a dot
                    if (decimalSeparator) {
                        v = v.replace(decimalSeparator, ".");
                    }
                    const n = parseFloat(v);
                    return { valid: !isNaN(n) && isFinite(n) && Math.floor(n) === n };
                },
            };
        }

        return integer;

    }));