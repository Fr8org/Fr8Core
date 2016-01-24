/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {

    describe("DocuSignPlugin Controller ", function () {
        var testData = {};

        var returnedData = null;

        var errorHandler = function (response, done) {
            if (response.status === 401) {
                console.log("User is not logged in, to run these tests, please login");
            } else {
                console.log("Something went wrong");
                console.log(response);
            }
            done.fail(response.responseText);
        };

        var activateInvoker = function (done, dataToSave) {
            $.ajax({
                type: "POST",
                url: "http://localhost:53234/plugin_docusign/actions/activate",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(dataToSave),
                dataType: "json"
            }).done(function (data, status) {
                console.log("Activated it Sucessfully");
                console.log(data);
                executeInvoker(done, dataToSave);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        };

        var executeInvoker = function (done, dataToSave) {
            $.ajax({
                type: "POST",
                url: "http://localhost:53234/plugin_docusign/actions/run",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(dataToSave),
                dataType: "json"
            }).done(function (data, status) {
                returnedData = data;
                console.log("Executed it Sucessfully");
                console.log(data);
                done();
            }).fail(function (response) {
                errorHandler(response, done);
            });
        };

        var postInvoker = function (done, dataToSave) {
            $.ajax({
                type: "POST",
                url: "http://localhost:53234/plugin_docusign/actions/configure",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(dataToSave),
                dataType: "json"
            }).done(function (data, status) {
                console.log("Configured it Sucessfully");
                console.log(data);
                activateInvoker(done, dataToSave);
            }).fail(function (response) {
                errorHandler(response, done);
            });
        };

        beforeEach(function (done) {
            // First POST, create a dummy entry

            var actions: interfaces.IActivityDTO =
                { 
                    name: "test action type",
                    configurationControls: utils.fixtures.ActivityDTO.configurationControls,
                    crateStorage: null,
                    parentRouteNodeId: '89EBF277-0CC4-4D6D-856B-52457F10C686',
                    activityTemplate: null,
                    activityTemplateId: 1,
                    isTempId: false,
                    id: '00000000-0000-0000-0000-000000000000',
                    childrenActions: null,
                    ordering: 0
                };

            postInvoker(done, actions);

        });

        it("can configure action", function () {
            expect(returnedData).not.toBe(null);
        });
    });

}