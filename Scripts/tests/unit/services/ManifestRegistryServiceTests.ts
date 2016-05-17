/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />
// <reference path="../../utils/endpoints.ts" />

module dockyard.tests.controller {

    import CrateHelper = dockyard.services.CrateHelper;
    import fx = utils.fixtures;
    import filterByTagFactory = dockyard.filters.filterByTag.factory;


    describe('ManifestRegistryService', () => {

        let mrSvc: dockyard.services.IManifestRegistryService;
        let httpBackend: ng.IHttpBackendService;
        let http: ng.IHttpService;
        //let endpoints: Endpoints = new Endpoints();

        beforeEach(module('app'));


        beforeEach(() => {
            inject((_ManifestRegistryService_, _$httpBackend_, _$http_) => {
                mrSvc = _ManifestRegistryService_;
                httpBackend = _$httpBackend_;
                http = _$http_;
            });
        });

        it('should call query endpoint',() => {
            debugger;
            let testVandName = mrSvc.checkVersionAndName({ version: 'v2.0', name: 'activity' });
            let testVersion = mrSvc.getDescriptionWithLastVersion( {name:'activity' });

            httpBackend.flush();

        });

    });
}