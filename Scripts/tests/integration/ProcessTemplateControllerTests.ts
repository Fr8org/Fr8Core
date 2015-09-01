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
            $.getJSON(endpoint, { id: currentProcessTemplate.Id })
                .done((data: interfaces.IProcessTemplateVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect(data.Name).toBe(fx.newProcessTemplate.Name);
                    expect(data.Description).toBe(fx.newProcessTemplate.Description);
                    expect(data.ProcessTemplateState).toBe(fx.newProcessTemplate.ProcessTemplateState);
                });
        });

        it("should specify DocuSign template successfully", function () {
            $.post(endpoint + "?updateRegistrations=true", JSON.stringify(fx.updatedProcessTemplate))
                .done((data: interfaces.IProcessTemplateVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect(data.Name).toBe(fx.updatedProcessTemplate.Name);
                    expect(data.Description).toBe(fx.updatedProcessTemplate.Description);
                    expect(data.ProcessTemplateState).toBe(fx.updatedProcessTemplate.ProcessTemplateState);
                    expect($.isArray(data.SubscribedDocuSignTemplates)).toBeTruthy();
                    expect(data.SubscribedDocuSignTemplates.length).toBe(1);
                    expect(data.SubscribedDocuSignTemplates[0]).toBe(fx.updatedProcessTemplate.SubscribedDocuSignTemplates[0]);
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