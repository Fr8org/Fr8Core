/// <reference path="../_all.ts" />

module dockyard.interfaces {

    export interface IPlanVM extends ng.resource.IResource<model.PlanDTO>, model.PlanDTO { }

    //tony.yakovets: it was:
    //export interface IPlanFullDTO extends ng.resource.IResource<model.PlanDTO>, model.PlanFullDTO { }
    //but after PlanFullDTO to PlanDTO refactoring it is the same as IPlanVM
    //export interface IPlanDTO extends ng.resource.IResource<model.PlanDTO>, model.PlanDTO { }

    export interface IPlanResultDTO extends ng.resource.IResource<model.PlanResultDTO>, model.PlanResultDTO { }

    export interface IHistoryResultDTO<T> extends ng.resource.IResource<model.PagedResultDTO<T>>, model.PagedResultDTO<T> { }

    export interface IReportFactVM extends ng.resource.IResource<model.FactDTO>, model.FactDTO { }

    export interface IHistoryItemDTO extends ng.resource.IResource<model.HistoryItemDTO>, model.HistoryItemDTO { }

    export interface ISubscribedDocuSignTemplates {
        planId?: number;
        id: number;
        docuSignTemplateId: string;
    }

    export interface IExternalEventSubscription {
        id: number;
        externalEvent: number;
        planId: number;
    }

    export interface IExternalEvent {
        id: number;
        name: string;
    }

    export interface ISubPlanVM extends ng.resource.IResource<model.SubPlanDTO>, model.SubPlanDTO { }
    export interface ICriteriaVM extends ng.resource.IResource<model.CriteriaDTO>, model.CriteriaDTO { }
    export interface IActionVM extends ng.resource.IResource<model.ActivityDTO>, model.ActivityDTO { }
    export interface IDocuSignTemplateVM extends ng.resource.IResource<IDocuSignTemplate> { }
    export interface IOrganizationVM extends ng.resource.IResource<model.OrganizationDTO>, model.OrganizationDTO { }
    export interface IDocuSignExternalEventVM extends ng.resource.IResource<IDocuSignExternalEvent> { }
    export interface IExternalEventVM extends ng.resource.IResource<IExternalEvent> { }
    export interface IDataSourceListVM extends ng.resource.IResource<Array<string>> { }

    export interface IPlanBuilderController extends ng.IControllerService {
        ptvm: interfaces.IPlanVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        pbAddCriteriaClick: () => void;
        pbCriteriaClick: (criteriaId: number) => void;
        pbAddActionClick: (criteriaId: number) => void;
        pbActionClick: (criteriaId: number, actionId: number) => void;

        planBuilder: any;
    }

    export interface IControlsListVM extends ng.resource.IResource<model.ControlsList>, model.ControlsList {
    }

    export interface IActivityTemplateVM extends ng.resource.IResource<model.ActivityTemplate>, model.ActivityTemplate {
    }

}