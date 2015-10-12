module dockyard.model {
    export class ActionDTO implements interfaces.IActionDTO {
        processNodeTemplateId: number;
        id: number;
        isTempId: boolean;
        actionListId: number;
        name: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplateId: number;
        activityTemplateName: string;
        currentView: string;

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
            this.configurationControls = new ControlsList();
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM> {
                id: this.id,
                isTempId: this.isTempId,
                processNodeTemplateId: this.processNodeTemplateId,
                actionListId: this.actionListId,
                name: this.name,
                crateStorage: this.crateStorage,
                configurationControls: this.configurationControls
            };
        }

        clone(): ActionDTO {
            var result = new ActionDTO(this.processNodeTemplateId, this.id, this.isTempId, this.actionListId);
            result.name = this.name;

            return result;
        }

        static isActionValid(action: interfaces.IActionVM) {
            return action && action.$resolved && !action.isTempId
        }

        static create(dataObject: interfaces.IActionDTO): ActionDTO {
            var result = new ActionDTO(0, 0, false, 0);
            result.actionListId = dataObject.actionListId;
            result.activityTemplateId = dataObject.activityTemplateId;
            result.crateStorage = dataObject.crateStorage;
            result.configurationControls = dataObject.configurationControls;
            result.id = dataObject.id;
            result.isTempId = dataObject.isTempId;
            result.name = dataObject.name;
            result.processNodeTemplateId = dataObject.processNodeTemplateId;
            return result;
        }
    }
}