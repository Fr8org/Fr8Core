/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />
// <reference path="../../utils/endpoints.ts" />

module dockyard.tests.controller {
    describe('ManifestRegistryService', () => {

        let mrSvc: dockyard.services.IManifestRegistryService;
        let httpBackend: ng.IHttpBackendService;
        //let endpoints: Endpoints = new Endpoints();

        beforeEach(module('app'));

        beforeEach(() => {
            inject((_ManifestRegistryService_, _$httpBackend_) => {
                mrSvc = _ManifestRegistryService_;
                httpBackend = _$httpBackend_;
                
            });
        });

        it('should call query endpoint',() => {
            httpBackend.resetExpectations();

            httpBackend.whenPOST('/api/v1/manifest_registry/query').respond(200, { id: "" });
            httpBackend.whenGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');

            let testVandName = mrSvc.checkVersionAndName({ version: 'v2.0', name: 'activity' });
            let testVersion = mrSvc.getDescriptionWithLastVersion( {name:'activity' });

            httpBackend.flush();
            httpBackend.verifyNoOutstandingRequest();
            httpBackend.verifyNoOutstandingExpectation();

        });

    });
}