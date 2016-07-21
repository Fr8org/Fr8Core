module dockyard.services {
    export interface IManageAuthTokenService
        extends ng.resource.IResourceClass<interfaces.IAuthenticationTokenTerminalVM> {

        list(): Array<interfaces.IAuthenticationTokenTerminalVM>;
        revoke(authToken: any);
    }

    /*
        FilesDTO CRUD service.
    */
    app.factory(
        'ManageAuthTokenService',
        [
            '$resource',
            ($resource: ng.resource.IResourceService): IManageAuthTokenService =>
                <IManageAuthTokenService> $resource(
                    '/api/authentication',
                    {},
                    {
                        list: {
                            method: 'GET',
                            isArray: true,
                            url: '/api/authentication/tokens'
                        },

                        revoke: {
                            method: 'POST',
                            url: '/api/authentication/tokens/revoke/:id',
                            params: {
                                id: '@id'
                            }
                        }
                    }
                )
        ]
    );
}