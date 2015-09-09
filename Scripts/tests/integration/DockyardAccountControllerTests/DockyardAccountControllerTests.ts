///// <reference path="../../../app/_all.ts" />
///// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    describe("Dockyard Account Controller ", function () {

        var controllerPoint = "/dockyardaccount/";

        var errorHandler = function (response, done) {
            done.fail(response.responseText);
        }


        var validationRules = function (data: any, status: string) {
            expect(data).not.toBe(null);
            expect(status).toBe("success");
        }


        beforeAll(function () {
            $(document).ajaxError(errorHandler);
            $.ajaxSetup({ async: false, dataType: "html", contentType: "text/json" });
        });


        it("LogOff should return a page", function () {
            $.get(controllerPoint + "logoff")
                .done((data: any, status: string) => {
                    validationRules(data, status);
                });
        });


        it("RegistrationSuccessful action should return the page", function () {
            $.get(controllerPoint + "registrationsuccessful")
                .done((data: any, status: string) => {
                    validationRules(data, status);
                });
        });
        
        it("Confirm action should return the page", function () {
            $.get(controllerPoint + "confirm", { email: null })
                .done((data: any, status: string) => {
                    validationRules(data, status);
                });
        });

        it("Login action should return the page", function () {
            $.post(controllerPoint + "login",
                { Login: "12345", Password: "12345" })
                .done((data: any, status: string) => {
                    validationRules(data, status);
                });
        });


        it("Register action should return the page", function () {
            $.get(controllerPoint + "register")
                .done((data: any, status: string) => {
                    validationRules(data, status);
                });
        });


        it("ProcessRegistration action should return the page", function () {

            $.post(controllerPoint + "processregistration")
                .done((data: any, status: string) => {
                    validationRules(data, status);
                });
        });
    })
} 