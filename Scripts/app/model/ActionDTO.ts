module dockyard.model {
    export class ActionDTO implements interfaces.IActionDTO {
        parentActivityId: number;
        id: number;
        isTempId: boolean;
        name: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplateId: number;
        activityTemplateName: string;
        currentView: string;

        constructor(
            parentActivityId: number,
            id: number,
            isTempId: boolean
        ) {
            this.parentActivityId = parentActivityId;
            this.id = id;
            this.isTempId = isTempId;
            this.configurationControls = new ControlsList();
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM> {
                id: this.id,
                isTempId: this.isTempId,
                parentActivityId: this.parentActivityId,
                name: this.name,
                crateStorage: this.crateStorage,
                configurationControls: this.configurationControls
            };
        }

        clone(): ActionDTO {
            var result = new ActionDTO(this.parentActivityId, this.id, this.isTempId);
            result.name = this.name;

            return result;
        }

        static isActionValid(action: interfaces.IActionVM) {
            return action && action.$resolved && !action.isTempId
        }

        static create(dataObject: interfaces.IActionDTO): ActionDTO {
            var result = new ActionDTO(0, 0, false);
            result.activityTemplateId = dataObject.activityTemplateId;
            result.crateStorage = dataObject.crateStorage;
            result.configurationControls = dataObject.configurationControls;
            result.id = dataObject.id;
            result.isTempId = dataObject.isTempId;
            result.name = dataObject.name;
            result.parentActivityId = dataObject.parentActivityId;
            return result;
        }
    }
}