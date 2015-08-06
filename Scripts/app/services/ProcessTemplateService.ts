/// <reference path="../_all.ts" />

/*
    The service enables operations with Process Templates
*/
module dockyard.services {
    export interface IProcessTemplateVMService extends ng.resource.IResourceClass<interfaces.IProcessTemplateVM> {
    }

    app.factory('ProcessTemplateService', ['$resource', ($resource: ng.resource.IResourceService): IProcessTemplateVMService =>
        <IProcessTemplateVMService> $resource('/api/ProcessTemplate/:id', { id: '@id' })
    ]);
}