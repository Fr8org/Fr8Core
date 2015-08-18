/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export enum ProcessState {
        Inactive,
        Active 
    }

    export interface IProcessTemplateVM extends ng.resource.IResource<IProcessTemplateVM> {
        Id: number;
        Name: string;
        Description: string;
        ProcessState: ProcessState;
    }

    export interface IProcessNodeTemplateVM extends ng.resource.IResource<IProcessNodeTemplateVM> {
        Id: number;
        ProcessTemplateId: number;
        Name: string;
    }

    export interface ICriteriaVM extends ng.resource.IResource<ICriteriaVM> {
        Id: number;
        ExecutionType: number;
        Conditions: Array<model.Condition>;
    }

    export interface IActionVM extends ng.resource.IResource<IAction>, IAction {

    }

}