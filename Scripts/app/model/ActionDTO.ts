module dockyard.model {
    export class ActivityDTO implements interfaces.IActivityDTO {
        planId: string;
        parentPlanNodeId: string;
        id: string;
        label: string;
        authTokenId: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplate: ActivityTemplateSummary;
        childrenActivities: Array<interfaces.IActivityDTO>;
        height: number = 300;
        ordering: number;
        documentation: string;
        showAdvisoryPopup: boolean;
        advisoryMessages: model.AdvisoryMessages;
        constructor(
            planId: string,
            parentPlanNodeId: string,
            id: string
        ) {
            this.planId = planId;
            this.parentPlanNodeId = parentPlanNodeId;
            this.id = id;
            this.configurationControls = new ControlsList();
            this.showAdvisoryPopup = false;
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM>angular.extend({}, this);
        }

        clone(): ActivityDTO {
            var result = new ActivityDTO(
                this.planId,
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
            result.parentPlanNodeId = dataObject.parentPlanNodeId;
            result.ordering = dataObject.ordering;
            return result;
        }
    }
}