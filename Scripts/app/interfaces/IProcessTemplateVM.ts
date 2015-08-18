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

    export interface IActionVM extends ng.resource.IResource<IAction>, IAction {

    }

}