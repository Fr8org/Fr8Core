module dockyard.model {
    export class ActionDTO implements interfaces.IActionDTO {
        rootRouteNodeId: string;
        parentRouteNodeId: string;
        id: string;
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
        ordering: number;

        constructor(
            rootRouteNodeId: string,
            parentRouteNodeId: string,
            id: string,
            isTempId: boolean
        ) {
            this.rootRouteNodeId = rootRouteNodeId;
            this.parentRouteNodeId = parentRouteNodeId;
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
                configurationControls: this.configurationControls,
                ordering: this.ordering
            };
        }

        clone(): ActionDTO {
            var result = new ActionDTO(
                this.rootRouteNodeId,
                this.parentRouteNodeId,
                this.id,
                this.isTempId
            );
            result.name = this.name;
            result.name = this.label;
            result.ordering = this.ordering;
            return result;
        }

        static isActionValid(action: interfaces.IActionVM) {
            return action && action.$resolved && !action.isTempId;
        }

        static create(dataObject: interfaces.IActionDTO): ActionDTO {
            var result = new ActionDTO('', '', '', false);
            result.activityTemplateId = dataObject.activityTemplateId;
            result.activityTemplate = dataObject.activityTemplate;
            result.crateStorage = dataObject.crateStorage;
            result.configurationControls = dataObject.configurationControls;
            result.id = dataObject.id;
            result.isTempId = dataObject.isTempId;
            result.name = dataObject.name;
            result.label = dataObject.label;
            result.parentRouteNodeId = dataObject.parentRouteNodeId;
            result.ordering = dataObject.ordering;
            return result;
        }
    }
}