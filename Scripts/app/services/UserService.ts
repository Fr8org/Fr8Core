/// <reference path="../_all.ts" />

/*
    The service implements centralized string storage.
*/

module dockyard.services {

    export interface IUserService extends ng.resource.IResourceClass<interfaces.IUserDTO> {
        getAll: () => Array<interfaces.IUserDTO>,
        getCurrentUser: () => interfaces.IUserDTO,
        getProfiles: () => Array<interfaces.IProfileDTO>,
        updateUserProfile: (data: { userId: string, profileId: string, userClass: string }) => any,
        update: (data: { oldPassword: string, newPassword: string, confirmPassword: string }) => any;
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
                updateUserProfile: {
                    method: 'POST',
                    isArray: false,
                    url: '/api/users/updateUserProfile/',
                    params: {
                        userId: '@userId',
                        profileId: '@profileId',
                        userClass: '@userClass'
                    }
                }
            })
    ]);
}