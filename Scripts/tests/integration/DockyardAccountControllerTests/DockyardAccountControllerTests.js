///// <reference path="../../../app/_all.ts" />
///// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />
var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var controller;
        (function (controller) {
            describe("Dockyard Account Controller ", function () {
                var controllerPoint = "/dockyardaccount/";
                var errorHandler = function (response, done) {
                    done.fail(response.responseText);
                };
                var validationRules = function (data, status) {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                };
                beforeAll(function () {
                    $(document).ajaxError(errorHandler);
                    $.ajaxSetup({ async: false, dataType: "html", contentType: "text/json" });
                });
                it("LogOff should return a page", function () {
                    $.get(controllerPoint + "logoff")
                        .done(function (data, status) {
                        validationRules(data, status);
                    });
                });
                it("RegistrationSuccessful action should return the page", function () {
                    $.get(controllerPoint + "registrationsuccessful")
                        .done(function (data, status) {
                        validationRules(data, status);
                    });
                });
                it("Confirm action should return the page", function () {
                    $.get(controllerPoint + "confirm", { email: null })
                        .done(function (data, status) {
                        validationRules(data, status);
                    });
                });
                it("Login action should return the page", function () {
                    $.post(controllerPoint + "login", { Login: "12345", Password: "12345" })
                        .done(function (data, status) {
                        validationRules(data, status);
                    });
                });
                it("Register action should return the page", function () {
                    $.get(controllerPoint + "register")
                        .done(function (data, status) {
                        validationRules(data, status);
                    });
                });
                it("ProcessRegistration action should return the page", function () {
                    $.post(controllerPoint + "processregistration")
                        .done(function (data, status) {
                        validationRules(data, status);
                    });
                });
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=DockyardAccountControllerTests.js.map