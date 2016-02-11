﻿module dockyard.model {
    export class ActivityDTO implements interfaces.IActivityDTO {
        rootRouteNodeId: string;
        parentRouteNodeId: string;
        id: string;
        name: string;
        label: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplate: ActivityTemplate;
        currentView: string;
        childrenActions: Array<interfaces.IActivityDTO>;
        height: number = 300;
        ordering: number;
        documentationSupport: string;
        constructor(
            rootRouteNodeId: string,
            parentRouteNodeId: string,
            id: string
        ) {
            this.rootRouteNodeId = rootRouteNodeId;
            this.parentRouteNodeId = parentRouteNodeId;
            this.id = id;
            this.configurationControls = new ControlsList();
        }

        toActionVM(): interfaces.IActionVM {
            return <interfaces.IActionVM>angular.extend({}, this);
        }

        clone(): ActivityDTO {
            var result = new ActivityDTO(
                this.rootRouteNodeId,
                this.parentRouteNodeId,
                this.id
            );
            result.name = this.name;
            result.name = this.label;
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
            result.name = dataObject.name;
            result.label = dataObject.label;
            result.parentRouteNodeId = dataObject.parentRouteNodeId;
            result.ordering = dataObject.ordering;
            return result;
        }
    }
}