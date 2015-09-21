
module dockyard.tests.utils.fixtures {

    export class ActionDesignDTO {

        public static newProcessTemplate = <interfaces.IProcessTemplateVM> {
            name: 'Test',
            description: 'Description',
            processTemplateState: 1
        };

        public static configurationControls = {
            "fields":
            [
                {
                    "type": "textField",
                    "name": "connection_string",
                    "required": true,
                    "value": "",
                    "fieldLabel": "SQL Connection String",
                    "events": []
                },
                {
                    "type": "textField",
                    "name": "query",
                    "required": true,
                    "value": "",
                    "fieldLabel": "Custom SQL Query",
                    "events": []
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions",
                    "selected": false,
                    "fieldLabel": "Log All Transactions?",
                    "events": []
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions1",
                    "selected": false,
                    "fieldLabel": "Log Some Transactions?",
                    "events": []
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions2",
                    "selected": false,
                    "fieldLabel": "Log No Transactions?",
                    "events": []
                },
                {
                    "type": "checkboxField",
                    "name": "log_transactions3",
                    "selected": false,
                    "fieldLabel": "Log Failed Transactions?",
                    "events": []
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

        public static actionDesignDTO = <interfaces.IActionDesignDTO> {
            name: "test action type",
            crateStorage: null,
            configurationControls: null,
            processNodeTemplateId: 1,
            activityTemplateId: 1,
            isTempId: false,
            id: 1,
            fieldMappingSettings: ActionDesignDTO.fieldMappingSettings,
            actionListId: null,
            activityTemplate: ActivityTemplate.activityTemplateDO
        };

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
        };
    }
} 