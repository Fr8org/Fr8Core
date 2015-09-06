module dockyard.model {
    export class ActionDesignDTO implements interfaces.IActionDesignDTO {
        processNodeTemplateId: number;
        id: number;
        isTempId: boolean;
        actionListId: number;
        name: string;
        configurationSettings: model.ConfigurationSettings;
        fieldMappingSettings: model.FieldMappingSettings;
        actionTemplateId: number;
        actionTemplate: model.ActionTemplate;

        constructor(
            processNodeTemplateId: number,
            id: number,
            isTempId: boolean,
            actionListId: number
        ) {
            this.processNodeTemplateId = processNodeTemplateId;
            this.id = id;
            this.isTempId = isTempId;
            this.actionListId = actionListId;
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM> {
                id: this.id,
                isTempId: this.isTempId,
                processNodeTemplateId: this.processNodeTemplateId,
                actionListId: this.actionListId,
                name: this.name,
                configurationSettings: this.configurationSettings,
                fieldMappingSettings: this.fieldMappingSettings,
                actionTemplate: this.actionTemplate
            };
        }

        clone(): ActionDesignDTO {
            var result = new ActionDesignDTO(this.processNodeTemplateId, this.id, this.isTempId, this.actionListId);
            result.name = this.name;

            return result;
        }

        static isActionValid(action: interfaces.IActionVM) {
            return action && action.$resolved && !action.isTempId
        }

        static create(dataObject: interfaces.IActionDesignDTO): ActionDesignDTO {
            var result = new ActionDesignDTO(0, 0, false, 0);
            result.actionListId = dataObject.actionListId;
            result.actionTemplateId = dataObject.actionTemplateId;
            result.configurationSettings = dataObject.configurationSettings;
            result.fieldMappingSettings = dataObject.fieldMappingSettings;
            result.id = dataObject.id;
            result.isTempId = dataObject.isTempId;
            result.name = dataObject.name;
            result.processNodeTemplateId = dataObject.processNodeTemplateId;
            result.actionTemplate = dataObject.actionTemplate;
            return result;
        }
    }
}