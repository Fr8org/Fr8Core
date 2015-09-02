var dockyard;
(function (dockyard) {
    var tests;
    (function (tests) {
        var utils;
        (function (utils) {
            var Fixtures = (function () {
                function Fixtures() {
                }
                Fixtures.newProcessTemplate = {
                    Name: 'Test',
                    Description: 'Description',
                    ProcessTemplateState: 1
                };
                Fixtures.updatedProcessTemplate = {
                    'Name': 'Updated',
                    'Description': 'Description',
                    'ProcessTemplateState': 1,
                    'SubscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
                };
                Fixtures.configurationSettings = {
                    "fields": [
                        {
                            "type": "textField",
                            "name": "connection_string",
                            "required": true,
                            "value": "",
                            "fieldLabel": "SQL Connection String"
                        },
                        {
                            "type": "textField",
                            "name": "query",
                            "required": true,
                            "value": "",
                            "fieldLabel": "Custom SQL Query"
                        },
                        {
                            "type": "checkboxField",
                            "name": "log_transactions",
                            "selected": false,
                            "fieldLabel": "Log All Transactions?"
                        },
                        {
                            "type": "checkboxField",
                            "name": "log_transactions1",
                            "selected": false,
                            "fieldLabel": "Log Some Transactions?"
                        },
                        {
                            "type": "checkboxField",
                            "name": "log_transactions2",
                            "selected": false,
                            "fieldLabel": "Log No Transactions?"
                        },
                        {
                            "type": "checkboxField",
                            "name": "log_transactions3",
                            "selected": false,
                            "fieldLabel": "Log Failed Transactions?"
                        }
                    ]
                };
                return Fixtures;
            })();
            utils.Fixtures = Fixtures;
        })(utils = tests.utils || (tests.utils = {}));
    })(tests = dockyard.tests || (dockyard.tests = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=fixtures.js.map