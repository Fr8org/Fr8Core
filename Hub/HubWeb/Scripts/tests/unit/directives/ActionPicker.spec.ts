/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />


module dockyard.tests.unit.directives {

    let compileTemplate = (localScope, rawTemplate, $compile) => {
        let template = angular.element(rawTemplate),
            elem = $compile(template)(localScope);
        localScope.$digest();
        return elem;
    };

    describe('Testing ActionPicker', () => {
        let $rootScope,
            $compile,
            element,
            scope,
            WebServiceService = jasmine.createSpyObj('WebServiceService', ['getActivities']),
            directive = '<action-picker></action-picker>';
    
        beforeEach(module('app', 'templates'));
    
        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and plans us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);
    
        beforeEach(() => {
            module(($provide) => {
                $provide.factory('WebServiceService', () => {
                    return WebServiceService;
                });
            });
    
            inject((_$compile_, _$rootScope_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
    
                scope = $rootScope.$new();
                element = compileTemplate(scope, directive, $compile);
            });
        });

        // FR-3969, commented out for now.
        // it('puts the "Built-In Services" service before the other services', () => {
        //     let builtinOrder = scope.sortBuiltinServices({ webServiceName: 'Built-In Services' }),
        //         otherOrder = scope.sortBuiltinServices({ webServiceName: 'Other Service' });
        //     expect(builtinOrder).toBeLessThan(otherOrder);
        // });
    });

}
