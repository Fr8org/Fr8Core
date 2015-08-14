/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {

    describe("Process Template Controller ", function () {
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

        var deleteInvoker = function (data, done) {
            $.ajax({
                type: "Delete",
                url: "/api/processtemplate/" + data.Id,
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
                url: "/api/processtemplate/" + data.Id,
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
                url: "/api/processtemplate",
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

            postInvoker(done, { name: "testdata" });

        });

        it("Can Get One Process Template", function () {
            expect(returnedData).not.toBe(null);
        });
    });

}