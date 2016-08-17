module dockyard.services {

    export interface IReportService extends ng.resource.IResourceClass<interfaces.IHistoryItemDTO> {
        getIncidentsByQuery: (query: model.PagedQueryDTO) => interfaces.IHistoryResultDTO<model.IncidentDTO>;
        getFactsByQuery: (query: model.PagedQueryDTO) => interfaces.IHistoryResultDTO<model.FactDTO>;
        canSeeOtherUserHistory: () => any;
    }

    app.factory('ReportService', ['$resource', ($resource: ng.resource.IResourceService): IReportService =>
        <IReportService> $resource('/api/reports/getallincidents', null, {
            'getIncidentsByQuery': {
                method: 'GET',
                isArray: false,
                url: '/api/reports/?type=incidents'
            },
            'getFactsByQuery': {
                method: 'GET',
                isArray: false,
                url: '/api/reports/?type=facts'
            },
            'canSeeOtherUserHistory' : {
                method: 'GET',
                isArray: false,
                url: '/api/reports/can_see_other_user_history'
            }
        })
    ]);
} 