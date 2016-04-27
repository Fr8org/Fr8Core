module dockyard.services {

    export interface IReportService extends ng.resource.IResourceClass<interfaces.IHistoryItemDTO> {
        getIncidentsByQuery: (query: model.HistoryQueryDTO) => interfaces.IHistoryResultDTO<model.IncidentDTO>;
        getFactsByQuery: (query: model.HistoryQueryDTO) => interfaces.IHistoryResultDTO<model.FactDTO>;
    }

    app.factory('ReportService', ['$resource', ($resource: ng.resource.IResourceService): IReportService =>
        <IReportService> $resource('/api/report/getallincidents', null, {
            'getIncidentsByQuery': {
                method: 'GET',
                isArray: false,
                url: '/api/report/getIncidentsByQuery'
            },
            'getFactsByQuery': {
                method: 'GET',
                isArray: false,
                url: '/api/report/getFactsByQuery'
            }
        })
    ]);
} 