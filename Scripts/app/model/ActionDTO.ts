module dockyard.model {
    export class ActivityDTO implements interfaces.IActivityDTO {
        rootPlanNodeId: string;
        parentPlanNodeId: string;
        id: string;
        label: string;
        name: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplate: ActivityTemplate;
        currentView: string;
        childrenActivities: Array<interfaces.IActivityDTO>;
        height: number = 300;
        ordering: number;
        documentation: string;
        constructor(
            rootPlanNodeId: string,
            parentPlanNodeId: string,
            id: string
        ) {
            this.rootPlanNodeId = rootPlanNodeId;
            this.parentPlanNodeId = parentPlanNodeId;
            this.id = id;
            this.configurationControls = new ControlsList();
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM>angular.extend({}, this);
        }

        clone(): ActivityDTO {
            var result = new ActivityDTO(
                this.rootPlanNodeId,
                this.parentPlanNodeId,
                this.id
            );
            result.ordering = this.ordering;
            return result;
        }

        static isActionValid(action: interfaces.IActionVM) {
            return action && action.$resolved;
        }

        static create(dataObject: interfaces.IActivityDTO): ActivityDTO {
            var result = new ActivityDTO('', '', '');
            result.activityTemplate = dataObject.activityTemplate;
            result.crateStorage = dataObject.crateStorage;
            result.configurationControls = dataObject.configurationControls;
            result.id = dataObject.id;
            result.label = dataObject.label;
            result.name = dataObject.name;
            result.parentPlanNodeId = dataObject.parentPlanNodeId;
            result.ordering = dataObject.ordering;
            return result;
        }
    }
}