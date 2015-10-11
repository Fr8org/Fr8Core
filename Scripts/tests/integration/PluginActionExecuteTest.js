/// <reference path="../../lib/jpattern.js" />

describe("plugins execute actions tests", function () {
    var returnedData;
    var apiUrl = "http://localhost:30643/api/processes";
    var testProcessId1;
    var testProcessId2;

    var errorHandler = function(response, done) {
        if (response.status === 401) {
            console.log("User is not logged in, to run these tests, please login");
        } else {
            console.log("Something went wrong");
            console.log(response);
        }
        done.fail(response.responseText);
    };
    
    var getDataFromApi = function(done, url, onContinue) {
        $.ajax({
            type: "GET",
            url: apiUrl + url,
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).done(function(data) {
            returnedData = data;
            console.log("Got it Sucessfully");
            console.log(returnedData);
            onContinue(done);
        }).fail(function(response) {
            errorHandler(response, done);
        });
    }

    var executePluginAction = function(done, pluginUrl, actionData, onContinue) {
        $.ajax({
            type: "POST",
            url: pluginUrl + "/actions/execute",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(actionData),
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
    
    /**********************************************************************************/
    // Init
    /**********************************************************************************/

    beforeAll(function(done) {
        getDataFromApi(done, "/getIdsByName?name=TestTemplate{0B6944E1-3CC5-45BA-AF78-728FFBE57358}",
            function () {
                testProcessId1 = returnedData[0];

                getDataFromApi(done, "/getIdsByName?name=TestTemplate{77D78B4E-111F-4F62-8AC6-6B77459042CB}",
                function () {
                    testProcessId2 = returnedData[0];
                    done();
                });
            });
    });
    
    /**********************************************************************************/
    // Specs
    /**********************************************************************************/

    it("Docusign plugin can execute action Monitor_DocuSign", function (done) {

        var actionDTO = {
            activityTemplate: {
                Name: "Monitor_DocuSign",
                Version: "1"
            },
            ProcessId: testProcessId1
        };

        executePluginAction(done, "http://localhost:53234", actionDTO, function () {

            console.log(JSON.stringify(returnedData));
            var responsePattern = {
                CrateStorage: {
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
                        },
                        {
                            Label: "DocuSign Envelope Payload Data",
                            Contents: [
                                {
                                    Key: "EnvelopeId"
                                }
                            ]
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

    /**********************************************************************************/

    it("Docusign plugin can execute action Extract_From_DocuSign_Envelope", function (done) {

        var actionDTO = {
            activityTemplate: {
                Name: "Extract_From_DocuSign_Envelope",
                Version: "1",
            },
            CrateStorage : {
                    crates: [
                        {
                            Label: "DocuSignTemplateUserDefinedFields",
                            ManifestType: "Standard Design-Time Fields",
                            Contents: JSON.stringify({
                                Fields: [
                                { Key: "ExternalEventType" },
                                { Key: "RecipientId" }]
                            })
                        }
                    ]
                },
            ProcessId: testProcessId2
        };

        executePluginAction(done, "http://localhost:53234", actionDTO, function () {

            console.log(JSON.stringify(returnedData));
            var responsePattern = {
                CrateStorage: {
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
                        },
                        {
                            Label: "DocuSign Envelope Payload Data",
                            Contents: [
                                {
                                    Key: "EnvelopeId"
                                }
                            ]
                        },
                        {
                            Label: "DocuSign Envelope Data",
                            ManifestType: "Standard Payload Data",
                            ManifestId:5,
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

    /**********************************************************************************/
});





