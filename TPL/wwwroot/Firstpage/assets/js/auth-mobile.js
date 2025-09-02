/**
 *  Pages Authentication mobile
 */

'use strict';
const countdown = document.querySelector('#verification-countdown');
const resendVerification = document.querySelector('#resend-verification');

resendVerification.addEventListener("click", function () {
    this.classList.add("d-none");
    countdown.classList.remove("d-none");
    timer(120);
});


let timerOn = true;

function timer(remaining) {
    var m = Math.floor(remaining / 60);
    var s = remaining % 60;

    m = m < 10 ? '0' + m : m;
    s = s < 10 ? '0' + s : s;
    countdown.innerHTML = m + ':' + s;
    remaining -= 1;

    if(remaining >= 0 && timerOn) {
        setTimeout(function() {
            timer(remaining);
        }, 1000);
        return;
    }

    if(!timerOn) {
        // Do validate stuff here
        return;
    }


    resendVerification.classList.remove('d-none');
    countdown.classList.add('d-none');
}
// start timer
timer(120);