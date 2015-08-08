/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessBuilderScope extends ng.IScope {
        isCriteriaSelected: () => boolean,
        isActionSelected: () => boolean,
        selectCriteria: (int) => void,
        selectAction: (criteriaId: number, actionId: number) => void,
        addAction: (criteriaId: number) => void,
        addCriteria: () => void,
        removeCriteria: () => void,
        removeAction: (criteriaId: number, actionId: number) => void,

        criteria: Array<ICriteria>,
        selectedCriteria: ICriteria,
        selectedAction: IAction,
        fields: Array<IField>
    }
} 