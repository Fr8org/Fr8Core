module dockyard.model {
    export class ActionDesignDTO implements interfaces.IActionDesignDTO {
        processNodeTemplateId: number;
        actionId: number;
        isTempId: boolean;
        actionListId: number;
        name: string;
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
                name: this.name,
                configurationSettings: this.configurationSettings,
                fieldMappingSettings: this.fieldMappingSettings
            };
        }

        clone(): ActionDesignDTO {
            var result = new ActionDesignDTO(this.processNodeTemplateId, this.actionId, this.isTempId, this.actionListId);
            result.userLabel = this.userLabel;
            result.name = this.name;

            return result;
        }
    }
}