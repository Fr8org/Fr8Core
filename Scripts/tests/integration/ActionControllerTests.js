/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var controller;
        (function (controller) {
            describe("Action Controller ", function () {
                var testData = {};
                var returnedData = null;
                var errorHandler = function (response, done) {
                    if (response.status === 401) {
                        console.log("User is not logged in, to run these tests, please login");
                    }
                    else {
                        console.log("Something went wrong");
                        console.log(response);
                    }
                    done.fail(response.responseText);
                };
                var deleteInvoker = function (data, done) {
                    $.ajax({
                        type: "Delete",
                        url: "/api/action/" + data.Id,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json"
                    }).done(function (data, status) {
                        console.log("Deleted Successfully id " + data);
                        done();
                    }).fail(function (response) {
                        errorHandler(response, done);
                    });
                };
                var getInvoker = function (data, done) {
                    $.ajax({
                        type: "GET",
                        url: "/api/action/" + data[0].Id,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json"
                    }).done(function (data, status) {
                        returnedData = data;
                        console.log("Got it Sucessfully");
                        console.log(returnedData);
                        //Delete after get
                        deleteInvoker(data, done);
                    }).fail(function (response) {
                        errorHandler(response, done);
                    });
                };
                var postInvoker = function (done, dataToSave) {
                    $.ajax({
                        type: "POST",
                        url: "/api/action/save",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify(dataToSave),
                        dataType: "json"
                    }).done(function (data, status) {
                        console.log("Saved it Sucessfully");
                        console.log(data);
                        // Then GET, 
                        getInvoker(data, done);
                    }).fail(function (response) {
                        errorHandler(response, done);
                    });
                };
                beforeEach(function (done) {
                    // First POST, create a dummy entry
                    var actions = {
                        actionType: "test action type",
                        configurationSettings: new dockyard.model.ConfigurationSettings(),
                        processNodeTemplateId: 1,
                        isTempId: false,
                        actionId: null,
                        fieldMappingSettings: "test",
                        userLabel: "test",
                        tempId: 0,
                        actionListId: null
                    };
                    postInvoker(done, actions);
                });
                it("can save action", function () {
                    expect(returnedData).not.toBe(null);
                });
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ActionControllerTests.js.map