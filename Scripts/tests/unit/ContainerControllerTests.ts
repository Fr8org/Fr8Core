/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    //import fx = utils.fixtures.ContainerBuilder; // just an alias

    //var errorHandler = function (response, detail) {
    //    if (detail.status === 401) {
    //        fail("User is not logged in, to run these tests, please login");
    //    } else {
    //        fail("Something went wrong" + detail.status);
    //    }
    //};

    //describe("Container Controller ", function () {
    //    var endpoint = "/api/containers/get",
    //        newContainier: interfaces.IContainerVM;

    //    beforeAll(function () {
    //        $(document).ajaxError(errorHandler);
    //        $.ajaxSetup({ async: false, url: endpoint, dataType: "json", contentType: "text/json" });
    //        newContainier = fx.newContainier;   
    //    });
        
    //    it("Should get a Container successfully", function () {
    //        $.getJSON(endpoint, { id: 1})
    //            .done((data: interfaces.IContainerVM, status: string) => {
    //                expect(data).not.toBe(null);
    //                // expect(status).toBe("success");
    //                // expect(data.name).toBe(fx.newContainier.name);
    //            });
    //    });

    //    it("Should get Container list successfully", function () {
    //        $.getJSON(endpoint)
    //            .done((data: interfaces.IContainerVM, status: string) => {
    //                expect(data).not.toBe(null);
    //                // expect(status).toBe("success");
    //                // expect((<any>data).length).toBe(2);
    //            });
    //    });
    //});
}