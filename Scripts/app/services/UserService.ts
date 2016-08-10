/// <reference path="../_all.ts" />

/*
    The service implements centralized string storage.
*/

module dockyard.services {

    export interface IUserService extends ng.resource.IResourceClass<interfaces.IUserDTO> {
        getAll: () => Array<interfaces.IUserDTO>,
        getCurrentUser: () => interfaces.IUserDTO,
        getProfiles: () => Array<interfaces.IProfileDTO>,
        updateUserProfile: (data: interfaces.IUserDTO) => any,
        update: (data: { oldPassword: string, newPassword: string, confirmPassword: string }) => any;
        checkPermission: (data: { permissionType: enums.PermissionType, objectType: string }) => any;
    }

    app.factory('UserService', [
        '$resource', ($resource: ng.resource.IResourceService): IUserService =>
            <IUserService>$resource('/api/users/userdata?id=:id', { id: '@id' }, {
                getAll: {
                    method: 'GET',
                    isArray: true,
                    url: '/api/users'
                },
                getCurrentUser: {
                    method: 'GET',
                    isArray: false,
                    url: '/api/users/userdata'
                },
                getProfiles: {
                    method: 'GET',
                    isArray: true,
                    url: '/api/users/getProfiles',
                    cache: true
                },
                update: {
                    method: 'POST',
                    isArray: false,
                    url: '/api/users/update/',
                    params: {
                        oldPassword: '@oldPassword',
                        newPassword: '@newPassword',
                        confirmPassword: '@confirmPassword'
                    }
                },
                checkPermission: {
                    method: 'GET',
                    isArray: false,
                    cache: true,
                    url: '/api/users/checkpermission',
                    params: {
                        permissionType: '@permissionType',
                        objectType: '@objectType',
                        username: '@userId'
                    }
                },
                updateUserProfile: {
                    method: 'POST',
                    isArray: false,
                    url: '/api/users/updateUserProfile/'
                }
            })
    ]);
}