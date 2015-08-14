/// <reference path="../_all.ts" />

/*
    The service enables operations with Process Templates
*/
module dockyard.services {
    export interface IProcessTemplateService extends ng.resource.IResourceClass<interfaces.IProcessTemplateVM> {
    }

    export interface IActionService extends ng.resource.IResourceClass<interfaces.IActionVM> {
    }

    app.factory('ProcessTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IProcessTemplateService =>
        <IProcessTemplateService> $resource(urlPrefix + '/ProcessTemplate/:id', { id: '@id' })
    ]);

    app.factory('ActionService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IActionService =>
        <IActionService> $resource(urlPrefix + '/Action/:id', { id: '@id' })
    ]);


}