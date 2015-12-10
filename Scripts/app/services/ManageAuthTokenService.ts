module dockyard.services {
    export interface IManageAuthTokenService
        extends ng.resource.IResourceClass<interfaces.IManageAuthToken_TerminalVM> {
    }

    /*
        FilesDTO CRUD service.
    */
    app.factory(
        'ManageAuthTokenService',
        [
            '$resource',
            ($resource: ng.resource.IResourceService): IManageAuthTokenService =>
                <IManageAuthTokenService> $resource('/api/manageAuthTokens', { isArray: true })
        ]
    );
}