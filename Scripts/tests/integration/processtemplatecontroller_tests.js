
describe("Process Template Controller ", function () {
    var returnedData = null;

    beforeEach(function (done) {
        $.ajax({
            type: "GET",
            url: "/api/processtemplate",
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).success(function (data, status) {
            returnedData = data;
            done();
        }).error(function (response) {
            if (response.status === 401) {
                console.log("User is not logged in, to run these tests, please login");
            } else {
                console.log("Something went wrong");
            }
            done();
        });
    });

    it("Can Get Data", function () {
        expect(returnedData).not.toBe(null);
    });


});