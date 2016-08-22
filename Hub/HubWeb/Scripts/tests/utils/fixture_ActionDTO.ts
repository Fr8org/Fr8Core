
module dockyard.tests.utils.fixtures {

    import models = dockyard.model;

    export class ActivityDTO {

        public static newPlan = <interfaces.IPlanVM> {
            name: 'Test',
            description: 'Description',
            planState: model.PlanState.Inactive
        };

        public static configurationControls = <models.ControlsList>{
            fields:
            [
                <models.ControlDefinitionDTO>{
                    "type": "textField",
                    "name": "connection_string",
                    "required": true,
                    "value": "",
                    "errorMessage": null,
                    "fieldLabel": "SQL Connection String",
                    "events": [],
                    "isFocused": false,
                    "label": null,
                    isHidden: false,
                },
                {
                    "type": "textField",
                    "name": "query",
                    "required": true,
                    "value": "",
                    "errorMessage": null,
                    "fieldLabel": "Custom SQL Query",
                    "events": [],
                    "isFocused": false
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log All Transactions?",
                    "events": [],
                    "errorMessage": null,
                    "isFocused": false
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions1",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log Some Transactions?",
                    "events": [],
                    "errorMessage": null,
                    "isFocused": false
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions2",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log No Transactions?",
                    "events": [],
                    "errorMessage": null,
                    "isFocused": false
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions3",
                    "selected": false,
                    "value": "",
                    "fieldLabel": "Log Failed Transactions?",
                    "events": [],
                    "errorMessage": null,
                    "isFocused": false
                }
            ]
        };

        public static fieldMappingSettings = {
            fields: [
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
                name: 'test'
            },
            id: 'E55315F9-A30B-4196-A43D-6F511B91CCF8'
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
                name: 'test'
            },
            id: 'E55315F9-A30B-4196-A43D-6F511B91CCF8'
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
                name: 'test'
            },
            id: 'E55315F9-A30B-4196-A43D-6F511B91CCF8'
        };

        public static typedFieldsVM = <interfaces.IActionVM> {
            crateStorage: {
                crates: [{
                    id: "36047336-fb27-4382-8c02-2ea41bc2ac92",
                    label: "Queryable Criteria",
                    contents: angular.fromJson('{ "Fields": [ { "Name": "CreatedById", "Label": "Created By ID", "FieldType": "String", "Control": { "name": "CreatedById", "required": false, "value": null, "label": null, "type": "TextBox", "selected": false, "events": [], "source": null, "showDocumentation": null, "errorMessage": null, "isHidden": false, "isCollapsed": false } }]}'),
                    parentCrateId: null,
                    manifestType: "Typed Fields",
                    manufacturer: null,
            }]},
            configurationControls: {
                fields: [{
                    fieldLabel: "Test Label",
                    name: "test_name",
                    value: null
                }]
            },
            activityTemplate: {
                name: 'test'
            },
            id: 'E55315F9-A30B-4196-A43D-6F511B91CCF8'
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