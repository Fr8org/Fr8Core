/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    import pca = dockyard.controllers.IRouteDetailsScope;

    describe('RouteDetailsController', () => {
        var scope,
            routeDetailsController,
            $rootScope,
            $timeout;

        beforeEach(module('app'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {
            inject((_$compile_, _$rootScope_, _$controller_, _$timeout_) => {
                $rootScope = _$rootScope_;
                scope = $rootScope.$new();
                scope.id = 1;
                $timeout = _$timeout_;
                routeDetailsController = _$controller_('RouteDetailsController', {
                     '$scope': scope
               });
            });
        });

        // Meaning of scope.ptvm  undefined RouteService.getFull() is not called.
        it('Should not call the RouteService.getFull() with integer ID ', () => {
            routeDetailsController.constructor($rootScope, scope, routeDetailsController.RouteService, routeDetailsController.$stateParams);
            expect(scope.ptvm).toBe(undefined);
        });
         
        it('Check Valid GUID by using RouteDetailsController function ', () => {
            scope.id = "dcea5986-91cc-4d8f-93a0-bd7268d58455";
            // We are checking the valid GUID.
            if (routeDetailsController.isValidGUID(scope.id)) {
                scope.ptvm = scope.id;
            }
            expect(scope.ptvm).not.toBe(undefined);
        });

        it('Should call the RouteService.getFull() with GUID ', () => {
            scope.id = "dcea5986-91cc-4d8f-93a0-bd7268d58455";

            // We are checking the valid GUID.
            if (routeDetailsController.isValidGUID(scope.id)) {
                scope.ptvm = scope.id;
            }

            // Here we are not waiting for the response of the RouteService.getFull function.
            routeDetailsController.constructor($rootScope, scope, routeDetailsController.RouteService, routeDetailsController.$stateParams);
            expect(scope.ptvm).not.toBe(undefined);
        });

    });
}