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
return (self["webpackChunktpl"] = self["webpackChunktpl"] || []).push([[789],{

/***/ 27:
/***/ (() => {

// Add onHover event for dropdowns
;
(function ($) {
  if (!$ || !$.fn) return;
  var SELECTOR = "[data-bs-toggle=dropdown][data-trigger=hover]";
  var TIMEOUT = 150;
  function openDropdown($i) {
    var t = $i.data("dd-timeout");
    if (t) {
      clearTimeout(t);
      t = null;
      $i.data("dd-timeout", t);
    }
    if ($i.attr("aria-expanded") !== "true") $i.dropdown("toggle");
  }
  function closeDropdown($i) {
    var t = $i.data("dd-timeout");
    if (t) clearTimeout(t);
    t = setTimeout(function () {
      var t2 = $i.data("dd-timeout");
      if (t2) {
        clearTimeout(t2);
        t2 = null;
        $i.data("dd-timeout", t2);
      }
      if ($i.attr("aria-expanded") === "true") $i.dropdown("toggle");
    }, TIMEOUT);
    $i.data("dd-timeout", t);
  }
  $(function () {
    $("body").on("mouseenter", "".concat(SELECTOR, ", ").concat(SELECTOR, " ~ .dropdown-menu"), function () {
      var $toggle = $(this).hasClass("dropdown-toggle") ? $(this) : $(this).prev(".dropdown-toggle");
      var $dropdown = $(this).hasClass("dropdown-menu") ? $(this) : $(this).next(".dropdown-menu");
      if (window.getComputedStyle($dropdown[0], null).getPropertyValue("position") === "static") return;

      // Set hovered flag
      if ($(this).is(SELECTOR)) {
        $(this).data("hovered", true);
      }
      openDropdown($(this).hasClass("dropdown-toggle") ? $(this) : $(this).prev(".dropdown-toggle"));
    }).on("mouseleave", "".concat(SELECTOR, ", ").concat(SELECTOR, " ~ .dropdown-menu"), function () {
      var $toggle = $(this).hasClass("dropdown-toggle") ? $(this) : $(this).prev(".dropdown-toggle");
      var $dropdown = $(this).hasClass("dropdown-menu") ? $(this) : $(this).next(".dropdown-menu");
      if (window.getComputedStyle($dropdown[0], null).getPropertyValue("position") === "static") return;

      // Remove hovered flag
      if ($(this).is(SELECTOR)) {
        $(this).data("hovered", false);
      }
      closeDropdown($(this).hasClass("dropdown-toggle") ? $(this) : $(this).prev(".dropdown-toggle"));
    }).on("hide.bs.dropdown", function (e) {
      if ($(this).find(SELECTOR).data("hovered")) e.preventDefault();
    });
  });
})(window.jQuery);

/***/ })

},
/******/ __webpack_require__ => { // webpackRuntimeModules
/******/ var __webpack_exec__ = (moduleId) => (__webpack_require__(__webpack_require__.s = moduleId))
/******/ var __webpack_exports__ = (__webpack_exec__(27));
/******/ return __webpack_exports__;
/******/ }
]);
});