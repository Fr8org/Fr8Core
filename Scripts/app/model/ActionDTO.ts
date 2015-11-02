module dockyard.model {
    export class ActionDTO implements interfaces.IActionDTO {
        parentRouteNodeId: number;
        id: number;
        isTempId: boolean;
        name: string;
        label: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplateId: number;
        activityTemplate: ActivityTemplate;
        currentView: string;
        childrenActions: Array<interfaces.IActionDTO>;
        height: number = 300;

        constructor(
            parentActivityId: number,
            id: number,
            isTempId: boolean
        ) {
            this.parentRouteNodeId = parentActivityId;
            this.id = id;
            this.isTempId = isTempId;
            this.activityTemplateId = 0;
            this.configurationControls = new ControlsList();
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM> {
                id: this.id,
                isTempId: this.isTempId,
                parentRouteNodeId: this.parentRouteNodeId,
                name: this.name,
                label: this.label,
                crateStorage: this.crateStorage,
                configurationControls: this.configurationControls
            };
        }

        clone(): ActionDTO {
            var result = new ActionDTO(this.parentRouteNodeId, this.id, this.isTempId);
            result.name = this.name;
            result.name = this.label;
            return result;
        }

        static isActionValid(action: interfaces.IActionVM) {
            return action && action.$resolved && !action.isTempId
        }

        static create(dataObject: interfaces.IActionDTO): ActionDTO {
            var result = new ActionDTO(0, 0, false);
            result.activityTemplateId = dataObject.activityTemplateId;
            result.activityTemplate = dataObject.activityTemplate;
            result.crateStorage = dataObject.crateStorage;
            result.configurationControls = dataObject.configurationControls;
            result.id = dataObject.id;
            result.isTempId = dataObject.isTempId;
            result.name = dataObject.name;
            result.label = dataObject.label;
            result.parentRouteNodeId = dataObject.parentRouteNodeId;
            return result;
        }
    }
}