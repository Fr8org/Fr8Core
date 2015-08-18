/// <reference path="../_all.ts" />

/*
    The service enables operations with Process Templates
*/
module dockyard.services {
    export interface IProcessTemplateService extends ng.resource.IResourceClass<interfaces.IProcessTemplateVM> {
    }

    export interface IProcessNodeTemplateService extends ng.resource.IResourceClass<interfaces.IProcessNodeTemplateVM> {
    }

    export interface ICriteriaService extends ng.resource.IResourceClass<interfaces.ICriteriaVM> {
    }

    export interface IActionService extends ng.resource.IResourceClass<interfaces.IActionVM> {
    }

    app.factory('ProcessTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IProcessTemplateService =>
        <IProcessTemplateService> $resource(urlPrefix + '/ProcessTemplate/:id', { id: '@id' })
    ]);

    app.factory('ProcessNodeTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IProcessNodeTemplateService =>
        <IProcessNodeTemplateService> $resource(urlPrefix + '/ProcessNodeTemplate/:id', { id: '@id' })
    ]);

    app.factory('CriteriaService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): ICriteriaService =>
        <ICriteriaService> $resource(urlPrefix + '/Criteria/:id', { id: '@id' })
    ]);

    app.factory('ActionService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IActionService =>
        <IActionService> $resource(urlPrefix + '/Action/:id',
            {
                id: '@id'
            },
            {
                'save': { method: 'POST', isArray: true }
            })
    ]);
}