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
                url: "http://localhost:53234/plugin_docusign/actions/execute",
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

            var actions: interfaces.IActionDTO =
                { 
                    name: "test action type",
                    configurationControls: utils.fixtures.ActionDesignDTO.configurationControls,
                    crateStorage: null,
                    parentActivityId: 1,
                    activityTemplateId: 1,
                    isTempId: false,
                    id: 0,
                    // ActionListId is set to null, since there is no ActionsLists on a blank db.
                    actionListId: null,
                    activityTemplate: utils.fixtures.ActivityTemplate.activityTemplateDO
                };

            postInvoker(done, actions);

        });

        it("can configure action", function () {
            expect(returnedData).not.toBe(null);
        });
    });

}