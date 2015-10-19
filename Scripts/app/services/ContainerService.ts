
module dockyard.services {

    export interface IContainerService extends ng.resource.IResourceClass<interfaces.IContainerVM>, interfaces.IContainerVM {
        getAll: (id: { id: number; }) => Array<interfaces.IContainerVM> ;
        getSingle: (id: { id: number; }) => interfaces.IContainerVM;
    }

    // Container Read service

    app.factory('ContainerService', ['$resource', ($resource: ng.resource.IResourceService): IContainerService =>
        <IContainerService>$resource('/api/containers/get/:id', { id: '@id' },
            {
                'getAll': {
                    method: 'GET',
                    isArray: true,
                    url: '/api/containers/get'
                },
                'getSingle': {
                    method: 'GET',
                    isArray: false,
                    url: '/api/containers/get/:id'
                }
            }
            )
    ]);
} 