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

    describe('TextArea Control', () => {
        var $rootScope,
            $compile,
            $element,
            $element1,
            scope,
            directive = '<text-area field="field" />',
            direcitve1 = '<text-area field="field1" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and routes us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {
            inject((_$compile_, _$rootScope_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;

                scope = $rootScope.$new();
                scope.field = $.extend(true, {}, fx.TextArea.sampleField);
                scope.field1 = $.extend(true, {}, fx.TextArea.readOnlyField);
                $element = compileTemplate(scope, directive, $compile);
                $element1 = compileTemplate(scope, direcitve1, $compile);
            });
        });


        /* SPECS */

        it('should be compiled', () => {
            expect($element.find('div[text-angular]').length).toBe(1);
        });

        it('should apply the value of scope field', () => {
            expect($element.find('input').val()).toBe(scope.field.value);
        });

        it('should set the not editable class if needed', () => {
            expect($element.find('div[text-angular]').hasClass('readOnlyTextArea')).toBe(false);
            expect($element1.find('div[text-angular]').hasClass('readOnlyTextArea')).toBe(true);
        });

        it('should apply the changes only in own scope', () => {
            var newVal = 'New test text value'
            scope.field.value = newVal;
            scope.$apply();

            expect($element.find('input').val()).toBe(newVal);
            expect($element1.find('input').val()).not.toBe(newVal);
        });

    });
} 