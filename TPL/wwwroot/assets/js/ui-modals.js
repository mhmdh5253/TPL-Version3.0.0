"use strict";
!function() {
    const e = document.querySelector("#animation-dropdown"),
          t = document.querySelector("#animationModal");
    
    if (e) {
        e.onchange = function() {
            t.classList = "";
            t.classList.add("modal", "animate__animated", this.value);
        };
    }
 
    [].slice.call(document.querySelectorAll('[data-bs-toggle="modal"]')).map(function(e) {
        e.onclick = function() {
            var target = this.getAttribute("data-bs-target"),
                videoUrl = this.getAttribute("data-theVideo") + "?autoplay=1",
                iframe = document.querySelector(target + " iframe");
            if (iframe) {
                iframe.setAttribute("src", videoUrl);
            }
        };
    });
    
    document.querySelectorAll(".carousel").forEach(t => {
        t.addEventListener("slide.bs.carousel", e => {
            const height = $(e.relatedTarget).height();
            $(t).find(".active.carousel-item").parent().animate({
                height: height
            }, 500);
        });
    });
}();
