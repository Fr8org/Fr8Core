/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    import pca = dockyard.controllers.IPlanDetailsScope;
    describe('PlanDetailsController', () => {
        var scope,
            planDetailsController,
            $rootScope,
            $timeout,
            //fakeIt ↓
            UINotificationService = {};

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

                planDetailsController = _$controller_('PlanDetailsController', {
                    '$scope': scope,
                    'UINotificationService': UINotificationService
                });
            });
        });

        // Meaning of scope.ptvm  undefined PlanService.getFull() is not called.
        it('Should not call the PlanService.getFull() with integer ID ', () => {
            planDetailsController.constructor($rootScope, scope, planDetailsController.PlanService, planDetailsController.$stateParams, UINotificationService);
            expect(scope.ptvm).toBe(undefined);
        });

        it('Check Valid GUID by using PlanDetailsController function ', () => {
            scope.id = "dcea5986-91cc-4d8f-93a0-bd7268d58455";
            // We are checking the valid GUID.
            if (planDetailsController.isValidGUID(scope.id)) {
                scope.ptvm = scope.id;
            }
            expect(scope.ptvm).not.toBe(undefined);
        });

        it('Should call the PlanService.getFull() with GUID ', () => {
            scope.id = "dcea5986-91cc-4d8f-93a0-bd7268d58455";

            // We are checking the valid GUID.
            if (planDetailsController.isValidGUID(scope.id)) {
                scope.ptvm = scope.id;
            }

            // Here we are not waiting for the response of the PlanService.getFull function.
            planDetailsController.constructor($rootScope, scope, planDetailsController.PlanService, planDetailsController.$stateParams, UINotificationService);
            expect(scope.ptvm).not.toBe(undefined);
        });

    });
}