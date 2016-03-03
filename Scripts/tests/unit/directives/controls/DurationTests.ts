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
        var $inputs = $element.find('input');
        return { days: parseInt($inputs[0].value), hours: parseInt($inputs[1].value), minutes: parseInt($inputs[2].value) };
    };

    describe('Duration Control', () => {
        var $rootScope,
            $compile,
            $element,
            $element1,
            scope,
            directive = '<duration field="field" />',
            direcitve1 = '<duration field="field1" />';

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
                scope.field = $.extend(true, {}, fx.Duration.sampleField);
                scope.field1 = $.extend(true, {}, fx.Duration.sampleField);
                $element = compileTemplate(scope, directive, $compile);
                $element1 = compileTemplate(scope, direcitve1, $compile);
            });
        });


        /* SPECS */

        it('should be compiled', () => {
            expect($element.find('counter').length).toBe(3);
        });

        it('should apply initial values', () => {
            var value = getValue($element);
            expect(value.days).toBe(scope.field.days);
            expect(value.hours).toBe(scope.field.hours);
            expect(value.minutes).toBe(scope.field.minutes);
        });

        it('should change days scope value when days counter value changes', () => {
            var $counters = $element.find('counter');
            var counterScope = $counters.eq(0).isolateScope();
            var days = scope.field.days;
            var hours = scope.field.hours;
            var minutes = scope.field.minutes;

            counterScope.counterValue = 15;
            counterScope.$apply();
            expect(scope.field.days).toBe(15);

            counterScope.counterValue = 1;
            counterScope.$apply();
            expect(scope.field.days).toBe(1);

            expect(hours).toBe(scope.field.hours);
            expect(minutes).toBe(scope.field.minutes);
        });

        it('should change hours scope value when hours counter value changes', () => {
            var $counters = $element.find('counter');
            var counterScope = $counters.eq(1).isolateScope();
            var days = scope.field.days;
            var hours = scope.field.hours;
            var minutes = scope.field.minutes;

            counterScope.counterValue = 15;
            counterScope.$apply();
            expect(scope.field.hours).toBe(15);

            counterScope.counterValue = 1;
            counterScope.$apply();
            expect(scope.field.hours).toBe(1);

            expect(days).toBe(scope.field.days);
            expect(minutes).toBe(scope.field.minutes);
        });

        it('should change minutes scope value when minutes counter value changes', () => {
            var $counters = $element.find('counter');
            var counterScope = $counters.eq(2).isolateScope();
            var days = scope.field.days;
            var hours = scope.field.hours;
            var minutes = scope.field.minutes;

            counterScope.counterValue = 15;
            counterScope.$apply();
            expect(scope.field.minutes).toBe(15);

            counterScope.counterValue = 1;
            counterScope.$apply();
            expect(scope.field.minutes).toBe(1);

            expect(days).toBe(scope.field.days);
            expect(hours).toBe(scope.field.hours);
        });

        it('should set the minimum values to 0', () => {
            var $counters = $element.find('counter');
            var daysScope = $counters.eq(0).isolateScope();
            var hoursScope = $counters.eq(1).isolateScope();
            var minutesScope = $counters.eq(2).isolateScope();

            daysScope.counterValue = -5;
            daysScope.$apply();
            daysScope.adjustValue();
            scope.$apply();
            expect(scope.field.days).toBe(0);

            hoursScope.counterValue = -4;
            hoursScope.$apply();
            expect(scope.field.hours).toBe(0);

            minutesScope.counterValue = -7;
            minutesScope.$apply();
            expect(scope.field.minutes).toBe(0);
        });

        describe('values cycling', () => {

            it('should increment day if hours go above 23', () => {
                var days = scope.field.days;

                scope.field.hours = 24;
                scope.$apply();
                expect(scope.field.days).toBe(days + 1);
                expect(scope.field.hours).toBe(0);

                scope.field.hours = 25;
                scope.$apply();
                expect(scope.field.days).toBe(days + 2);
                expect(scope.field.hours).toBe(1);

                scope.field.hours = 50;
                scope.$apply();
                expect(scope.field.days).toBe(days + 4);
                expect(scope.field.hours).toBe(2);
            });

            it('should decrement day if hours go below 0', () => {
                var days = scope.field.days = 10;

                scope.field.hours = -1;
                scope.$apply();
                expect(scope.field.days).toBe(days - 1);
                expect(scope.field.hours).toBe(23);

                scope.field.hours = -50;
                scope.$apply();
                expect(scope.field.days).toBe(days - 4);
                expect(scope.field.hours).toBe(22);
            });

            it('should not decrement day below 0', () => {
                scope.field.days = 0;
                scope.field.hours = -1;
                scope.$apply();

                expect(scope.field.days).toBe(0);
                expect(scope.field.hours).toBe(0);
            });

            it('should increment hours if minutes go above 59', () => {
                var hours = scope.field.hours;

                scope.field.minutes = 60;
                scope.$apply();
                expect(scope.field.hours).toBe(hours + 1);
                expect(scope.field.minutes).toBe(0);

                scope.field.minutes = 61;
                scope.$apply();
                expect(scope.field.hours).toBe(hours + 2);
                expect(scope.field.minutes).toBe(1);

                scope.field.minutes = 122;
                scope.$apply();
                expect(scope.field.hours).toBe(hours + 4);
                expect(scope.field.minutes).toBe(2);
            });

            it('should decrement hours if minutes go below 0', () => {
                var hours = scope.field.hours = 15;

                scope.field.minutes = -1;
                scope.$apply();
                expect(scope.field.hours).toBe(hours - 1);
                expect(scope.field.minutes).toBe(59);

                scope.field.minutes = -122;
                scope.$apply();
                expect(scope.field.hours).toBe(hours - 4);
                expect(scope.field.minutes).toBe(58);
            });

            it('should not decrement hours below 0 when days are 0', () => {
                scope.field.days = scope.field.hours = 0;
                scope.field.minutes = -1;
                scope.$apply();
                expect(scope.field.days).toBe(0);
                expect(scope.field.hours).toBe(0);
                expect(scope.field.minutes).toBe(0);
            });

            it('should decremenet days if minutes go below 0 and hours are 0', () => {
                scope.field.days = 5;
                scope.field.hours = 0;
                scope.field.minutes = -1;
                scope.$apply();

                expect(scope.field.days).toBe(4);
                expect(scope.field.hours).toBe(23);
                expect(scope.field.minutes).toBe(59);
            });

        });

        describe('multiple elements', () => {

            it('should have and use its own scope', () => {
                var isolateScope = $element.isolateScope();
                var isolateScope1 = $element1.isolateScope();
                var hours = isolateScope1.field.hours;

                isolateScope.field.hours = 15;
                scope.$apply();

                expect(isolateScope1.field.hours).toBe(hours);
                expect($element.find('input').eq(1).val()).toBe('15');
                expect($element1.find('input').eq(1).val()).toBe('' + hours);
            });

        });

    });
} 