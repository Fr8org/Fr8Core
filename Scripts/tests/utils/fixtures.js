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
                    name: 'Test',
                    description: 'Description',
                    processTemplateState: 1
                };
                Fixtures.updatedProcessTemplate = {
                    'name': 'Updated',
                    'description': 'Description',
                    'processTemplateState': 1,
                    'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
                };
                Fixtures.fieldMappingSettings = {
                    "fields": [
                        {
                            "name": "[_AccessLevelTemplate].Value]",
                            "value": "Text"
                        },
                        {
                            "name": "[_AccessLevelTemplate].Version]",
                            "value": "Checkbox"
                        }
                    ]
                };
                Fixtures.configurationStore = {
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