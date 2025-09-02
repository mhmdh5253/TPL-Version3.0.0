"use strict";
document.addEventListener("DOMContentLoaded", function () {
    const e = document.querySelector(".credit-card-mask"),
        t = document.querySelector(".expiry-date-mask"),
        n = document.querySelector(".cvv-code-mask"),
        a = document.querySelector(".btn-reset");
    let o;

    const resultModals = document.getElementsByClassName("resultModal");

    Array.from(resultModals).forEach((modal) => {
        modal.addEventListener("show.bs.modal", function () {
            if (e) {
                o = new Cleave(e, {
                    creditCard: true,
                    onCreditCardTypeChanged: function (e) {
                        document.querySelector(".card-type").innerHTML = e !== "" && e !== "unknown" ? '<img src="' + assetsPath + 'img/icons/payments/' + e + '-cc.png" class="cc-icon-image" height="28"/>' : "";
                    }
                });
            }
        });
    });

});
