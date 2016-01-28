/// <reference path="../_all.ts" />

module dockyard.interfaces {

    export interface IRouteVM extends ng.resource.IResource<model.RouteDTO>, model.RouteDTO { }

    export interface IReportFactVM extends ng.resource.IResource<model.FactDTO>, model.FactDTO { }

    export interface IReportIncidentVM extends ng.resource.IResource<model.IncidentDTO>, model.IncidentDTO { }

    export interface ISubscribedDocuSignTemplates {
        routeId?: number;
        id: number;
        docuSignTemplateId: string;
    }

    export interface IExternalEventSubscription {
        id: number;
        externalEvent: number;
        routeId: number;
    }

    export interface IExternalEvent {
        id: number;
        name: string;
    }

    export interface ISubrouteVM extends ng.resource.IResource<model.SubrouteDTO>, model.SubrouteDTO { }
    export interface ICriteriaVM extends ng.resource.IResource<model.CriteriaDTO>, model.CriteriaDTO { }
    export interface IActionVM extends ng.resource.IResource<model.ActionDTO>, model.ActionDTO { }
    export interface IDocuSignTemplateVM extends ng.resource.IResource<IDocuSignTemplate> { }
    export interface IDocuSignExternalEventVM extends ng.resource.IResource<IDocuSignExternalEvent> { }
    export interface IExternalEventVM extends ng.resource.IResource<IExternalEvent> { }
    export interface IDataSourceListVM extends ng.resource.IResource<Array<string>> { }

    export interface IRouteBuilderController extends ng.IControllerService {
        ptvm: interfaces.IRouteVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        pbAddCriteriaClick: () => void;
        pbCriteriaClick: (criteriaId: number) => void;
        pbAddActionClick: (criteriaId: number) => void;
        pbActionClick: (criteriaId: number, actionId: number) => void;

        routeBuilder: any;
    }

    export interface IControlsListVM extends ng.resource.IResource<model.ControlsList>, model.ControlsList {
    }

    export interface IActivityTemplateVM extends ng.resource.IResource<model.ActivityTemplate>, model.ActivityTemplate {
    }

}