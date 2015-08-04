/// <reference path="../_all.ts" />

module dockyard.interfaces {


    export interface ICriteria {
        id: number,
        name: string,
        actions: Array<IAction>,
        conditions: Array<ICondition>,
        executionMode: string
    }

    export interface IAction {
        id: number,
        name: string
    }

    export interface ICondition {
        field: string,
        operator: string,
        value: string
    }

    export interface IField {
        key: string, name: string
    }
}