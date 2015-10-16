/// <reference path="../_all.ts" />

module dockyard.interfaces {

    export interface IProcessTemplateVM extends ng.resource.IResource<model.ProcessTemplateDTO>, model.ProcessTemplateDTO { }

    export interface IReportFactVM extends ng.resource.IResource<model.FactDTO>, model.FactDTO { }

    export interface IReportIncidentVM extends ng.resource.IResource<model.IncidentDTO>, model.IncidentDTO { }

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
        id: number;
        name: string;
    }

    export interface IProcessNodeTemplateVM extends ng.resource.IResource<model.ProcessNodeTemplateDTO>, model.ProcessNodeTemplateDTO { }
    export interface ICriteriaVM extends ng.resource.IResource<model.CriteriaDTO>, model.CriteriaDTO { }
    export interface IActionVM extends ng.resource.IResource<model.ActionDTO>, model.ActionDTO { }
    export interface IIsAuthenticatedVM extends ng.resource.IResource<model.IsAuthenticatedDTO>, model.IsAuthenticatedDTO { }
    export interface IDocuSignTemplateVM extends ng.resource.IResource<IDocuSignTemplate> { }
    export interface IDocuSignExternalEventVM extends ng.resource.IResource<IDocuSignExternalEvent> { }
    export interface IExternalEventVM extends ng.resource.IResource<IExternalEvent> { }
    export interface IDataSourceListVM extends ng.resource.IResource<Array<string>> { }

    export interface IProcessBuilderController extends ng.IControllerService {
        ptvm: interfaces.IProcessTemplateVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        pbAddCriteriaClick: () => void;
        pbCriteriaClick: (criteriaId: number) => void;
        pbAddActionClick: (criteriaId: number) => void;
        pbActionClick: (criteriaId: number, actionId: number) => void;

        processBuilder: any;
    }

    export interface IControlsListVM extends ng.resource.IResource<model.ControlsList>, model.ControlsList {
    }

    export interface IActivityTemplateVM extends ng.resource.IResource<model.ActivityTemplate>, model.ActivityTemplate {
    }

}