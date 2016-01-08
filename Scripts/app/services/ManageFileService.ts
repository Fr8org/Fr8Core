/// <reference path="../_all.ts" />
/*
    The service enables operations with Files
*/
module dockyard.services {
    export interface IManageFileService extends ng.resource.IResourceClass<interfaces.IFileVM> { }

    /*
        FilesDTO CRUD service.
    */
    app.factory('ManageFileService', ['$resource', ($resource: ng.resource.IResourceService): IManageFileService =>
        <IManageFileService> $resource('/api/files/:id', { id: '@id' })
    ]);
}