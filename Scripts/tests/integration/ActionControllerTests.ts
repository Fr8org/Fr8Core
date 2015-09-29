/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    import fx = utils.fixtures; // just an alias

    describe("Action Controller ", function () {
        var testData = {};

        var errorHandler = function (response, done) {
            if (response.status === 401) {
                console.log("User is not logged in, to run these tests, please login");
            } else {
                console.log("Something went wrong");
                console.log(response);
            }
            done.fail(response.responseText);
        };

        describe("Action#Get,Delete,Save", function () {
            var returnedData = null;

        var deleteInvoker = function (data, done) {
            $.ajax({
                type: "Delete",
                url: "/actions/" + data.id,
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
                url: "/actions/" + data[0].id,
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
                url: "/actions/save",
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

            var actions: interfaces.IActionDTO =
            {
                    name: "test action type",
                    configurationControls: fx.ActionDesignDTO.configurationControls,
                    crateStorage: null,
                    processNodeTemplateId: 1,
                    activityTemplateId: 1,
                    isTempId: false,
                    id: 0,
                    actionListId: 1,
                    activityTemplate: null
                };

            postInvoker(done, actions);

        });

        it("can save action", function () {
            expect(returnedData).not.toBe(null);
        });
    });

        describe("Action#GetConfigurationSettings", function () {
            var endpoint = "/actions";

            var currentActionDesignDTO: interfaces.IActionDTO =
                    {
                        name: "test action type",
                        configurationControls: fx.ActionDesignDTO.configurationControls,
                        crateStorage: null,
                        processNodeTemplateId: 1,
                        activityTemplateId: 1,
                        isTempId: false,
                        id: 1,
                        // ActionListId is set to null, since there is no ActionsLists on a blank db.
                        actionListId: null,
                        activityTemplate: fx.ActivityTemplate.activityTemplateDO
                    };

            beforeAll(function () {
                $(document).ajaxError(errorHandler);
                $.ajaxSetup({ async: false, url: endpoint, dataType: "json", contentType: "text/json" });
            });

            it("Should get the correct configuration settings (AzureSqlServerPluginRegistration_v1)", function () {
                var expectedSettings = JSON.stringify({ "fields": [{ "name": "connection_string", "required": true, "value": "", "fieldLabel": "SQL Connection String", "type": "textField", "selected": false }], "data-fields": [] });
                $.ajax({
                    type: "POST",
                    url: "/actions/actions/configuration",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(currentActionDesignDTO),
                    dataType: "json"
                }).done(function (data, status) {
                    expect(data).not.toBe(null);

                    expect(angular.equals(data, expectedSettings)).toBe(true);
                });
            });
        });
    });

}