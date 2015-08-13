/// <reference path="../../_all.ts" />

module dockyard.directives.paneDefineCriteria {
    export interface IPaneDefineCriteriaScope extends ng.IScope {
        isVisible: boolean,
        removeCriteria: () => void,
        operators: Array<interfaces.IOperator>,
        defaultOperator: string,
        criteria: model.Criteria,
        fields: Array<model.Field>,
    }
}
