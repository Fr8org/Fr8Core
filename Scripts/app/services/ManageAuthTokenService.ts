module dockyard.services {
    export interface IManageAuthTokenService
        extends ng.resource.IResourceClass<interfaces.IManageAuthToken_TerminalVM> {

        list(): Array<interfaces.IManageAuthToken_TerminalVM>;
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
                    '/api/manageAuthTokens',
                    {},
                    {
                        list: {
                            method: 'GET',
                            isArray: true,
                            url: '/api/manageAuthToken'
                        },

                        revoke: {
                            method: 'POST',
                            url: '/api/manageAuthToken/revoke/:id',
                            params: {
                                id: '@id'
                            }
                        }
                    }
                )
        ]
    );
}