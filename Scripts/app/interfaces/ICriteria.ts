/// <reference path="../_all.ts" />

module dockyard.interfaces {


    export interface ICriteria {
        id: number,
        isTempId: boolean,
        name: string,
        actions: Array<IAction>,
        conditions: Array<ICondition>,
        executionMode: string
    }

    export interface IAction {
        id: number,
        tempId: number,
        criteriaId: number;
        name: string,
        actionTypeId: number
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