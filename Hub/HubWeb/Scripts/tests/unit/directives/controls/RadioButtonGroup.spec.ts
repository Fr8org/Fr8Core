/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />


module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias

    var compileTemplate = (localScope, rawTemplate, $compile) => {
        var template = angular.element(rawTemplate);
        var pcaCtrl = {
            registerControl: () => { }
        };
        template.data('$paneConfigureActionController', pcaCtrl);
        var elem = $compile(template)(localScope);
        localScope.$digest();
        return elem;
    };

    describe('Testing RadioButtonGroup control', () => {
        var $rootScope,
            $compile,
            $timeout,
            element,
            scope,
            directive = '<radio-button-group field="field" currentAction="currentAction" />';

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
                scope.field = angular.copy(fx.FieldDTO.radioButtonGroupField);
                element = compileTemplate(scope, directive, $compile);
            });
        });

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

        it('Should have multiple radio buttons', () => {
            expect(element.find('input[type=radio]').length).toBe(2);
        });

        it('Shouldn\'t have any checked radio', () => {
            expect(element.find('input[type=radio]:checked').length).toBe(0);
        });

        it('Should update selected field on radio button click', () => {
            expect(element.find('input[type=radio]:checked').length).toBe(0);
            expect((<model.RadioButtonGroup>scope.field).radios[0].selected).toBe(false);
            element.find('input[type=radio]').first().prop("checked", true).trigger("click");
            scope.$digest();
            expect((<model.RadioButtonGroup>scope.field).radios[0].selected).toBe(true);
            expect(element.find('input[type=radio]:checked').length).toBe(1);
        });
    });

    //MULTI USAGE TESTS

    describe('Testing RadioButtonGroup multi usage', () => {
        var $rootScope,
            $compile,
            $timeout,
            element1,
            element2,
            scope,
            directive1 = '<radio-button-group field="field1" currentAction="currentAction" />',
            directive2 = '<radio-button-group field="field2" currentAction="currentAction" />';

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
                scope.field1 = angular.copy(fx.FieldDTO.radioButtonGroupField);
                scope.field2 = angular.copy(fx.FieldDTO.radioButtonGroupField);
                element1 = compileTemplate(scope, directive1, $compile);
                element2 = compileTemplate(scope, directive2, $compile);
            });
        });

        it('Should update only self group value on radio click', () => {
            expect(element1.find('input[type=radio]:checked').length).toBe(0);
            expect((<model.RadioButtonGroup>scope.field1).radios[0].selected).toBe(false);
            element1.find('input[type=radio]').first().prop("checked", true).trigger("click");
            scope.$digest();
            expect((<model.RadioButtonGroup>scope.field1).radios[0].selected).toBe(true);
            expect(element1.find('input[type=radio]:checked').length).toBe(1);

            expect((<model.RadioButtonGroup>scope.field2).radios[0].selected).toBe(false);
            expect(element2.find('input[type=radio]:checked').length).toBe(0);
        });
    });
}
