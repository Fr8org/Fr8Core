/// <reference path="../_all.ts" />
module dockyard.interfaces {

    // TODO: Do we really need all these interfaces, since we have model type safe classes?

    export interface ICriteria {
        id: number;
        isTempId: boolean;
        userLabel: string;
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
        configurationSettings: model.ConfigurationSettings;
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
