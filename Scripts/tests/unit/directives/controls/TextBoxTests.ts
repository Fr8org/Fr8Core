/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias


    describe('Testing TextBox control', () => {
        var $rootScope,
            $compile,
            $timeout,
            element,
            scope,
            directive = '<configuration-control current-action="currentAction" field="field" />';

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
                scope.currentAction = fx.ActionDesignDTO.noAuthActionVM;
                scope.field = fx.ActionDesignDTO.textField;
                element = compileTemplate(scope, directive);
                
            });
        });

        var compileTemplate = (localScope, rawTemplate) => {
            var template = angular.element(rawTemplate);
            var elem = $compile(template)(localScope);
            localScope.$digest();
            return elem;
        };

        var getTextInput = () => {
            return angular.element(element.find('input[type=text]'));
        };

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

        it('Should contain single input type=text', () => {
            expect(getTextInput().length).toBe(1);
        });

        it('Should set value of input correctly', () => {
            expect(getTextInput().val()).toBe(scope.field.value);
        });

        it('Should update model value on value change when blurs', () => {
            //this simulates a type and blur effect on textbox
            getTextInput().val('super-complex-test-value').trigger('input');
            getTextInput().blur();
            scope.$apply();
            expect(element.isolateScope().field.value).toBe('super-complex-test-value');
        });
    });
} 