/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
/// <reference path="../../typings/jquery/jquery.d.ts" />
/// <reference path="../utils/fixtures.ts" />
var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var controller;
        (function (controller) {
            var fx = tests.utils.Fixtures; // just an alias
            var errorHandler = function (response, detail) {
                if (detail.status === 401) {
                    fail("User is not logged in, to run these tests, please login");
                }
                else {
                    fail("Something went wrong" + detail.status);
                }
            };
            describe("Process Template Controller ", function () {
                var endpoint = "/api/ProcessTemplate", currentProcessTemplate, changeProcessTemplate1 = "";
                beforeAll(function () {
                    $(document).ajaxError(errorHandler);
                    $.ajaxSetup({ async: false, url: endpoint, dataType: "json", contentType: "text/json" });
                    //Create a ProcessTemplate
                    $.post(endpoint, JSON.stringify(fx.newProcessTemplate), function (curProcessTemplate, status) { return currentProcessTemplate = curProcessTemplate; });
                });
                it("should get a Process Template successfully", function () {
                    $.getJSON(endpoint, { id: currentProcessTemplate.id })
                        .done(function (data, status) {
                        expect(data).not.toBe(null);
                        expect(status).toBe("success");
                        expect(data.name).toBe(fx.newProcessTemplate.name);
                        expect(data.description).toBe(fx.newProcessTemplate.description);
                        expect(data.processTemplateState).toBe(fx.newProcessTemplate.processTemplateState);
                    });
                });
                it("should specify DocuSign template successfully", function () {
                    $.post(endpoint + "?updateRegistrations=true", JSON.stringify(fx.updatedProcessTemplate))
                        .done(function (data, status) {
                        expect(data).not.toBe(null);
                        expect(status).toBe("success");
                        expect(data.name).toBe(fx.updatedProcessTemplate.name);
                        expect(data.description).toBe(fx.updatedProcessTemplate.description);
                        expect(data.processTemplateState).toBe(fx.updatedProcessTemplate.processTemplateState);
                        expect($.isArray(data.subscribedDocuSignTemplates)).toBeTruthy();
                        expect(data.subscribedDocuSignTemplates.length).toBe(1);
                        expect(data.subscribedDocuSignTemplates[0]).toBe(fx.updatedProcessTemplate.subscribedDocuSignTemplates[0]);
                    });
                });
                it("should return the list of external events", function () {
                    $.get(endpoint + "/triggersettings").done(function (data, status) {
                        expect(data).not.toBe(null);
                        expect(status).toBe("success");
                        expect(data.length).toBe(4);
                    });
                });
            });
        })(controller = tests.controller || (tests.controller = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessTemplateControllerTests.js.map