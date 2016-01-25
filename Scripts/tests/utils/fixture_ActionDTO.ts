
module dockyard.tests.utils.fixtures {

    export class ActivityDTO {

        public static newRoute = <interfaces.IRouteVM> {
            name: 'Test',
            description: 'Description',
            routeState: 1
        };

        public static configurationControls = {
            "fields":
            [
                {
                    "type": "textField",
                    "name": "connection_string",
                    "required": true,
                    "value": "",
                    "errorMessage": null,
                    "fieldLabel": "SQL Connection String",
                    "events": []
                },
                {
                    "type": "textField",
                    "name": "query",
                    "required": true,
                    "value": "",
                    "errorMessage": null,
                    "fieldLabel": "Custom SQL Query",
                    "events": []
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log All Transactions?",
                    "events": [],
                    "errorMessage": null,
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions1",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log Some Transactions?",
                    "events": [],
                    "errorMessage": null
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions2",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log No Transactions?",
                    "events": [],
                    "errorMessage": null
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions3",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log Failed Transactions?",
                    "events": [],
                    "errorMessage": null
                }
            ]
        };

        public static fieldMappingSettings = {
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

        public static noAuthActionVM = <interfaces.IActionVM> {
            crateStorage: {
                crates: [{
                    id: "37ea608f-eead-4d0f-b75f-8033474e6030",
                    label: "Configuration_Controls",
                    contents: angular.fromJson("{\"Controls\":[{\"name\":\"connection_string\",\"required\":true,\"value\":null,\"label\":\"SQL Connection String\",\"type\":\"TextBox\",\"selected\":false,\"events\":[{\"name\":\"onChange\",\"handler\":\"requestConfig\"}],\"source\":null}],\"ManifestType\":6,\"ManifestId\":6,\"ManifestName\":\"Standard UI Controls\"}"),
                    parentCrateId: null,
                    manifestType: "Standard UI Controls",                    
                    manufacturer: null
                }]
            },
            configurationControls: {
                fields: [{
                    fieldLabel: "SQL Connection String",
                    name: "connection_string",
                    value: null
                }]
            },
            activityTemplate: {
                id: 2
            },
            isTempId: false,
            currentView: null,
            id: 'E55315F9-A30B-4196-A43D-6F511B91CCF8',
            name: "Write_To_Sql_Server"
        };

        public static internalAuthActionVM = <interfaces.IActionVM> {
            crateStorage: {
                crates: [{
                    id: "37ea608f-eead-4d0f-b75f-8033474e6030",
                    label: "Configuration_Controls",
                    contents: angular.fromJson("{\"Controls\":[{\"name\":\"connection_string\",\"required\":true,\"value\":null,\"label\":\"SQL Connection String\",\"type\":\"TextBox\",\"selected\":false,\"events\":[{\"name\":\"onChange\",\"handler\":\"requestConfig\"}],\"source\":null}],\"ManifestType\":6,\"ManifestId\":6,\"ManifestName\":\"Standard UI Controls\"}"),
                    parentCrateId: null,
                    manifestType: "Standard UI Controls",
                    manufacturer: null
                }, {
                        id: "37ea608f-eead-4d0f-b75f-8033474e6030",
                        label: "Test_Auth_Crate",
                        contents: "{\"Mode\":\"1\"}",
                        parentCrateId: null,
                        manifestType: "Standard Authentication",
                        manufacturer: null
                    }]
            },
            configurationControls: {
                fields: [{
                    fieldLabel: "SQL Connection String",
                    name: "connection_string",
                    value: null
                }]
            },
            activityTemplate: {
                id: 2
            },
            isTempId: false,
            currentView: null,
            id: 'E55315F9-A30B-4196-A43D-6F511B91CCF8',
            name: "Write_To_Sql_Server"
        };

        public static externalAuthActionVM = <interfaces.IActionVM> {
            crateStorage: {
                crates: [{
                    id: "37ea608f-eead-4d0f-b75f-8033474e6030",
                    label: "Configuration_Controls",
                    contents: angular.fromJson("{\"Controls\":[{\"name\":\"connection_string\",\"required\":true,\"value\":null,\"label\":\"SQL Connection String\",\"type\":\"TextBox\",\"selected\":false,\"events\":[{\"name\":\"onChange\",\"handler\":\"requestConfig\"}],\"source\":null}],\"ManifestType\":6,\"ManifestId\":6,\"ManifestName\":\"Standard UI Controls\"}"),
                    parentCrateId: null,
                    manifestType: "Standard UI Controls",
                    manufacturer: null
                }, {
                        id: "37ea608f-eead-4d0f-b75f-8033474e6030",
                        label: "Test_Auth_Crate",
                        contents: "{\"Mode\":\"2\"}",
                        parentCrateId: null,
                        manifestType: "Standard Authentication",
                        manufacturer: null
                    }]
            },
            configurationControls: {
                fields: [{
                    fieldLabel: "SQL Connection String",
                    name: "connection_string",
                    value: null
                }]
            },
            activityTemplate: {
                id: 2
            },
            isTempId: false,
            currentView: null,
            id: 'E55315F9-A30B-4196-A43D-6F511B91CCF8',
            name: "Write_To_Sql_Server"
        };

        /*
        public static paneConfiguration = <dockyard.directives.paneConfigureAction.IPaneConfigureActionScope> {
            currentAction: ActionDesignDTO.actionDesignDTO
        };

        public static PaneConfigureActionOnRender_EventArgs = <dockyard.directives.paneConfigureAction.RenderEventArgs> {
             action: ActionDesignDTO.actionDesignDTO
        };

        public static PaneConfigureActionOnRender_Event = <ng.IAngularEvent> {
            currentScope: ActionDesignDTO.paneConfiguration,
            targetScope: ActionDesignDTO.paneConfiguration,
            defaultPrevented: null,
            name: "",
            preventDefault: null,
            stopPropagation: null
        };*/
    }
} 