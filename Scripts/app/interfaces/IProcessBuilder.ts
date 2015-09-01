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
    export interface IDataSourceListVM extends ng.resource.IResource<Array<string>> { }

    export interface IProcessBuilderController extends ng.IControllerService {
        ptvm: interfaces.IProcessTemplateVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        pbAddCriteriaClick: () => void,
        pbCriteriaClick: (criteriaId: number) => void,
        pbAddActionClick: (criteriaId: number) => void,
        pbActionClick: (criteriaId: number, actionId: number) => void,
        processBuilder: any
    }

    export interface IProcessBuilderScope extends ng.IScope {
        processTemplateId: number;
        processNodeTemplates: Array<model.ProcessNodeTemplate>,
        fields: Array<model.Field>;

        // Identity of currently selected processNodeTemplate.
        curNodeId: number;
        // Flag, that indicates if currently selected processNodeTemplate has temporary identity.
        curNodeIsTempId: boolean;
        currentProcessTemplate: interfaces.IProcessTemplateVM;
        currentAction: IActionVM;
        Save: Function;
        Cancel: Function;
        Loaded: Function;
    }

    export interface IConfigurationSettingsVM extends ng.resource.IResource<model.ConfigurationSettings>, model.ConfigurationSettings {
    }
}