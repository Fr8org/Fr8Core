/// <reference path="../_all.ts" />
module dockyard.interfaces {

    // TODO: Do we really need all these interfaces, since we have model type safe classes?

    export interface ICriteriaDTO {
        id: number;
        isTempId: boolean;
        actions: Array<IActionDTO>;
        conditions: Array<ICondition>;
        executionType: model.CriteriaExecutionType;
    }

    export interface IActionDTO {
        id: number,
        isTempId: boolean, 
        parentRouteNodeId: number,
        name: string;
        crateStorage: model.CrateStorage;
        configurationControls: model.ControlsList;
        activityTemplateId: number;
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
