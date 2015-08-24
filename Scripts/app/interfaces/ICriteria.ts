/// <reference path="../_all.ts" />
module dockyard.interfaces {

    // TODO: Do we really need all these interfaces, since we have model type safe classes?

    export interface ICriteria {
        id: number,
        isTempId: boolean,
        userLabel: string,
        actions: Array<IAction>,
        conditions: Array<ICondition>,
        executionMode: string
    }

    export interface IAction {
        id: number,
        isTempId: boolean, 
        processNodeTemplateId: number,
        userLabel: string,
        actionListId: number,
        actionType: string;
        configurationSettings: string;
        fieldMappingSettings: string
    }

    export interface ICondition {
        field: string,
        operator: string,
        value: string,
        valueError: boolean
    }

    export interface IField {
        key: string,
        name: string
    }
}
