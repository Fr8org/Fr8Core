/// <reference path="../_all.ts" />
module dockyard.Fr8Events {
    export enum Plan {
        SUB_PLAN_MODIFICATION = <any>"subPlanModification",
        ON_FIELD_FOCUS = <any>"onFieldFocus",
    };
    export enum FilePicker {
        FP_SUCCESS = <any>"fp-success"    
    }
    export enum DesignerHeader {
        PLAN_EXECUTION_STARTED = <any>"planExecutionStarted",
        PLAN_EXECUTION_FAILED = <any>"planExecutionFailed",
        PLAN_EXECUTION_STOPPED = <any>"planExecutionStopped",
        PLAN_EXECUTION_COMPLETED_REARRANGE_PLANS = <any>"planExecutionCompleted-rearrangePlans",
        PLAN_IS_DEACTIVATED = <any>"planIsDeactivated"
    }    
}