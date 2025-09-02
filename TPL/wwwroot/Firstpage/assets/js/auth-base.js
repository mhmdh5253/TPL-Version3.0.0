/**
 *  Pages Authentication
 */

'use strict';
const formAuthentication = document.querySelector('#formAuthentication');

document.addEventListener('DOMContentLoaded', function (e) {
    (function () {
        // Form validation for Add new record
        if (formAuthentication) {
            const fv = FormValidation.formValidation(formAuthentication, {
                fields: {
                    inputUsername: {
                        validators: {
                            notEmpty: {
                                message: 'لطفا نام کاربری را وارد کنید'
                            },
                            stringLength: {
                                min: 6,
                                message: 'نام کاربری باید بیش از 6 کاراکتر باشد'
                            }
                        }
                    },
                    inputEmailAddress: {
                        validators: {
                            notEmpty: {
                                message: 'لطفا ایمیل خود را وارد کنید'
                            },
                            emailAddress: {
                                message: 'لطفا یک آدرس ایمیل معتبر وارد کنید'
                            }
                        }
                    },
                    'email-username': {
                        validators: {
                            notEmpty: {
                                message: 'لطفا ایمیل / نام کاربری را وارد کنید'
                            },
                            stringLength: {
                                min: 6,
                                message: 'نام کاربری باید بیش از 6 کاراکتر باشد'
                            }
                        }
                    },
                    inputPassword: {
                        validators: {
                            notEmpty: {
                                message: 'لطفا رمز عبور خود را وارد کنید'
                            },
                            stringLength: {
                                min: 6,
                                message: 'رمز عبور باید بیش از 6 کاراکتر باشد'
                            }
                        }
                    },
                    'confirm-password': {
                        validators: {
                            notEmpty: {
                                message: 'لطفا رمز عبور را تایید کنید'
                            },
                            identical: {
                                compare: function () {
                                    return formAuthentication.querySelector('[name="inputPassword"]').value;
                                },
                                message: 'رمز عبور و تایید آن یکسان نیستند'
                            },
                            stringLength: {
                                min: 6,
                                message: 'رمز عبور باید بیش از 6 کاراکتر باشد'
                            }
                        }
                    },
                    inputTerms: {
                        validators: {
                            notEmpty: {
                                message: 'لطفا با قوانین و مقررات موافقت کنید'
                            }
                        }
                    }
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap5: new FormValidation.plugins.Bootstrap5({
                        eleValidClass: '',
                        rowSelector: '.mb-3'
                    }),
                    submitButton: new FormValidation.plugins.SubmitButton(),

                    defaultSubmit: new FormValidation.plugins.DefaultSubmit(),
                    autoFocus: new FormValidation.plugins.AutoFocus()
                },
                init: instance => {
                    instance.on('plugins.message.placed', function (e) {
                        if (e.element.parentElement.classList.contains('input-group')) {
                            e.element.parentElement.insertAdjacentElement('afterend', e.messageElement);
                        }
                    });
                }
            });
        }

    })();
});
const togglePassword = document.querySelector("#togglePassword");
const password = document.querySelector("#inputPassword");

togglePassword.addEventListener("click", function () {
    // toggle the type attribute
    const type = password.getAttribute("type") === "password" ? "text" : "password";
    password.setAttribute("type", type);

    // toggle the icon

    this.classList.toggle("bx-show");
    this.classList.toggle("bx-hide");
});