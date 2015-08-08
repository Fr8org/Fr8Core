/// <reference path="../_all.ts" />

module dockyard.interfaces {

    export interface IOperator {
        text: string,
        value: string
    }

    export interface IQueryBuilderWidgetScope extends ng.IScope {
        operators: Array<IOperator>,
        fields: Array<IField>,
        rows: Array<ICondition>,
        defaultOperator: string,

        addRow: () => void,
        removeRow: (index: number) => void,
        valueChanged: (row: ICondition) => void
    }

}
 