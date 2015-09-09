/// <reference path="../_all.ts" />
module dockyard.interfaces {

    // TODO: Do we really need all these interfaces, since we have model type safe classes?

    export interface ICriteriaDTO {
        id: number;
        isTempId: boolean;
        actions: Array<IActionDesignDTO>;
        conditions: Array<ICondition>;
        executionType: model.CriteriaExecutionType;
    }

    export interface IActionDesignDTO {
        id: number,
        isTempId: boolean, 
        processNodeTemplateId: number,
        actionListId: number,
        name: string;
        crateStorage: model.CrateStorage;
        fieldMappingSettings: model.FieldMappingSettings;
        actionTemplateId: number;
        actionTemplate: model.ActionTemplate;
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

    export interface IFileDTO {
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
