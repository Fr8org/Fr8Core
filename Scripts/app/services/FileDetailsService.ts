/// <reference path="../_all.ts" />
/*
    The service enables operations with Files
*/
module dockyard.services{
    export interface IFileDetailsService extends ng.resource.IResourceClass<interfaces.IFileVM> {
        getDetails: (id: { id: string; }) => any;
    }

    /*
        FilesDTO CRUD service.
    */
    app.factory('FileDetailsService',
        ['$resource', ($resource: ng.resource.IResourceService): IFileDetailsService =>
            <IFileDetailsService> $resource('/api/fileDetails', { }, {
                getDetails: {
                    method: 'GET',
                    url: '/api/fileDetails/getDetails/:id',
                    params: {
                        id: '@id'
                    }
                }
            })
    ]);
}