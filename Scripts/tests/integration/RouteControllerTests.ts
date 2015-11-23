/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />
/// <reference path="../../typings/jquery/jquery.d.ts" />
/// <reference path="../utils/fixture_routebuilder.ts" />


module dockyard.tests.controller {
    import fx = utils.fixtures.RouteBuilder; // just an alias

    var errorHandler = function (response, detail) {
        if (detail.status === 401) {
            fail("User is not logged in, to run these tests, please login");
        } else {
            fail("Something went wrong" + detail.status);
        }
    };

    describe("Route Controller ", function () {
        var endpoint = "/api/route",
            currentRoute: interfaces.IRouteVM,
            changeRoute1 = "";

        beforeAll(function () {
            $(document).ajaxError(errorHandler);
            $.ajaxSetup({ async: false, url: endpoint, dataType: "json", contentType: "text/json" });

            //Create a Route
            $.post(endpoint, JSON.stringify(fx.newRoute),
                (curRoute, status) => currentRoute = curRoute
            ); 
        });

        it("should get a Route successfully", function () {
            $.getJSON(endpoint, { id: currentRoute.id })
                .done((data: interfaces.IRouteVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect(data.name).toBe(fx.newRoute.name);
                    expect(data.description).toBe(fx.newRoute.description);
                    expect(data.routeState).toBe(fx.newRoute.routeState);
                });
        });

        it("should specify DocuSign template successfully", function () {
            $.post(endpoint + "?updateRegistrations=true", JSON.stringify(fx.updatedRoute))
                .done((data: interfaces.IRouteVM, status: string) => {
                    expect(data).not.toBe(null);
                    expect(status).toBe("success");
                    expect(data.name).toBe(fx.updatedRoute.name);
                    expect(data.description).toBe(fx.updatedRoute.description);
                    expect(data.routeState).toBe(fx.updatedRoute.routeState);
                    expect($.isArray(data.subscribedDocuSignTemplates)).toBeTruthy();
                    expect(data.subscribedDocuSignTemplates.length).toBe(1);
                    expect(data.subscribedDocuSignTemplates[0]).toBe(fx.updatedRoute.subscribedDocuSignTemplates[0]);
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