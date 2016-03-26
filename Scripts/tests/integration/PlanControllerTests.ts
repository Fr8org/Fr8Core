/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
/// <reference path="../../typings/jquery/jquery.d.ts" />
/// <reference path="../utils/fixture_planbuilder.ts" />

// **************************************************************************************************
// NOTE: The tests below are only for example. They are not run in the current version of the system. 
// **************************************************************************************************
module dockyard.tests.controller {
    import fx = utils.fixtures.PlanBuilder; // just an alias

    var errorHandler = function (response, detail) {
        if (detail.status === 401) {
            fail("User is not logged in, to run these tests, please login");
        } else {
            fail("Something went wrong" + detail.status);
        }
    };

    describe("Plan Controller ", function () {
        var endpoint = "/api/plan",
            currentPlan: interfaces.IPlanVM,
            changePlan1 = "";

        beforeAll(function () {
            $(document).ajaxError(errorHandler);
            $.ajaxSetup({ async: false, url: endpoint, dataType: "json", contentType: "text/json" });

            //Create a Plan
            $.post(endpoint, JSON.stringify(fx.newPlan),
                (curPlan, status) => currentPlan = curPlan
            );
        });

        it("should get a Plan successfully", function () {
            $.getJSON(endpoint, { id: currentPlan.id })
                .done((data: interfaces.IPlanVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect(data.name).toBe(fx.newPlan.name);
                    expect(data.description).toBe(fx.newPlan.description);
                    expect(data.planState).toBe(fx.newPlan.planState);
                });
        });

        it("should specify DocuSign template successfully", function () {
            $.post(endpoint + "?updateRegistrations=true", JSON.stringify(fx.updatedPlan))
                .done((data: interfaces.IPlanVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect(data.name).toBe(fx.updatedPlan.name);
                    expect(data.description).toBe(fx.updatedPlan.description);
                    expect(data.planState).toBe(fx.updatedPlan.planState);
                    expect($.isArray(data.subscribedDocuSignTemplates)).toBeTruthy();
                    expect(data.subscribedDocuSignTemplates.length).toBe(1);
                    expect(data.subscribedDocuSignTemplates[0]).toBe(fx.updatedPlan.subscribedDocuSignTemplates[0]);
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