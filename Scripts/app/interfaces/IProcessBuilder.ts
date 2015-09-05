/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export enum ProcessState {
        Inactive = 1,
        Active = 2 
    }

    export interface IProcessTemplateVM extends ng.resource.IResource<IProcessTemplateVM> {
        id: number;
        name: string;
        description: string;
        processTemplateState: ProcessState;
        subscribedDocuSignTemplates: Array<string>;
        externalEventSubscription: Array<number>; 
    }

    export interface ISubscribedDocuSignTemplates {
        processTemplateId?: number;
        id: number;
        docuSignTemplateId: string;
    }

    export interface IExternalEventSubscription {
        id: number;
        externalEvent: number;
        processTemplateId: number;
    }
    
    export interface IExternalEvent {
        id: number,
        name: string
    }

    export interface IProcessNodeTemplateVM extends ng.resource.IResource<IProcessNodeTemplateVM> {
        id: number;
        processTemplateId: number;
        name: string;
    }

    export interface ICriteriaVM extends ng.resource.IResource<ICriteriaDTO> {
        //Id: number;
        //ExecutionType: number;
        //Conditions: Array<model.Condition>;
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

    export interface IConfigurationSettingsVM extends ng.resource.IResource<model.ConfigurationSettings>, model.ConfigurationSettings {
    }
}