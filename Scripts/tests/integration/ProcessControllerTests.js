/// <reference path="../../lib/jpattern.js" />

describe("ProcessController tests", function () {
    var returnedData;
    var apiUrl = "http://localhost:30643/api/processes";
    var processIdList;

    var errorHandler = function (response, done) {
        if (response.status === 401) {
            console.log("User is not logged in, to run these tests, please login");
        } else {
            console.log("Something went wrong");
            console.log(response);
        }
        done.fail(response.responseText);
    };

    var getDataFromApi = function (done, url, onContinue) {
        $.ajax({
            type: "GET",
            url: apiUrl + url,
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).done(function (data) {
            returnedData = data;
            console.log("Got it Sucessfully");
            console.log(returnedData);
            onContinue(done);
        }).fail(function (response) {
            errorHandler(response, done);
        });
    }

    beforeAll(function (done) {
        getDataFromApi(done, "/getIdsByName?name=TestTemplate{0B6944E1-3CC5-45BA-AF78-728FFBE57358}",
            function () {
                processIdList = returnedData;
                done();
            });
    });

    it("can get list of process ids by name", function () {
        expect(processIdList instanceof Array).toBe(true);
        expect(processIdList.length).toBe(1);
    });
    // find(obj).where(function(x) {  }
    it("can get process by id", function (done) {
        getDataFromApi(done, "/" + processIdList[0], function () {

            var responsePattern = {
                crateStorage: {
                    crates: [
                        {
                            Label: "Standard Event Report",
                            Contents: {
                                EventNames: "DocuSign Envelope Sent",
                                EventPayload: [
                                    {
                                        Label: "Payload Data",
                                        Contents: [
                                            {
                                                Key: "EnvelopeId"
                                            }
                                        ]
                                    }
                                ]
                            }
                        }
                    ]
                }
            }

            var result = matchObjectWithPattern(responsePattern, returnedData);

            if (result !== true) {
                console.log(result);
            }

            expect(result).toBe(true);

            done();
        });
    });
});





