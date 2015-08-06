/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {

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
        widget: any
    }
} 
