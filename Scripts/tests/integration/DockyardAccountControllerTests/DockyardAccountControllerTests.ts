/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    describe("Dockyard Account Controller ", function () {
        var returnedData = null;

        var errorHandler = function (response, done) {
            done.fail(response.responseText);
        }

        var logInInvoker = function (done, data) {
            $.ajax({
                type: "POST",
                url: "/dockyardaccount/login",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(data),
                dataType: "json"
            }).done(function (data, status) {
                console.log("Log In sucessfully");
                console.log(data);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        };

        var logOffInvoker = function (done) {
            $.ajax({
                type: "GET",
                url: "/dockyardaccount/logoff",
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            }).done(function (data, status) {
                returnedData = data;
                console.log("Log Off Sucessfully");
                console.log(returnedData);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        }; 

        var confirmInvoker = function (data, done) {
            $.ajax({
                type: "GET",
                url: "/dockyardaccount/logoff",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(data)
            }).done(function (data, status) {
                returnedData = data;
                console.log("Confirm Sucessfully");
                console.log(returnedData);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        }; 

        var registrationSuccessfulInvoker = function (done) {
            $.ajax({
                type: "GET",
                url: "/dockyardaccount/registrationsuccessful",
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            }).done(function (data, status) {
                returnedData = data;
                console.log("RegistrationSuccessful Sucessfully");
                console.log(returnedData);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        }; 

        var processRegistrationInvoker = function (data, done) {
            $.ajax({
                type: "POST",
                url: "/dockyardaccount/ProcessRegistration",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(data),
                dataType: "json"
            }).done(function (data, status) {
                console.log("ProcessRegistration sucessfully");
                console.log(data);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        };

        var registerInvoker = function (done) {
            $.ajax({
                type: "GET",
                url: "/dockyardaccount/register",
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            }).done(function (data, status) {
                returnedData = data;
                console.log("Register Sucessfully");
                console.log(returnedData);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        }; 
    });
} 