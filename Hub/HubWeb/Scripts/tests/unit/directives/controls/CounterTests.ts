/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias

    var compileTemplate = (localScope, rawTemplate, $compile) => {
        var template = angular.element(rawTemplate);
        var elem = $compile(template)(localScope);
        localScope.$digest();
        return elem;
    };

    var getValue = ($element) => {
        return parseInt($element.find('input').val());
    };

    var changeInputValue = ($element, newValue) => {
        $element.find('input').val(newValue).trigger('input').blur();
        $element.isolateScope().adjustValue();
        $element.scope().$apply();
    };

    var MIN_VALUE = 1;
    var TOOLTIP = 'Test tooltip';

    describe('Counter Control', () => {
        var $rootScope,
            $compile,
            $element,
            $element1,
            scope,
            directive = '<counter counter-value="value" min-value="' + MIN_VALUE + '" counter-tooltip="\'' + TOOLTIP + '\'"/>',
            directive1 = '<counter counter-value="10" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and plans us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {
            inject((_$compile_, _$rootScope_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;

                scope = $rootScope.$new();
                scope.value = 3;
                $element = compileTemplate(scope, directive, $compile);
                $element1 = compileTemplate(scope, directive1, $compile);
            });
        });


        /* SPECS */

        it('should be compiled', () => {
            expect($element.find('a.btn').length).toBe(2);
            expect($element.find('input').length).toBe(1);
        });

        it('should apply the specified value of counter', () => {
            expect(getValue($element)).toEqual(scope.value);
        });

        it('should change the value in scope when input value is changed', () => {
            changeInputValue($element, 15);
            expect(parseInt(scope.value)).toBe(15);
        });

        it('should increment value on up button click', () => {
            var $button = $element.find('.glyphicon-chevron-up');
            var value = scope.value;
            $button.parent().trigger('click');

            expect(getValue($element)).toBe(parseInt(value) + 1);
        });

        it('should decrement value on down button click', () => {
            var $button = $element.find('.glyphicon-chevron-down');
            var value = scope.value;
            $button.parent().trigger('click');

            expect(getValue($element)).toBe(parseInt(value) - 1);
        });

        it('should not allow value go below min value', () => {
            changeInputValue($element, -10);

            $element.find('.glyphicon-chevron-down').parent().click();
            expect(getValue($element)).toBe(MIN_VALUE);
        });

        it('should apply tooltip to the input node', () => {
            var $input = $element.find('input');
            expect($input.attr('tooltip')).toBe(TOOLTIP);
        });

        it('should keep the previous value if not a number is set to input', () => {
            var value = getValue($element);
            changeInputValue($element, 'asdf');
            expect(getValue($element)).toBe(value);
        });

        describe('multiple elements', () => {

            it('should apply own value to own scope', () => {
                expect(getValue($element)).not.toBe(getValue($element1));
                expect($element.isolateScope()).not.toBe($element1.isolateScope());
                expect($element.isolateScope().counterValue).not.toBe($element1.isolateScope().counterValue);
            });

            it('should change only own scope values by buttons', () => {
                var value = getValue($element);
                var value1 = getValue($element1);

                var $button = $element.find('.glyphicon-chevron-down');
                $button.parent().trigger('click');

                expect(getValue($element)).not.toBe(value);
                expect(getValue($element1)).toBe(value1);
            });

            it('should change only own scope values by input change', () => {
                var value = getValue($element);
                var value1 = getValue($element1);

                changeInputValue($element1, 999);
                expect(getValue($element)).toBe(value);
                expect(getValue($element1)).not.toBe(value1);
            });
            
        });

    });
} 