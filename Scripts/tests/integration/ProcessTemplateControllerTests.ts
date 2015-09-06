/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
/// <reference path="../../typings/jquery/jquery.d.ts" />
/// <reference path="../utils/fixtures.ts" />

module dockyard.tests.controller {
    import fx = utils.Fixtures; // just an alias

    var errorHandler = function (response, detail) {
        if (detail.status === 401) {
            fail("User is not logged in, to run these tests, please login");
        } else {
            fail("Something went wrong" + detail.status);
        }
    };

    describe("Process Template Controller ", function () {
        var endpoint = "/api/ProcessTemplate",
            currentProcessTemplate: interfaces.IProcessTemplateVM,
            changeProcessTemplate1 = "";

        beforeAll(function () {
            $(document).ajaxError(errorHandler);
            $.ajaxSetup({ async: false, url: endpoint, dataType: "json", contentType: "text/json" });

            //Create a ProcessTemplate
            $.post(endpoint, JSON.stringify(fx.newProcessTemplate),
                (curProcessTemplate, status) => currentProcessTemplate = curProcessTemplate
            ); 
        });

        it("should get a Process Template successfully", function () {
            $.getJSON(endpoint, { id: currentProcessTemplate.id })
                .done((data: interfaces.IProcessTemplateVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect(data.name).toBe(fx.newProcessTemplate.name);
                    expect(data.description).toBe(fx.newProcessTemplate.description);
                    expect(data.processTemplateState).toBe(fx.newProcessTemplate.processTemplateState);
                });
        });

        it("should specify DocuSign template successfully", function () {
            $.post(endpoint + "?updateRegistrations=true", JSON.stringify(fx.updatedProcessTemplate))
                .done((data: interfaces.IProcessTemplateVM, status: string) => {
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
            $.get(endpoint + "/triggersettings").done(
                (data: interfaces.IExternalEventVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect((<any>data).length).toBe(4);
                });
        });

    });
}