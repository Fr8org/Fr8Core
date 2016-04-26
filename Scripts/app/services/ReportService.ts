module dockyard.services {

    export interface IReportFactService extends ng.resource.IResourceClass<interfaces.IReportFactVM> { }

    export interface IReportIncidentService extends ng.resource.IResourceClass<interfaces.IReportIncidentVM> {
        getByQuery: (query: model.HistoryQueryDTO) => interfaces.IHistoryResultDTO;
    }


    app.factory('ReportFactService', ['$resource', ($resource: ng.resource.IResourceService): IReportFactService =>
        <IReportFactService> $resource('/api/report/getallfacts', {})
            
    ]);

    app.factory('ReportIncidentService', ['$resource', ($resource: ng.resource.IResourceService): IReportIncidentService =>
        <IReportIncidentService> $resource('/api/report/getallincidents', null, {
            'getByQuery': {
                method: 'GET',
                isArray: false,
                url: '/api/report/getByQuery'
            }
        })
    ]);
} 