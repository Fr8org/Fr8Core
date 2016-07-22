/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias

    var compileTemplate = (localScope, rawTemplate, $compile) => {
        var template = angular.element(rawTemplate);
        var pcaCtrl = {
            registerControl: () => {}
        };
        template.data('$paneConfigureActionController', pcaCtrl);
        var elem = $compile(template)(localScope);
        localScope.$digest();
        return elem;
    };

    var getTextInput = (curElement) => {
        return angular.element(curElement.find('input[type=text]'));
    };

    //this simulates a type and blur effect on textbox
    var changeText = (scope, curElement, newText) => {
        getTextInput(curElement).val(newText).trigger('input');
        getTextInput(curElement).blur();
        scope.$apply();
    };

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
                scope.currentAction = angular.copy(fx.ActivityDTO.noAuthActionVM);
                scope.field = angular.copy(fx.FieldDTO.textField);
                element = compileTemplate(scope, directive, $compile);
                
            });
        });

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

        it('Should contain single input type=text', () => {
            expect(getTextInput(element).length).toBe(1);
        });

        it('Should set value of input correctly', () => {
            expect(getTextInput(element).val()).toBe(scope.field.value);
        });

        it('Should update model value on value change when blurs', () => {
            changeText(scope, element, 'super-complex-test-value');
            expect(element.isolateScope().field.value).toBe('super-complex-test-value');
        });

        it('Should call onchange method when value changes', () => {
            spyOn(element.isolateScope(), 'onChange');
            changeText(scope, element, 'super-complex-test-value');
            expect(element.isolateScope().onChange).toHaveBeenCalled();
        });
    });

    ///MULTI-USAGE TESTS

    describe('Testing TextBox multi usage', () => {

        var $rootScope,
            $compile,
            $timeout,
            element1,
            element2,
            scope,
            directive1 = '<configuration-control current-action="currentAction" field="field1" />',
            directive2 = '<configuration-control current-action="currentAction" field="field2" />';

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
                scope.currentAction = fx.ActivityDTO.noAuthActionVM;
                scope.field1 = angular.copy(fx.FieldDTO.textField);
                scope.field2 = angular.copy(fx.FieldDTO.textField);
                element1 = compileTemplate(scope, directive1, $compile);
                element2 = compileTemplate(scope, directive2, $compile);

            });
        });

        it('Should update it\'s value but not any different control', () => {
            changeText(scope, element1, 'super-complex-test-value'); 
            expect(element1.isolateScope().field.value).toBe('super-complex-test-value');
            expect(element2.isolateScope().field.value).toBe(fx.FieldDTO.textField.value);
        });

        it('Should call only own change function', () => {
            spyOn(element1.isolateScope(), 'onChange');
            spyOn(element2.isolateScope(), 'onChange');
            changeText(scope, element1, 'super-complex-test-value');
            expect(element1.isolateScope().onChange).toHaveBeenCalled();
            expect(element2.isolateScope().onChange).not.toHaveBeenCalled();
        });
    });
}