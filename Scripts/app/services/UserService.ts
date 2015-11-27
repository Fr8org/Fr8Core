/// <reference path="../_all.ts" />

/*
    The service implements centralized string storage.
*/

module dockyard.services {

    export interface IUserService extends ng.resource.IResourceClass<interfaces.IUserDTO> {
        getAll: () => Array<interfaces.IUserDTO>
        getCurrentUser: () => interfaces.IUserDTO
    }

    app.factory('UserService', [
        '$resource', ($resource: ng.resource.IResourceService): IUserService =>
        <IUserService> $resource('/api/user?id=:id', { id: '@id' }, {
            getAll: {
                method: 'GET',
                isArray: true,
                url: '/api/user'
                },
            getCurrentUser: {
                method: 'GET',
                isArray: false,
                url: '/api/user/getCurrent'
            }
        })
    ]);
}