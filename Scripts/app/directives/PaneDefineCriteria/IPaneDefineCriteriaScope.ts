/// <reference path="../../_all.ts" />

module dockyard.directives.paneDefineCriteria {
    export interface IPaneDefineCriteriaScope extends ng.IScope {
        isVisible: boolean;
        removeCriteria: () => void;
        save: () => void;
        cancel: () => void;
        operators: Array<interfaces.IOperator>;
        defaultOperator: string;
        processNodeTemplate: model.ProcessNodeTemplate;
        fields: Array<model.Field>;
        currentAction: interfaces.IActionVM;
    }
}
