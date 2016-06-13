/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />

describe('PusherNotifierServic', () => {


    let pusher: pusherjs.pusher.Pusher;
    let pnSvc: dockyard.services.IPusherNotifierService;
    let httpBackend: ng.IHttpBackendService;
    let timeout: ng.ITimeoutService;


    beforeEach(module('app'));

    beforeEach(() => {
        inject((_PusherNotifierService_, _$httpBackend_, _$timeout_) => {
            pnSvc = _PusherNotifierService_;
            httpBackend = _$httpBackend_;
            timeout = _$timeout_;
            pusher = new Pusher("Test", null);
        });
    });
    it('should have channels');

    it('should post frontend failuer message');
    it('should post frontend success message');

    it('should not post using frontendEvent if eventType is emty ');
});
