/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export enum ProcessState {
        Inactive = 1,
        Active = 2 
    }

    export interface IProcessTemplateVM extends ng.resource.IResource<IProcessTemplateVM> {
        Id: number;
        Name: string;
        Description: string;
        ProcessTemplateState: ProcessState;
        SubscribedDocuSignTemplates: Array<string>;
        ExternalEventSubscription: Array<number>; 
    }

    export interface ISubscribedDocuSignTemplates {
        ProcessTemplateId?: number;
        Id: number;
        DocuSignTemplateId: string;
    }

    export interface IExternalEventSubscription {
        Id: number;
        ExternalEvent: number;
        ProcessTemplateId: number;
    }
    
    export interface IExternalEvent {
        id: number,
        name: string
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

    export interface IActionVM extends ng.resource.IResource<model.ActionDesignDTO>, model.ActionDesignDTO { }
    export interface IDocuSignTemplateVM extends ng.resource.IResource<IDocuSignTemplate> { }
    export interface IDocuSignExternalEventVM extends ng.resource.IResource<IDocuSignExternalEvent> { }
    export interface IExternalEventVM extends ng.resource.IResource<IExternalEvent> { }
}