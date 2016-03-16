
module dockyard.services {

    export interface IContainerService
        extends ng.resource.IResourceClass<interfaces.IContainerVM>, interfaces.IContainerVM {

        getAll: (id: { id: string; }) => Array<interfaces.IContainerVM>;
        getSingle: (id: { id: string; }) => interfaces.IContainerVM;
        getPayload: (id: { id: string; }) => any;
        getFacts: (objectId: { objectId: string; }) => any;
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
                },
                'getPayload': {
                    method: 'GET',
                    isArray: false,
                    url: '/api/containers/payload/:id'
                },
                'getFacts': {
                    method: 'POST',
                    isArray: true,
                    url: '/api/facts/query',
                    params: {
                        objectId: '@id'
                    }
                }
            })
    ]);
} 