/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessBuilderScope extends ng.IScope {
        isCriteriaSelected: () => boolean;
        selectCriteria: (int) => void;
        selectAction: (criteriaId: number, actionId: number) => void
        addAction: (criteriaId: number) => void;
        addCriteria: () => void;
        removeCriteria: () => void;

        criteria: Array<ICriteria>;
        selectedCriteria: ICriteria;
        fields: Array<IField>;
    }
} 