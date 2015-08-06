/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IPaneScope extends ng.IScope {
        criteriaList: Array<ICriteria>,
        selectedCriteria: ICriteria,
        selectedAction: IAction
    }
}