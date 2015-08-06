/// <reference path="../_all.ts" />

module dockyard.interfaces {

    export interface IPaneWorkflowDesignerSelectCriteriaArgs {
        criteriaId: any
    }

    export interface IPaneWorkflowDesignerAddActionArgs {
        criteriaId: any
    }

    export interface IPaneWorkflowDesignerSelectActionArgs {
        criteriaId: any,
        actionId: any
    }

    export interface IPaneWorkflowDesignerScope extends ng.IScope {
        addCriteria: () => void,
        selectCriteria: (args: IPaneWorkflowDesignerSelectCriteriaArgs) => void,
        addAction: (args: IPaneWorkflowDesignerAddActionArgs) => void,
        selectAction: (args: IPaneWorkflowDesignerSelectActionArgs) => void
    }
} 
