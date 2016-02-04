/// <reference path="../_all.ts" />
module dockyard.interfaces {

    // TODO: Do we really need all these interfaces, since we have model type safe classes?

    export interface ICriteriaDTO {
        id: string;
        isTempId: boolean;
        actions: Array<IActivityDTO>;
        conditions: Array<ICondition>;
        executionType: model.CriteriaExecutionType;
    }

    export interface IActivityDTO {
        id: string;
        isTempId: boolean;
        parentRouteNodeId: string;
        label?: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplate: model.ActivityTemplate;
        activityTemplateId: number;
        childrenActions: Array<IActivityDTO>;
        ordering: number;
    }

    export interface IActivityCategoryDTO {
        name: string;
        activities: Array<IActivityTemplateVM>
    }

    export interface ICondition {
        field: string;
        operator: string;
        value: string;
        valueError: boolean;
    }

    export interface IField {
        key: string;
        name: string;
    }

    export interface IFileDescriptionDTO {
        id: number;
        originalFileName: string;
    }

    export interface IDocuSignTemplate {
        id: number;
        name: string;
        description: string;
    }

    export interface IDocuSignExternalEvent {
        id: number;
        name: string;
        description: string;
    }
}
