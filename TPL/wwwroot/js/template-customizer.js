"use strict";
(function webpackUniversalModuleDefinition(root, factory) {
	if(typeof exports === 'object' && typeof module === 'object')
		module.exports = factory();
	else if(typeof define === 'function' && define.amd)
		define([], factory);
	else {
		var a = factory();
		for(var i in a) (typeof exports === 'object' ? exports : root)[i] = a[i];
	}
})(self, () => {
return (self["webpackChunktpl"] = self["webpackChunktpl"] || []).push([[11],{

/***/ 55:
/***/ ((module) => {

module.exports = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAABClJREFUaEPtmY1RFEEQhbsjUCIQIhAiUCNQIxAiECIQIxAiECIAIpAMhAiECIQI2vquZqnZvp6fhb3SK5mqq6Ju92b69bzXf6is+dI1t1+eAfztG5z1BsxsU0S+ici2iPB3vm5E5EpEDlSVv2dZswFIxv8UkZcNy+5EZGcuEHMCOBeR951uvVDVD53vVl+bE8DvDu8Pxtyo6ta/BsByg1R15Bwzqz5/LJgn34CZwfnPInI4BUB6/1hV0cSjVxcAM4PbcBZjL0XklIPN7Is3fLCkdQPpPYw/VNXj5IhPIvJWRIhSl6p60ULWBGBm30Vk123EwRxCuIzWkkjNrCZywith10ewE1Xdq4GoAjCz/RTXW44Ynt+LyBEfT43kYfbj86J3w5Q32DNcRQDpwF+dkQXDMey8xem0L3TEqB4g3PZWad8agBMRgZPeu96D1/C2Zbh3X0p80Op1xxloztN48bMQQNoc7+eLEuAoPSPiIDY4Ooo+E6ixeNXM+D3GERz2U3CIqMstLJUgJQDe+7eq6mub0NYEkLAKwEHkiBQDCZtddZCZ8d6r7JDwFkoARklHRPZUFVDVZWbwGuNrC4EfdOzFrRABh3Wnqhv+d70AEBLGFROPmeHlnM81G69UdSd6IUuM0GgUVn1uqWmg5EmMfBeEyB7Pe3txBkY+rGT8j0J+WXq/BgDkUCaqLgEAnwcRog0veMIqFAAwCy2wnw+bI2GaGboBgF9k5N0o0rUSGUb4eO0BeO9j/GYhkSHMHMTIqwGARX6p6a+nlPBl8kZuXMD9j6pKfF9aZuaFOdJCEL5D4eYb9wCYVCanrBmGyii/tIq+SLj/HQBCaM5bLzwfPqdQ6FpVHyra4IbuVbXaY7dETC2ESPNNWiIOi69CcdgSMXsh4tNSUiklMgwmC0aNd08Y5WAES6HHehM4gu97wyhBgWpgqXsrASglprDy7CwhehMZOSbK6JMSma+Fio1KltCmlBIj7gfZOGx8ppQSXrhzFnOhJ/31BDkjFHRvOd09x0mRBA9SFgxUgHpQg0q0t5ymPMlL+EnldFTfDA0NAmf+OTQ0X0sRouf7NNkYGhrOYNrxtIaGg83MNzVDSe3LXLhP7O/yrCsCz1zlWTpjWkuZAOBpX3yVnLqI1yLCOKU6qMrmP7SSrUEw54XF4WBIK5FxCMOr3lVsfGqNSmPzBXUnJTIX1jyVBq9wO6UObOpgC5GjO98vFKnTdQMZXxEsWZlDiCZMIxAbNxQOqlpVZtobejBaZNoBnRDzMFpkxvTQOD36BlrcySZuI6p1ACB6LU3wWuf5581+oHfD1vi89bz3nFUC8Nm7ZlP3nKkFbM4bWPt/MSFwklprYItwt6cmvpWJ2IVcQBCz6bLysSCv3SaANCiTsnaNRrNRqMXVVT1/BrAqz/buu/Y38Ad3KC5PARej0QAAAABJRU5ErkJggg==";

/***/ }),

/***/ 290:
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

// ESM COMPAT FLAG
__webpack_require__.r(__webpack_exports__);

// EXPORTS
__webpack_require__.d(__webpack_exports__, {
  TemplateCustomizer: () => (/* binding */ TemplateCustomizer)
});

// EXTERNAL MODULE: ./node_modules/style-loader/dist/runtime/injectStylesIntoStyleTag.js
var injectStylesIntoStyleTag = __webpack_require__(72);
var injectStylesIntoStyleTag_default = /*#__PURE__*/__webpack_require__.n(injectStylesIntoStyleTag);
// EXTERNAL MODULE: ./node_modules/style-loader/dist/runtime/styleDomAPI.js
var styleDomAPI = __webpack_require__(825);
var styleDomAPI_default = /*#__PURE__*/__webpack_require__.n(styleDomAPI);
// EXTERNAL MODULE: ./node_modules/style-loader/dist/runtime/insertBySelector.js
var insertBySelector = __webpack_require__(659);
var insertBySelector_default = /*#__PURE__*/__webpack_require__.n(insertBySelector);
// EXTERNAL MODULE: ./node_modules/style-loader/dist/runtime/setAttributesWithoutAttributes.js
var setAttributesWithoutAttributes = __webpack_require__(56);
var setAttributesWithoutAttributes_default = /*#__PURE__*/__webpack_require__.n(setAttributesWithoutAttributes);
// EXTERNAL MODULE: ./node_modules/style-loader/dist/runtime/insertStyleElement.js
var insertStyleElement = __webpack_require__(540);
var insertStyleElement_default = /*#__PURE__*/__webpack_require__.n(insertStyleElement);
// EXTERNAL MODULE: ./node_modules/style-loader/dist/runtime/styleTagTransform.js
var styleTagTransform = __webpack_require__(113);
var styleTagTransform_default = /*#__PURE__*/__webpack_require__.n(styleTagTransform);
// EXTERNAL MODULE: ./node_modules/css-loader/dist/cjs.js??ruleSet[1].rules[3].use[1]!./node_modules/sass-loader/dist/cjs.js??ruleSet[1].rules[3].use[2]!./wwwroot/js/_template-customizer/_template-customizer.scss
var _template_customizer = __webpack_require__(433);
;// ./wwwroot/js/_template-customizer/_template-customizer.scss

      
      
      
      
      
      
      
      
      

var options = {};

options.styleTagTransform = (styleTagTransform_default());
options.setAttributes = (setAttributesWithoutAttributes_default());
options.insert = insertBySelector_default().bind(null, "head");
options.domAPI = (styleDomAPI_default());
options.insertStyleElement = (insertStyleElement_default());

var update = injectStylesIntoStyleTag_default()(_template_customizer/* default */.A, options);




       /* harmony default export */ const _template_customizer_template_customizer = (_template_customizer/* default */.A && _template_customizer/* default */.A.locals ? _template_customizer/* default */.A.locals : undefined);

;// ./wwwroot/js/_template-customizer/_template-customizer.html
// Module
var code = `<div id="template-customizer" class="invert-bg-white"> <a href="javascript:void(0)" class="template-customizer-open-btn" tabindex="-1"></a> <div class="p-4 m-0 lh-1 border-bottom template-customizer-header"> <h4 class="template-customizer-t-panel_header mb-2"></h4> <p class="template-customizer-t-panel_sub_header mb-0"></p> <a href="javascript:void(0)" class="btn-close template-customizer-close-btn fw-light px-4 py-2 text-body" tabindex="-1"> </a> </div> <div class="template-customizer-inner pt-4"> <div class="template-customizer-theming"> <h5 class="m-0 px-4 py-4 lh-1 text-light d-block"> <span class="template-customizer-t-theming_header"></span> </h5> <div class="m-0 px-4 pb-3 template-customizer-themes w-100"> <label for="customizerTheme" class="form-label template-customizer-t-theme_label"></label> <div class="row row-cols-lg-auto g-3 align-items-center template-customizer-themes-options"></div> </div> <div class="m-0 px-4 pb-3 pt-1 template-customizer-style w-100"> <label for="customizerStyle" class="form-label d-block template-customizer-t-style_label"></label> <label class="switch switch-sm"> <span class="switch-label template-customizer-t-style_switch_light"></span> <input type="checkbox" class="switch-input"/> <span class="switch-toggle-slider"> <span class="switch-on"></span> <span class="switch-off"></span> </span> <span class="switch-label template-customizer-t-style_switch_dark"></span> </label> </div> </div> <div class="template-customizer-layout"> <hr class="m-0 border-light"> <h5 class="m-0 px-4 py-4 lh-1 text-light d-block"> <span class="template-customizer-t-layout_header"></span> </h5> <div class="m-0 px-4 pb-3 d-block template-customizer-layoutType"> <label for="customizerStyle" class="form-label d-block template-customizer-t-layout_label"></label> <div class="row row-cols-lg-auto g-3 align-items-center template-customizer-layouts-options"> <div class="col-12"> <div class="form-check"> <input class="form-check-input" type="radio" name="layoutRadios" id="layoutRadios-static" value="static"> <label class="form-check-label template-customizer-t-layout_static" for="layoutRadios-static"></label> </div> </div> <div class="col-12"> <div class="form-check"> <input class="form-check-input" type="radio" name="layoutRadios" id="layoutRadios-fixed" value="fixed"> <label class="form-check-label template-customizer-t-layout_fixed" for="layoutRadios-fixed"></label> </div> </div> </div> </div> <label class="m-0 px-4 pb-3 d-flex media align-items-middle justify-content-between template-customizer-layoutNavbarFixed"> <span class="template-customizer-t-layout_navbar_label"></span> <label class="switch switch-sm pe-4"> <input type="checkbox" class="switch-input"/> <span class="switch-toggle-slider"> <span class="switch-on"></span> <span class="switch-off"></span> </span> </label> </label> <label class="m-0 px-4 pb-3 d-flex media align-items-middle justify-content-between template-customizer-layoutFooterFixed"> <span class="template-customizer-t-layout_footer_label"></span> <label class="switch switch-sm pe-4"> <input type="checkbox" class="switch-input"/> <span class="switch-toggle-slider"> <span class="switch-on"></span> <span class="switch-off"></span> </span> </label> </label> <label class="m-0 px-4 pb-3 d-flex media align-items-middle justify-content-between template-customizer-showDropdownOnHover"> <span class="template-customizer-t-layout_dd_open_label"></span> <label class="switch switch-sm pe-4"> <input type="checkbox" class="switch-input"/> <span class="switch-toggle-slider"> <span class="switch-on"></span> <span class="switch-off"></span> </span> </label> </label> </div> <div class="template-customizer-misc"> <hr class="m-0 border-light"> <h5 class="m-0 px-4 py-4 lh-1 text-light d-block"> <span class="template-customizer-t-misc_header"></span> </h5> <label class="m-0 px-4 pb-3 d-flex media align-items-middle justify-content-between template-customizer-rtl"> <span class="template-customizer-t-rtl_label"></span> <label class="switch switch-sm pe-4"> <input type="checkbox" class="switch-input"/> <span class="switch-toggle-slider"> <span class="switch-on"></span> <span class="switch-off"></span> </span> </label> </label> </div> </div> </div>`;
// Exports
/* harmony default export */ const js_template_customizer_template_customizer = (code);
;// ./wwwroot/js/template-customizer.js
function _typeof(o) { "@babel/helpers - typeof"; return _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (o) { return typeof o; } : function (o) { return o && "function" == typeof Symbol && o.constructor === Symbol && o !== Symbol.prototype ? "symbol" : typeof o; }, _typeof(o); }
function _defineProperty(e, r, t) { return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, { value: t, enumerable: !0, configurable: !0, writable: !0 }) : e[r] = t, e; }
function _classCallCheck(a, n) { if (!(a instanceof n)) throw new TypeError("Cannot call a class as a function"); }
function _defineProperties(e, r) { for (var t = 0; t < r.length; t++) { var o = r[t]; o.enumerable = o.enumerable || !1, o.configurable = !0, "value" in o && (o.writable = !0), Object.defineProperty(e, _toPropertyKey(o.key), o); } }
function _createClass(e, r, t) { return r && _defineProperties(e.prototype, r), t && _defineProperties(e, t), Object.defineProperty(e, "prototype", { writable: !1 }), e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == _typeof(i) ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != _typeof(t) || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != _typeof(i)) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }


var CSS_FILENAME_PATTERN = "%name%.css";
var CONTROLS = ["rtl", "style", "layoutType", "layoutMenuFlipped", "showDropdownOnHover", "layoutNavbarFixed", "layoutFooterFixed", "themes"];
var STYLES = ["light", "dark"];
var cl = document.documentElement.classList;
var DISPLAY_CUSTOMIZER = true;
var DEFAULT_THEME = document.getElementsByTagName("HTML")[0].getAttribute("data-theme") || 0;
var DEFAULT_STYLE = cl.contains("dark-style") ? "dark" : "light";
var DEFAULT_TEXT_DIR = document.documentElement.getAttribute("dir") === "rtl";
var DEFAULT_MENU_COLLAPSED = !!cl.contains("layout-menu-collapsed");
var DEFAULT_MENU_FLIPPED = !!cl.contains("layout-menu-flipped");
var DEFAULT_SHOW_DROPDOWN_ON_HOVER = undefined;
var DEFAULT_NAVBAR_FIXED = !!cl.contains("layout-navbar-fixed");
var DEFAULT_FOOTER_FIXED = !!cl.contains("layout-footer-fixed");
var layoutType;
if (cl.contains("layout-menu-offcanvas")) {
  layoutType = "static-offcanvas";
} else if (cl.contains("layout-menu-fixed")) {
  layoutType = "fixed";
} else if (cl.contains("layout-menu-fixed-offcanvas")) {
  layoutType = "fixed-offcanvas";
} else {
  layoutType = "static";
}
var DEFAULT_LAYOUT_TYPE = layoutType;
var TemplateCustomizer = /*#__PURE__*/function () {
  function TemplateCustomizer(_ref) {
    var cssPath = _ref.cssPath,
      themesPath = _ref.themesPath,
      cssFilenamePattern = _ref.cssFilenamePattern,
      displayCustomizer = _ref.displayCustomizer,
      controls = _ref.controls,
      defaultTextDir = _ref.defaultTextDir,
      defaultLayoutType = _ref.defaultLayoutType,
      defaultMenuCollapsed = _ref.defaultMenuCollapsed,
      defaultMenuFlipped = _ref.defaultMenuFlipped,
      defaultShowDropdownOnHover = _ref.defaultShowDropdownOnHover,
      defaultNavbarFixed = _ref.defaultNavbarFixed,
      defaultFooterFixed = _ref.defaultFooterFixed,
      styles = _ref.styles,
      defaultStyle = _ref.defaultStyle,
      availableThemes = _ref.availableThemes,
      defaultTheme = _ref.defaultTheme,
      pathResolver = _ref.pathResolver,
      onSettingsChange = _ref.onSettingsChange,
      lang = _ref.lang;
    _classCallCheck(this, TemplateCustomizer);
    if (this._ssr) return;
    if (!window.Helpers) throw new Error("window.Helpers required.");
    this.settings = {};
    this.settings.cssPath = cssPath;
    this.settings.themesPath = themesPath;
    this.settings.cssFilenamePattern = cssFilenamePattern || CSS_FILENAME_PATTERN;
    this.settings.displayCustomizer = typeof displayCustomizer !== "undefined" ? displayCustomizer : DISPLAY_CUSTOMIZER;
    this.settings.controls = controls || CONTROLS;
    this.settings.defaultTextDir = defaultTextDir === "rtl" ? true :  false || DEFAULT_TEXT_DIR;
    this.settings.defaultLayoutType = defaultLayoutType || DEFAULT_LAYOUT_TYPE;
    this.settings.defaultMenuCollapsed = typeof defaultMenuCollapsed !== "undefined" ? defaultMenuCollapsed : DEFAULT_MENU_COLLAPSED;
    this.settings.defaultMenuFlipped = typeof defaultMenuFlipped !== "undefined" ? defaultMenuFlipped : DEFAULT_MENU_FLIPPED;
    this.settings.defaultShowDropdownOnHover = typeof defaultShowDropdownOnHover !== "undefined" ? defaultShowDropdownOnHover : DEFAULT_SHOW_DROPDOWN_ON_HOVER;
    this.settings.defaultNavbarFixed = typeof defaultNavbarFixed !== "undefined" ? defaultNavbarFixed : DEFAULT_NAVBAR_FIXED;
    this.settings.defaultFooterFixed = typeof defaultFooterFixed !== "undefined" ? defaultFooterFixed : DEFAULT_FOOTER_FIXED;
    this.settings.availableThemes = availableThemes || TemplateCustomizer.THEMES;
    this.settings.defaultTheme = this._getDefaultTheme(typeof defaultTheme !== "undefined" ? defaultTheme : DEFAULT_THEME);
    this.settings.styles = styles || STYLES;
    this.settings.defaultStyle = defaultStyle || DEFAULT_STYLE;
    this.settings.lang = lang || "en";
    this.pathResolver = pathResolver || function (p) {
      return p;
    };
    if (this.settings.styles.length < 2) {
      var i = this.settings.controls.indexOf("style");
      if (i !== -1) {
        this.settings.controls = this.settings.controls.slice(0, i).concat(this.settings.controls.slice(i + 1));
      }
    }
    this.settings.onSettingsChange = typeof onSettingsChange === "function" ? onSettingsChange : function () {};
    this._loadSettings();
    this._listeners = [];
    this._controls = {};
    this._initDirection();
    this._initStyle();
    this._initTheme();
    this.setLayoutType(this.settings.layoutType, false);
    this.setLayoutMenuFlipped(this.settings.layoutMenuFlipped, false);
    this.setDropdownOnHover(this.settings.showDropdownOnHover, false);
    this.setLayoutNavbarFixed(this.settings.layoutNavbarFixed, false);
    this.setLayoutFooterFixed(this.settings.layoutFooterFixed, false);
    this._setup();
  }
  return _createClass(TemplateCustomizer, [{
    key: "setRtl",
    value: function setRtl(rtl) {
      if (!this._hasControls("rtl")) return;
      this._setSetting("Rtl", String(rtl));
      window.location.reload();
    }
  }, {
    key: "setStyle",
    value: function setStyle(style) {
      if (!this._hasControls("style")) return;
      this._setSetting("Style", ["dark"].indexOf(style) === -1 ? "light" : style);
      window.location.reload();
    }
  }, {
    key: "setTheme",
    value: function setTheme(themeName) {
      var updateStorage = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : true;
      var cb = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : null;
      if (!this._hasControls("themes")) return;
      var theme = this._getThemeByName(themeName);
      if (!theme) return;
      this.settings.theme = theme;
      if (updateStorage) this._setSetting("Theme", themeName);
      var themeUrl = this.pathResolver(this.settings.themesPath + this.settings.cssFilenamePattern.replace("%name%", themeName + (this.settings.style !== "light" ? "-".concat(this.settings.style) : "")));
      this._loadStylesheets(_defineProperty({}, themeUrl, document.querySelector(".template-customizer-theme-css")), cb || function () {});
      if (updateStorage) this.settings.onSettingsChange.call(this, this.settings);
    }
  }, {
    key: "setLayoutType",
    value: function setLayoutType(pos) {
      var updateStorage = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : true;
      if (!this._hasControls("layoutType")) return;
      if (pos !== "static" && pos !== "static-offcanvas" && pos !== "fixed" && pos !== "fixed-offcanvas") return;
      this.settings.layoutType = pos;
      if (updateStorage) this._setSetting("LayoutType", pos);
      window.Helpers.setPosition(pos === "fixed" || pos === "fixed-offcanvas", pos === "static-offcanvas" || pos === "fixed-offcanvas");
      if (updateStorage) this.settings.onSettingsChange.call(this, this.settings);

      // Perfectscrollbar change on Layout change
      var menuScroll = window.Helpers.menuPsScroll;
      var PerfectScrollbarLib = window.PerfectScrollbar;
      if (this.settings.layoutType === "fixed" || this.settings.layoutType === "fixed-offcanvas") {
        // Set perfectscrollbar wheelPropagation false for fixed layout
        if (PerfectScrollbarLib && menuScroll) {
          window.Helpers.menuPsScroll.destroy();
          menuScroll = new PerfectScrollbarLib(document.querySelector(".menu-inner"), {
            suppressScrollX: true,
            wheelPropagation: false
          });
          window.Helpers.menuPsScroll = menuScroll;
        }
      } else if (menuScroll) {
        // Destroy perfectscrollbar for static layout
        window.Helpers.menuPsScroll.destroy();
      }
    }
  }, {
    key: "setLayoutMenuFlipped",
    value: function setLayoutMenuFlipped(flipped) {
      var updateStorage = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : true;
      if (!this._hasControls("layoutMenuFlipped")) return;
      this.settings.layoutMenuFlipped = flipped;
      if (updateStorage) this._setSetting("MenuFlipped", flipped);
      window.Helpers.setFlipped(flipped);
      if (updateStorage) this.settings.onSettingsChange.call(this, this.settings);
    }
  }, {
    key: "setDropdownOnHover",
    value: function setDropdownOnHover(open) {
      var updateStorage = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : true;
      if (!this._hasControls("showDropdownOnHover")) return;
      this.settings.showDropdownOnHover = open;
      if (updateStorage) this._setSetting("ShowDropdownOnHover", open);
      if (window.Helpers.mainMenu) {
        window.Helpers.mainMenu.destroy();
        config.showDropdownOnHover = open;
        var _window = window,
          Menu = _window.Menu;
        window.Helpers.mainMenu = new Menu(document.getElementById("layout-menu"), {
          orientation: "horizontal",
          closeChildren: true,
          showDropdownOnHover: config.showDropdownOnHover
        });
      }
      if (updateStorage) this.settings.onSettingsChange.call(this, this.settings);
    }
  }, {
    key: "setLayoutNavbarFixed",
    value: function setLayoutNavbarFixed(fixed) {
      var updateStorage = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : true;
      if (!this._hasControls("layoutNavbarFixed")) return;
      this.settings.layoutNavbarFixed = fixed;
      if (updateStorage) this._setSetting("FixedNavbar", fixed);
      window.Helpers.setNavbarFixed(fixed);
      if (updateStorage) this.settings.onSettingsChange.call(this, this.settings);
    }
  }, {
    key: "setLayoutFooterFixed",
    value: function setLayoutFooterFixed(fixed) {
      var updateStorage = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : true;
      if (!this._hasControls("layoutFooterFixed")) return;
      this.settings.layoutFooterFixed = fixed;
      if (updateStorage) this._setSetting("FixedFooter", fixed);
      window.Helpers.setFooterFixed(fixed);
      if (updateStorage) this.settings.onSettingsChange.call(this, this.settings);
    }
  }, {
    key: "setLang",
    value: function setLang(lang) {
      var _this = this;
      var force = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : false;
      if (lang === this.settings.lang && !force) return;
      if (!TemplateCustomizer.LANGUAGES[lang]) throw new Error("Language \"".concat(lang, "\" not found!"));
      var t = TemplateCustomizer.LANGUAGES[lang];
      ["panel_header", "panel_sub_header", "theming_header", "theme_header", "style_label", "style_switch_light", "style_switch_dark", "layout_header", "layout_label", "layout_static", "layout_offcanvas", "layout_fixed", "layout_fixed_offcanvas", "layout_flipped_label", "layout_dd_open_label", "layout_navbar_label", "layout_footer_label", "misc_header", "theme_label", "rtl_label"].forEach(function (key) {
        var el = _this.container.querySelector(".template-customizer-t-".concat(key));
        // eslint-disable-next-line no-unused-expressions
        el && (el.textContent = t[key]);
      });
      var tt = t.themes || {};
      var themes = this.container.querySelectorAll(".template-customizer-theme-item") || [];
      for (var i = 0, l = themes.length; i < l; i++) {
        var themeName = themes[i].querySelector('input[type="radio"]').value;
        themes[i].querySelector(".template-customizer-theme-name").textContent = tt[themeName] || this._getThemeByName(themeName).title;
      }
      this.settings.lang = lang;
    }

    // Update theme settings control
  }, {
    key: "update",
    value: function update() {
      if (this._ssr) return;
      var hasNavbar = !!document.querySelector(".layout-navbar");
      var hasMenu = !!document.querySelector(".layout-menu");
      var hasHorizontalMenu = !!document.querySelector(".layout-menu-horizontal.menu, .layout-menu-horizontal .menu");
      var isLayout1 = !!document.querySelector(".layout-wrapper.layout-navbar-full");
      var hasFooter = !!document.querySelector(".content-footer");
      if (this._controls.layoutMenuFlipped) {
        if (!hasMenu) {
          this._controls.layoutMenuFlipped.setAttribute("disabled", "disabled");
          this._controls.layoutMenuFlipped.classList.add("disabled");
        } else {
          this._controls.layoutMenuFlipped.removeAttribute("disabled");
          this._controls.layoutMenuFlipped.classList.remove("disabled");
        }
      }
      if (this._controls.showDropdownOnHover) {
        if (hasMenu) {
          this._controls.showDropdownOnHover.setAttribute("disabled", "disabled");
          this._controls.showDropdownOnHover.classList.add("disabled");
        } else {
          this._controls.showDropdownOnHover.removeAttribute("disabled");
          this._controls.showDropdownOnHover.classList.remove("disabled");
        }
      }
      if (this._controls.layoutNavbarFixed) {
        if (!hasNavbar) {
          this._controls.layoutNavbarFixed.setAttribute("disabled", "disabled");
          this._controls.layoutNavbarFixedW.classList.add("disabled");
        } else {
          this._controls.layoutNavbarFixed.removeAttribute("disabled");
          this._controls.layoutNavbarFixedW.classList.remove("disabled");
        }

        //  Horizontal menu fixed layout - disabled fixed navbar switch
        if (hasHorizontalMenu && hasNavbar && this.settings.layoutType == "fixed") {
          this._controls.layoutNavbarFixed.setAttribute("disabled", "disabled");
          this._controls.layoutNavbarFixedW.classList.add("disabled");
        }
      }
      if (this._controls.layoutFooterFixed) {
        if (!hasFooter) {
          this._controls.layoutFooterFixed.setAttribute("disabled", "disabled");
          this._controls.layoutFooterFixedW.classList.add("disabled");
        } else {
          this._controls.layoutFooterFixed.removeAttribute("disabled");
          this._controls.layoutFooterFixedW.classList.remove("disabled");
        }
      }
      if (this._controls.layoutType) {
        // ? Uncomment If using offcanvas layout
        /*
        if (!hasMenu) {
          this._controls.layoutType.querySelector('[value="static-offcanvas"]').setAttribute('disabled', 'disabled')
          this._controls.layoutType.querySelector('[value="fixed-offcanvas"]').setAttribute('disabled', 'disabled')
        } else {
          this._controls.layoutType.querySelector('[value="static-offcanvas"]').removeAttribute('disabled')
          this._controls.layoutType.querySelector('[value="fixed-offcanvas"]').removeAttribute('disabled')
        }
        */

        // Disable menu layouts options if menu (vertical or horizontal) is not there
        // if ((!hasNavbar && !hasMenu) || (!hasMenu && !isLayout1)) {
        if (hasMenu || hasHorizontalMenu) {
          // (Updated condition)
          this._controls.layoutType.removeAttribute("disabled");
        } else {
          this._controls.layoutType.setAttribute("disabled", "disabled");
        }
      }
    }

    // Clear local storage
  }, {
    key: "clearLocalStorage",
    value: function clearLocalStorage() {
      if (this._ssr) return;
      this._setSetting("Theme", "");
      this._setSetting("Rtl", "");
      this._setSetting("Style", "");
      this._setSetting("MenuFlipped", "");
      this._setSetting("FixedNavbar", "");
      this._setSetting("FixedFooter", "");
      this._setSetting("LayoutType", "");
    }

    // Clear local storage
  }, {
    key: "destroy",
    value: function destroy() {
      if (this._ssr) return;
      this._cleanup();
      this.settings = null;
      this.container.parentNode.removeChild(this.container);
      this.container = null;
    }
  }, {
    key: "_loadSettings",
    value: function _loadSettings() {
      // Get settings

      // const cl = document.documentElement.classList;
      var rtl = this._getSetting("Rtl");
      var style = this._getSetting("Style");
      var collapsedMenu = this._getSetting("LayoutCollapsed"); // Value will be set from main.js
      var flippedMenu = this._getSetting("LayoutMenuFlipped");
      var dropdownOnHover = this._getSetting("ShowDropdownOnHover"); // Value will be set from main.js
      var fixedNavbar = this._getSetting("FixedNavbar");
      var fixedFooter = this._getSetting("FixedFooter");
      var lType = this._getSetting("LayoutType");
      var type;
      if (lType !== "" && ["static", "static-offcanvas", "fixed", "fixed-offcanvas"].indexOf(lType) !== -1) {
        type = lType;
      } else {
        type = this.settings.defaultLayoutType;
      }
      this.settings.layoutType = type;

      // ! Set settings by following priority: Local Storage, Theme Config, HTML Classes
      this.settings.rtl = rtl !== "" ? rtl === "true" : this.settings.defaultTextDir;
      this.settings.style = this.settings.styles.indexOf(style) !== -1 ? style : this.settings.defaultStyle;
      if (this.settings.styles.indexOf(this.settings.style) === -1) {
        // eslint-disable-next-line prefer-destructuring
        this.settings.style = this.settings.styles[0];
      }
      this.settings.layoutMenu = collapsedMenu !== "" ? collapsedMenu === "true" : this.settings.defaultMenuCollapsed;
      this.settings.layoutMenuFlipped = flippedMenu !== "" ? flippedMenu === "true" : this.settings.defaultMenuFlipped;
      this.settings.showDropdownOnHover = dropdownOnHover !== "" ? dropdownOnHover === "true" : this.settings.defaultShowDropdownOnHover;
      this.settings.layoutNavbarFixed = fixedNavbar !== "" ? fixedNavbar === "true" : this.settings.defaultNavbarFixed;
      this.settings.layoutFooterFixed = fixedFooter !== "" ? fixedFooter === "true" : this.settings.defaultFooterFixed;
      this.settings.theme = this._getThemeByName(this._getSetting("Theme"), true);

      // Filter options depending on available controls
      if (!this._hasControls("rtl")) this.settings.rtl = document.documentElement.getAttribute("dir") === "rtl";
      if (!this._hasControls("style")) this.settings.style = cl.contains("dark-style") ? "dark" : "light";
      if (!this._hasControls("layoutType")) this.settings.layoutType = null;
      if (!this._hasControls("layoutMenuFlipped")) this.settings.layoutMenuFlipped = null;
      if (!this._hasControls("showDropdownOnHover")) this.settings.showDropdownOnHover = null;
      if (!this._hasControls("layoutNavbarFixed")) this.settings.layoutNavbarFixed = null;
      if (!this._hasControls("layoutFooterFixed")) this.settings.layoutFooterFixed = null;
      if (!this._hasControls("themes")) this.settings.theme = null;
    }

    // Setup theme settings controls and events
  }, {
    key: "_setup",
    value: function _setup() {
      var _this2 = this;
      var _container = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : document;
      this._cleanup();
      this.container = this._getElementFromString(js_template_customizer_template_customizer);

      // Customizer visibility condition
      //
      var customizerW = this.container;
      if (this.settings.displayCustomizer) customizerW.setAttribute("style", "visibility: visible");else customizerW.setAttribute("style", "visibility: hidden");

      // Open btn
      //
      var openBtn = this.container.querySelector(".template-customizer-open-btn");
      var openBtnCb = function openBtnCb() {
        _this2.container.classList.add("template-customizer-open");
        _this2.update();
        if (_this2._updateInterval) clearInterval(_this2._updateInterval);
        _this2._updateInterval = setInterval(function () {
          _this2.update();
        }, 500);
      };
      openBtn.addEventListener("click", openBtnCb);
      this._listeners.push([openBtn, "click", openBtnCb]);

      // Close btn
      //

      var closeBtn = this.container.querySelector(".template-customizer-close-btn");
      var closeBtnCb = function closeBtnCb() {
        _this2.container.classList.remove("template-customizer-open");
        if (_this2._updateInterval) {
          clearInterval(_this2._updateInterval);
          _this2._updateInterval = null;
        }
      };
      closeBtn.addEventListener("click", closeBtnCb);
      this._listeners.push([closeBtn, "click", closeBtnCb]);

      // RTL
      //

      var rtlW = this.container.querySelector(".template-customizer-misc");
      // ? Hide RTL control in following 2 case
      if (!this._hasControls("rtl") || !rtlSupport) {
        rtlW.parentNode.removeChild(rtlW);
      } else {
        var rtl = rtlW.querySelector("input");
        if (this.settings.rtl) rtl.setAttribute("checked", "checked");
        var rtlCb = function rtlCb(e) {
          _this2._loadingState(true);
          _this2.setRtl(e.target.checked);
        };
        rtl.addEventListener("change", rtlCb);
        this._listeners.push([rtl, "change", rtlCb]);
      }

      // Style

      //

      var styleW = this.container.querySelector(".template-customizer-style");
      if (!this._hasControls("style")) {
        styleW.parentNode.removeChild(styleW);
      } else {
        var style = styleW.querySelector("input");
        if (this.settings.style === "dark") style.setAttribute("checked", "checked");
        var styleCb = function styleCb(e) {
          _this2._loadingState(true);
          if (e.target.checked) {
            _this2.setStyle("dark");
          } else {
            _this2.setStyle("light");
          }
        };
        style.addEventListener("change", styleCb);
        this._listeners.push([style, "change", styleCb]);
      }

      // Theme

      var themesW = this.container.querySelector(".template-customizer-themes");
      if (!this._hasControls("themes")) {
        themesW.parentNode.removeChild(themesW);
      } else {
        var themesWInner = themesW.querySelector(".template-customizer-themes-options");
        this.settings.availableThemes.forEach(function (theme) {
          var themeEl = _this2._getElementFromString("<div class=\"col-12\"><div class=\"form-check\"><input class=\"form-check-input\" type=\"radio\" name=\"themeRadios\" id=\"themeRadios".concat(theme.name, "\" value=\"").concat(theme.name, "\"><label class=\"form-check-label\" for=\"themeRadios").concat(theme.name, "\">").concat(theme.title, "</label></div></div>"));
          themesWInner.appendChild(themeEl);
        });
        themesWInner.querySelector("input[value=\"".concat(this.settings.theme.name, "\"]")).setAttribute("checked", "checked");
        var themeCb = function themeCb(e) {
          if (_this2._loading) return;
          _this2._loading = true;
          _this2._loadingState(true, true);
          _this2.setTheme(e.target.value, true, function () {
            _this2._loading = false;
            _this2._loadingState(false, true);
          });
        };
        themesWInner.addEventListener("change", themeCb);
        this._listeners.push([themesWInner, "change", themeCb]);
      }
      var themingW = this.container.querySelector(".template-customizer-theming");
      if (!this._hasControls("style") && !this._hasControls("themes")) {
        themingW.parentNode.removeChild(themingW);
      }

      // Layout wrapper
      //

      var layoutW = this.container.querySelector(".template-customizer-layout");
      if (!this._hasControls("layoutType layoutNavbarFixed layoutFooterFixed layoutMenuFlipped showDropdownOnHover", true)) {
        layoutW.parentNode.removeChild(layoutW);
      } else {
        // Position
        //

        var layoutTypeW = this.container.querySelector(".template-customizer-layoutType");
        if (!this._hasControls("layoutType")) {
          layoutTypeW.parentNode.removeChild(layoutTypeW);
        } else {
          this._controls.layoutType = layoutTypeW.querySelector(".template-customizer-layouts-options");

          // this._controls.layoutType.value = this.settings.layoutType
          this._controls.layoutType.querySelector("input[value=\"".concat(this.settings.layoutType, "\"]")).setAttribute("checked", "checked");
          var layoutTypeCb = function layoutTypeCb(e) {
            return _this2.setLayoutType(e.target.value);
          };
          this._controls.layoutType.addEventListener("change", layoutTypeCb);
          this._listeners.push([this._controls.layoutType, "change", layoutTypeCb]);
        }

        // Menu flipped
        // ? Uncomment If needed

        /* this._controls.layoutMenuFlipped = this.container.querySelector('.template-customizer-layoutMenuFlipped')
         if (!this._hasControls('layoutMenuFlipped')) {
          this._controls.layoutMenuFlipped.parentNode.removeChild(this._controls.layoutMenuFlipped)
        } else {
          this._controls.layoutMenuFlipped = this._controls.layoutMenuFlipped.querySelector('input')
           if (this.settings.layoutMenuFlipped) this._controls.layoutMenuFlipped.setAttribute('checked', 'checked')
           const layoutMenuFlipped = e => this.setLayoutMenuFlipped(e.target.checked)
          this._controls.layoutMenuFlipped.addEventListener('change', layoutMenuFlipped)
          this._listeners.push([this._controls.layoutMenuFlipped, 'change', layoutMenuFlipped])
        } */

        // Menu open
        //

        this._controls.showDropdownOnHover = this.container.querySelector(".template-customizer-showDropdownOnHover");
        if (!this._hasControls("showDropdownOnHover")) {
          this._controls.showDropdownOnHover.parentNode.removeChild(this._controls.showDropdownOnHover);
        } else {
          this._controls.showDropdownOnHover = this._controls.showDropdownOnHover.querySelector("input");
          if (this.settings.showDropdownOnHover) this._controls.showDropdownOnHover.setAttribute("checked", "checked");
          var showDropdownOnHover = function showDropdownOnHover(e) {
            return _this2.setDropdownOnHover(e.target.checked);
          };
          this._controls.showDropdownOnHover.addEventListener("change", showDropdownOnHover);
          this._listeners.push([this._controls.showDropdownOnHover, "change", showDropdownOnHover]);
        }

        // Navbar
        //

        this._controls.layoutNavbarFixedW = this.container.querySelector(".template-customizer-layoutNavbarFixed");
        if (!this._hasControls("layoutNavbarFixed")) {
          this._controls.layoutNavbarFixedW.parentNode.removeChild(this._controls.layoutNavbarFixedW);
        } else {
          this._controls.layoutNavbarFixed = this._controls.layoutNavbarFixedW.querySelector("input");
          if (this.settings.layoutNavbarFixed) this._controls.layoutNavbarFixed.setAttribute("checked", "checked");
          var layoutNavbarFixedCb = function layoutNavbarFixedCb(e) {
            return _this2.setLayoutNavbarFixed(e.target.checked);
          };
          this._controls.layoutNavbarFixed.addEventListener("change", layoutNavbarFixedCb);
          this._listeners.push([this._controls.layoutNavbarFixed, "change", layoutNavbarFixedCb]);
        }

        // Footer
        //

        this._controls.layoutFooterFixedW = this.container.querySelector(".template-customizer-layoutFooterFixed");
        if (!this._hasControls("layoutFooterFixed")) {
          this._controls.layoutFooterFixedW.parentNode.removeChild(this._controls.layoutFooterFixedW);
        } else {
          this._controls.layoutFooterFixed = this._controls.layoutFooterFixedW.querySelector("input");
          if (this.settings.layoutFooterFixed) this._controls.layoutFooterFixed.setAttribute("checked", "checked");
          var layoutFooterFixedCb = function layoutFooterFixedCb(e) {
            return _this2.setLayoutFooterFixed(e.target.checked);
          };
          this._controls.layoutFooterFixed.addEventListener("change", layoutFooterFixedCb);
          this._listeners.push([this._controls.layoutFooterFixed, "change", layoutFooterFixedCb]);
        }
      }

      // Set language
      this.setLang(this.settings.lang, true);

      // Append container
      if (_container === document) {
        if (_container.body) {
          _container.body.appendChild(this.container);
        } else {
          window.addEventListener("DOMContentLoaded", function () {
            return _container.body.appendChild(_this2.container);
          });
        }
      } else {
        _container.appendChild(this.container);
      }
    }
  }, {
    key: "_initDirection",
    value: function _initDirection() {
      if (this._hasControls("rtl")) document.documentElement.setAttribute("dir", this.settings.rtl ? "rtl" : "ltr");
    }

    // Init template styles
  }, {
    key: "_initStyle",
    value: function _initStyle() {
      if (!this._hasControls("style")) return;
      var style = this.settings.style;
      this._insertStylesheet("template-customizer-core-css", this.pathResolver(this.settings.cssPath + this.settings.cssFilenamePattern.replace("%name%", "core".concat(style !== "light" ? "-".concat(style) : ""))));
      // ? Uncomment if needed
      /*
      this._insertStylesheet(
        'template-customizer-bootstrap-css',
        this.pathResolver(
          this.settings.cssPath +
            this.settings.cssFilenamePattern.replace('%name%', `bootstrap${style !== 'light' ? `-${style}` : ''}`)
        )
      )
      this._insertStylesheet(
        'template-customizer-bsextended-css',
        this.pathResolver(
          this.settings.cssPath +
            this.settings.cssFilenamePattern.replace(
              '%name%',
              `bootstrap-extended${style !== 'light' ? `-${style}` : ''}`
            )
        )
      )
      this._insertStylesheet(
        'template-customizer-components-css',
        this.pathResolver(
          this.settings.cssPath +
            this.settings.cssFilenamePattern.replace('%name%', `components${style !== 'light' ? `-${style}` : ''}`)
        )
      )
      this._insertStylesheet(
        'template-customizer-colors-css',
        this.pathResolver(
          this.settings.cssPath +
            this.settings.cssFilenamePattern.replace('%name%', `colors${style !== 'light' ? `-${style}` : ''}`)
        )
      )
      */

      var classesToRemove = style === "light" ? ["dark-style"] : ["light-style"];
      classesToRemove.forEach(function (cls) {
        document.documentElement.classList.remove(cls);
      });
      document.documentElement.classList.add("".concat(style, "-style"));
    }

    // Init theme style
  }, {
    key: "_initTheme",
    value: function _initTheme() {
      if (this._hasControls("themes")) {
        this._insertStylesheet("template-customizer-theme-css", this.pathResolver(this.settings.themesPath + this.settings.cssFilenamePattern.replace("%name%", this.settings.theme.name + (this.settings.style !== "light" ? "-".concat(this.settings.style) : ""))));
      } else {
        // If theme control is not enabled, get the current theme from localstorage else display default theme
        var theme = this._getSetting("Theme");
        this._insertStylesheet("template-customizer-theme-css", this.pathResolver(this.settings.themesPath + this.settings.cssFilenamePattern.replace("%name%", theme ? theme : "theme-default".concat(this.settings.style !== "light" ? "-".concat(this.settings.style) : ""))));
      }
    }
  }, {
    key: "_insertStylesheet",
    value: function _insertStylesheet(className, href) {
      var curLink = document.querySelector(".".concat(className));
      if (typeof document.documentMode === "number" && document.documentMode < 11) {
        if (!curLink) return;
        if (href === curLink.getAttribute("href")) return;
        var link = document.createElement("link");
        link.setAttribute("rel", "stylesheet");
        link.setAttribute("type", "text/css");
        link.className = className;
        link.setAttribute("href", href);
        curLink.parentNode.insertBefore(link, curLink.nextSibling);
      } else {
        document.write("<link rel=\"stylesheet\" type=\"text/css\" href=\"".concat(href, "\" class=\"").concat(className, "\">"));
      }
      curLink.parentNode.removeChild(curLink);
    }
  }, {
    key: "_loadStylesheets",
    value: function _loadStylesheets(stylesheets, cb) {
      var paths = Object.keys(stylesheets);
      var count = paths.length;
      var loaded = 0;
      function loadStylesheet(path, curLink, _cb) {
        var link = document.createElement("link");
        link.setAttribute("href", path);
        link.setAttribute("rel", "stylesheet");
        link.setAttribute("type", "text/css");
        link.className = curLink.className;
        var sheet = "sheet" in link ? "sheet" : "styleSheet";
        var cssRules = "sheet" in link ? "cssRules" : "rules";
        var intervalId;
        var timeoutId = setTimeout(function () {
          clearInterval(intervalId);
          clearTimeout(timeoutId);
          curLink.parentNode.removeChild(link);
          _cb(false, path);
        }, 15000);
        intervalId = setInterval(function () {
          try {
            if (link[sheet] && link[sheet][cssRules].length) {
              clearInterval(intervalId);
              clearTimeout(timeoutId);
              curLink.parentNode.removeChild(curLink);
              _cb(true);
            }
          } catch (e) {
            // Catch error
          }
        }, 10);
        curLink.parentNode.insertBefore(link, curLink.nextSibling);
      }
      function stylesheetCallBack() {
        if ((loaded += 1) >= count) {
          cb();
        }
      }
      for (var i = 0; i < paths.length; i++) {
        loadStylesheet(paths[i], stylesheets[paths[i]], stylesheetCallBack());
      }
    }
  }, {
    key: "_loadingState",
    value: function _loadingState(enable, themes) {
      this.container.classList[enable ? "add" : "remove"]("template-customizer-loading".concat(themes ? "-theme" : ""));
    }
  }, {
    key: "_getElementFromString",
    value: function _getElementFromString(str) {
      var wrapper = document.createElement("div");
      wrapper.innerHTML = str;
      return wrapper.firstChild;
    }

    // Set settings in LocalStorage with layout & key
  }, {
    key: "_getSetting",
    value: function _getSetting(key) {
      var result = null;
      var layoutName = this._getLayoutName();
      try {
        result = localStorage.getItem("templateCustomizer-".concat(layoutName, "--").concat(key));
      } catch (e) {
        // Catch error
      }
      return String(result || "");
    }

    // Set settings in LocalStorage with layout & key
  }, {
    key: "_setSetting",
    value: function _setSetting(key, val) {
      var layoutName = this._getLayoutName();
      try {
        localStorage.setItem("templateCustomizer-".concat(layoutName, "--").concat(key), String(val));
      } catch (e) {
        // Catch Error
      }
    }

    // Get layout name to set unique
  }, {
    key: "_getLayoutName",
    value: function _getLayoutName() {
      return document.getElementsByTagName("HTML")[0].getAttribute("data-template");
    }
  }, {
    key: "_removeListeners",
    value: function _removeListeners() {
      for (var i = 0, l = this._listeners.length; i < l; i++) {
        this._listeners[i][0].removeEventListener(this._listeners[i][1], this._listeners[i][2]);
      }
    }
  }, {
    key: "_cleanup",
    value: function _cleanup() {
      this._removeListeners();
      this._listeners = [];
      this._controls = {};
      if (this._updateInterval) {
        clearInterval(this._updateInterval);
        this._updateInterval = null;
      }
    }
  }, {
    key: "_ssr",
    get: function get() {
      return typeof window === "undefined";
    }

    // Check controls availability
  }, {
    key: "_hasControls",
    value: function _hasControls(controls) {
      var _this3 = this;
      var oneOf = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : false;
      return controls.split(" ").reduce(function (result, control) {
        if (_this3.settings.controls.indexOf(control) !== -1) {
          if (oneOf || result !== false) result = true;
        } else if (!oneOf || result !== true) result = false;
        return result;
      }, null);
    }

    // Get the default theme
  }, {
    key: "_getDefaultTheme",
    value: function _getDefaultTheme(themeId) {
      var theme;
      if (typeof themeId === "string") {
        theme = this._getThemeByName(themeId, false);
      } else {
        theme = this.settings.availableThemes[themeId];
      }
      if (!theme) {
        throw new Error("Theme ID \"".concat(themeId, "\" not found!"));
      }
      return theme;
    }

    // Get theme by themeId/themeName
  }, {
    key: "_getThemeByName",
    value: function _getThemeByName(themeName) {
      var returnDefault = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : false;
      var themes = this.settings.availableThemes;
      for (var i = 0, l = themes.length; i < l; i++) {
        if (themes[i].name === themeName) return themes[i];
      }
      return returnDefault ? this.settings.defaultTheme : null;
    }
  }]);
}(); // Themes
TemplateCustomizer.THEMES = [{
  name: "theme-default",
  title: "Default"
}, {
  name: "theme-semi-dark",
  title: "Semi Dark"
}, {
  name: "theme-bordered",
  title: "Bordered"
}];

// Theme setting language
TemplateCustomizer.LANGUAGES = {
  en: {
    panel_header: "TEMPLATE CUSTOMIZER",
    panel_sub_header: "Customize and preview in real time",
    theming_header: "THEMING",
    theme_header: "THEME",
    theme_label: "Themes",
    style_label: "Style (Mode)",
    style_switch_light: "Light",
    style_switch_dark: "Dark",
    layout_header: "LAYOUT",
    layout_label: "Layout (Menu)",
    layout_static: "Static",
    layout_offcanvas: "Offcanvas",
    layout_fixed: "Fixed",
    layout_fixed_offcanvas: "Fixed offcanvas",
    layout_flipped_label: "Menu flipped",
    layout_dd_open_label: "Dropdown on hover",
    layout_navbar_label: "Fixed navbar",
    layout_footer_label: "Fixed footer",
    misc_header: "MISC",
    rtl_label: "RTL direction"
  },
  fr: {
    panel_header: "MODÈLE DE PERSONNALISATION",
    panel_sub_header: "Personnalisez et prévisualisez en temps réel",
    theming_header: "THÉMATISATION",
    theme_header: "THÈME",
    theme_label: "Thèmes",
    style_label: "Style (Mode)",
    style_switch_light: "Léger",
    style_switch_dark: "Sombre",
    layout_header: "DISPOSITION",
    layout_label: "Mise en page (Menu)",
    layout_static: "Statique",
    layout_offcanvas: "Hors toile",
    layout_fixed: "Fixé",
    layout_fixed_offcanvas: "Fixe hors toile",
    layout_flipped_label: "Menu inversé",
    layout_dd_open_label: "Liste déroulante au survol",
    layout_navbar_label: "Barre de navigation fixe",
    layout_footer_label: "Pied de page fixe",
    misc_header: "DIVERS",
    rtl_label: "Sens RTL"
  },
  de: {
    panel_header: "VORLAGEN-ANPASSER",
    panel_sub_header: "Anpassen und Vorschau in Echtzeit",
    theming_header: "THEMEN",
    theme_header: "THEMA",
    theme_label: "Themen",
    style_label: "Stil (Modus)",
    style_switch_light: "Hell",
    style_switch_dark: "Dunkel",
    layout_header: "LAYOUT",
    layout_label: "Layout (Speisekarte)",
    layout_static: "Statisch",
    layout_offcanvas: "Leinwand",
    layout_fixed: "Fest",
    layout_fixed_offcanvas: "Außerhalb der Leinwand behoben",
    layout_flipped_label: "Menü umgedreht",
    layout_dd_open_label: "Dropdown beim Hover",
    layout_navbar_label: "Navigationsleiste behoben",
    layout_footer_label: "Feste Fußzeile",
    misc_header: "VERSCHIEDENES",
    rtl_label: "RTL-Regie"
  },
  pt: {
    panel_header: "PERSONALIZADOR DE MODELO",
    panel_sub_header: "Personalize e visualize em tempo real",
    theming_header: "TEMAS",
    theme_header: "TEMA",
    theme_label: "Temas",
    style_label: "Estilo (Modo)",
    style_switch_light: "Luz",
    style_switch_dark: "Escuro",
    layout_header: "ESQUEMA",
    layout_label: "Esquema (Cardápio)",
    layout_static: "Estático",
    layout_offcanvas: "Offcanvas",
    layout_fixed: "Fixo",
    layout_fixed_offcanvas: "Offscreen fixo",
    layout_flipped_label: "Menu invertido",
    layout_dd_open_label: "Suspensão ao passar o mouse",
    layout_navbar_label: "Barra de navegação fixa",
    layout_footer_label: "Rodapé fixo",
    misc_header: "DIVERSOS",
    rtl_label: "Direção RTL"
  }
};


/***/ }),

/***/ 433:
/***/ ((module, __webpack_exports__, __webpack_require__) => {

/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   A: () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _node_modules_css_loader_dist_runtime_noSourceMaps_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(601);
/* harmony import */ var _node_modules_css_loader_dist_runtime_noSourceMaps_js__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(_node_modules_css_loader_dist_runtime_noSourceMaps_js__WEBPACK_IMPORTED_MODULE_0__);
/* harmony import */ var _node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(314);
/* harmony import */ var _node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1___default = /*#__PURE__*/__webpack_require__.n(_node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1__);
/* harmony import */ var _node_modules_css_loader_dist_runtime_getUrl_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(417);
/* harmony import */ var _node_modules_css_loader_dist_runtime_getUrl_js__WEBPACK_IMPORTED_MODULE_2___default = /*#__PURE__*/__webpack_require__.n(_node_modules_css_loader_dist_runtime_getUrl_js__WEBPACK_IMPORTED_MODULE_2__);
// Imports



var ___CSS_LOADER_URL_IMPORT_0___ = new URL(/* asset import */ __webpack_require__(55), __webpack_require__.b);
var ___CSS_LOADER_EXPORT___ = _node_modules_css_loader_dist_runtime_api_js__WEBPACK_IMPORTED_MODULE_1___default()((_node_modules_css_loader_dist_runtime_noSourceMaps_js__WEBPACK_IMPORTED_MODULE_0___default()));
var ___CSS_LOADER_URL_REPLACEMENT_0___ = _node_modules_css_loader_dist_runtime_getUrl_js__WEBPACK_IMPORTED_MODULE_2___default()(___CSS_LOADER_URL_IMPORT_0___);
// Module
___CSS_LOADER_EXPORT___.push([module.id, `#template-customizer{font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif,"Apple Color Emoji","Segoe UI Emoji","Segoe UI Symbol" !important;font-size:inherit !important;position:fixed;top:0;right:0;height:100%;z-index:99999999;display:-webkit-box;display:-ms-flexbox;display:flex;-webkit-box-orient:vertical;-webkit-box-direction:normal;-ms-flex-direction:column;flex-direction:column;width:370px;background:#fff;-webkit-box-shadow:0 0 20px 0 rgba(0,0,0,.2);box-shadow:0 0 20px 0 rgba(0,0,0,.2);-webkit-transition:all .2s ease-in;-o-transition:all .2s ease-in;transition:all .2s ease-in;-webkit-transform:translateX(390px);-ms-transform:translateX(390px);transform:translateX(390px)}#template-customizer h5{position:relative;font-size:11px;font-weight:600}#template-customizer>h5{flex:0 0 auto}#template-customizer .disabled{color:#d1d2d3 !important}#template-customizer.template-customizer-open{-webkit-transition-delay:.1s;-o-transition-delay:.1s;transition-delay:.1s;-webkit-transform:none !important;-ms-transform:none !important;transform:none !important}#template-customizer .template-customizer-open-btn{position:absolute;top:180px;left:0;z-index:-1;display:block;width:42px;height:42px;border-top-left-radius:15%;border-bottom-left-radius:15%;background:#333;color:#fff !important;text-align:center;font-size:18px !important;line-height:42px;opacity:1;-webkit-transition:all .1s linear .2s;-o-transition:all .1s linear .2s;transition:all .1s linear .2s;-webkit-transform:translateX(-62px);-ms-transform:translateX(-62px);transform:translateX(-62px)}@media(max-width: 991.98px){#template-customizer .template-customizer-open-btn{top:145px}}.dark-style #template-customizer .template-customizer-open-btn{background:#555}#template-customizer .template-customizer-open-btn::before{content:"";width:22px;height:22px;display:block;background-size:100% 100%;position:absolute;background-image:url(${___CSS_LOADER_URL_REPLACEMENT_0___});margin:10px}.customizer-hide #template-customizer .template-customizer-open-btn{display:none}[dir=rtl] #template-customizer .template-customizer-open-btn{border-radius:0;border-top-right-radius:15%;border-bottom-right-radius:15%}[dir=rtl] #template-customizer .template-customizer-open-btn::before{margin-left:-2px}#template-customizer.template-customizer-open .template-customizer-open-btn{opacity:0;-webkit-transition-delay:0s;-o-transition-delay:0s;transition-delay:0s;-webkit-transform:none !important;-ms-transform:none !important;transform:none !important}#template-customizer .template-customizer-close-btn{position:absolute;top:32px;right:0;display:block;font-size:20px;-webkit-transform:translateY(-50%);-ms-transform:translateY(-50%);transform:translateY(-50%)}#template-customizer .template-customizer-inner{position:relative;overflow:auto;-webkit-box-flex:0;-ms-flex:0 1 auto;flex:0 1 auto;opacity:1;-webkit-transition:opacity .2s;-o-transition:opacity .2s;transition:opacity .2s}#template-customizer .template-customizer-inner>div:first-child>hr:first-of-type{display:none !important}#template-customizer .template-customizer-inner>div:first-child>h5:first-of-type{padding-top:0 !important}#template-customizer .template-customizer-themes-inner{position:relative;opacity:1;-webkit-transition:opacity .2s;-o-transition:opacity .2s;transition:opacity .2s}#template-customizer .template-customizer-theme-item{display:-webkit-box;display:-ms-flexbox;display:flex;-webkit-box-align:center;align-items:center;-ms-flex-align:center;-webkit-box-flex:1;-ms-flex:1 1 100%;flex:1 1 100%;-webkit-box-pack:justify;-ms-flex-pack:justify;justify-content:space-between;margin-bottom:10px;padding:0 24px;width:100%;cursor:pointer}#template-customizer .template-customizer-theme-item input{position:absolute;z-index:-1;opacity:0}#template-customizer .template-customizer-theme-item input~span{opacity:.25;-webkit-transition:all .2s;-o-transition:all .2s;transition:all .2s}#template-customizer .template-customizer-theme-item .template-customizer-theme-checkmark{display:inline-block;width:6px;height:12px;border-right:1px solid;border-bottom:1px solid;opacity:0;-webkit-transition:all .2s;-o-transition:all .2s;transition:all .2s;-webkit-transform:rotate(45deg);-ms-transform:rotate(45deg);transform:rotate(45deg)}[dir=rtl] #template-customizer .template-customizer-theme-item .template-customizer-theme-checkmark{border-right:none;border-left:1px solid;-webkit-transform:rotate(-45deg);-ms-transform:rotate(-45deg);transform:rotate(-45deg)}#template-customizer .template-customizer-theme-item input:checked:not([disabled])~span,#template-customizer .template-customizer-theme-item:hover input:not([disabled])~span{opacity:1}#template-customizer .template-customizer-theme-item input:checked:not([disabled])~span .template-customizer-theme-checkmark{opacity:1}#template-customizer .template-customizer-theme-colors span{display:block;margin:0 1px;width:10px;height:10px;border-radius:50%;-webkit-box-shadow:0 0 0 1px rgba(0,0,0,.1) inset;box-shadow:0 0 0 1px rgba(0,0,0,.1) inset}#template-customizer.template-customizer-loading .template-customizer-inner,#template-customizer.template-customizer-loading-theme .template-customizer-themes-inner{opacity:.2}#template-customizer.template-customizer-loading .template-customizer-inner::after,#template-customizer.template-customizer-loading-theme .template-customizer-themes-inner::after{content:"";position:absolute;top:0;right:0;bottom:0;left:0;z-index:999;display:block}.layout-menu-100vh #template-customizer{height:100vh}[dir=rtl] #template-customizer{right:auto;left:0;-webkit-transform:translateX(-390px);-ms-transform:translateX(-390px);transform:translateX(-390px)}[dir=rtl] #template-customizer .template-customizer-open-btn{right:0;left:auto;-webkit-transform:translateX(62px);-ms-transform:translateX(62px);transform:translateX(62px)}[dir=rtl] #template-customizer .template-customizer-close-btn{right:auto;left:0}#template-customizer .template-customizer-layouts-options[disabled]{opacity:.5;pointer-events:none}[dir=rtl] .template-customizer-t-style_switch_light{padding-right:0 !important}`, ""]);
// Exports
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (___CSS_LOADER_EXPORT___);


/***/ })

},
/******/ __webpack_require__ => { // webpackRuntimeModules
/******/ var __webpack_exec__ = (moduleId) => (__webpack_require__(__webpack_require__.s = moduleId))
/******/ __webpack_require__.O(0, [96], () => (__webpack_exec__(290)));
/******/ var __webpack_exports__ = __webpack_require__.O();
/******/ return __webpack_exports__;
/******/ }
]);
});