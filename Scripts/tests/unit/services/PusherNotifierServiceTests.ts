/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />

describe('PusherNotifierServic', () => {

    let pnSvc: dockyard.services.IPusherNotifierService;
    let httpBackend: ng.IHttpBackendService;

    beforeEach(module('app'));

    beforeEach(() => {
        inject((_PusherNotifierService_, _$httpBackend_) => {
            pnSvc = _PusherNotifierService_;
            httpBackend = _$httpBackend_;

        });
    });
    it('should have channels');

    it('should post frontend failuer message');
    it('should post frontend success message');

    it('should not post using frontendEvent if eventType is emty ');
});
