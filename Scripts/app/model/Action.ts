module dockyard.model {
    export class Action implements interfaces.IAction {
        processNodeTemplateId: number;
        actionId: number;
        isTempId: boolean;
        actionListId: number;
        actionType: string;
        configurationSettings: model.ConfigurationSettings;
        fieldMappingSettings: string;
        userLabel: string;
        actionTemplateId: number;

        constructor(
            processNodeTemplateId: number,
            id: number,
            isTempId: boolean,
            actionListId: number
        ) {
            this.processNodeTemplateId = processNodeTemplateId;
            this.actionId = id;
            this.isTempId = isTempId;
            this.actionListId = actionListId;
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM> {
                actionId: this.actionId,
                isTempId: this.isTempId,
                processNodeTemplateId: this.processNodeTemplateId,
                userLabel: this.userLabel,
                actionListId: this.actionListId,
                actionType: this.actionType,
                configurationSettings: this.configurationSettings,
                fieldMappingSettings: this.fieldMappingSettings
            };
        }

        clone(): Action {
            var result = new Action(this.processNodeTemplateId, this.actionId, this.isTempId, this.actionListId);
            result.userLabel = this.userLabel;
            result.actionType = this.actionType;

            return result;
        }
    }
}