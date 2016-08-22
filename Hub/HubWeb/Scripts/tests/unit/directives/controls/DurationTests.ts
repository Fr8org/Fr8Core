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
            expect($element.find('time-picker').length).toBe(1);
        });

        it('should apply initial values', () => {

            var $counters = $element.find('time-picker');
            var timerPickerScope = $counters.isolateScope();
            expect(scope.field.days).toBe(timerPickerScope.duration.days);
            expect(scope.field.hours).toBe(timerPickerScope.duration.hours);
            expect(scope.field.minutes).toBe(timerPickerScope.duration.minutes);

        });

        it('should change days scope value when days counter value changes', () => {

            var timePicker = $element.find('time-picker').isolateScope();

            var days = timePicker.duration.days;
            var hours = scope.field.hours;
            var minutes = scope.field.minutes;

            timePicker.duration.days = 15;
            timePicker.$apply();
            expect(scope.field.days).toBe(15);

        });

        it('should change hours scope value when hours counter value changes', () => {
            var $counters = $element.find('time-picker');
            var timePickerScope = $counters.isolateScope();

            var days = scope.field.days;
            var hours = scope.field.hours;
            var minutes = scope.field.minutes;

            timePickerScope.duration.days = 15;
            timePickerScope.$apply();
            expect(scope.field.days).toBe(15);
        });

        it('should change minutes scope value when minutes counter value changes', () => {
            var $counters = $element.find('time-picker');
            var counterScope = $counters.isolateScope();

            var days = scope.field.days;
            var hours = scope.field.hours;
            var minutes = scope.field.minutes;

            counterScope.duration.minutes = 15;
            counterScope.$apply();
            expect(scope.field.minutes).toBe(15);
        });


        describe('multiple elements', () => {

            it('should have and use its own scope', () => {
                var isolateScope = $element.isolateScope();
                var isolateScope1 = $element1.isolateScope();

                expect(isolateScope.id).toBe($element.isolateScope().id);
                expect(isolateScope1.id).toBe($element1.isolateScope().id);
            });

        });

    });
} 