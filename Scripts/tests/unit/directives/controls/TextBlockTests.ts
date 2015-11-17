/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />
/*
module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias


    describe('Testing TextBlock control', () => {
        var $rootScope,
            $compile,
            $timeout,
            element,
            scope,
            directive = '<text-block field="field" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and routes us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {
            
            inject((_$compile_, _$rootScope_, _$timeout_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
                $timeout = _$timeout_;

                scope = $rootScope.$new();
                scope.field = fx.ActionDesignDTO.textBlock;
                element = compileTemplate(scope, directive);
                
            });
        });

        var compileTemplate = (localScope, rawTemplate) => {
            var template = angular.element(rawTemplate);
            var elem = $compile(template)(localScope);
            localScope.$digest();
            return elem;
        };

        var getLabelArea = () => {
            return angular.element(element.find('label'));
        };

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

        it('Should contain single label area', () => {
            expect(getLabelArea().length).toBe(1);
        });

        it('Should set value of label area correctly', () => {
            expect(getLabelArea().html().trim()).toBe(scope.field.value);
        });

        it('Should have a span inside', () => {
            expect(getLabelArea().find('span').length).toBe(1);
        });
    });
} */