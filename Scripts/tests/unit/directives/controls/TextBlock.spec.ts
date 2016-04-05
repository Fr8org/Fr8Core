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

    var getLabelArea = (element) => {
        return angular.element(element.find('label'));
    };

    var getSpanArea = (element) => {
        return angular.element(element.find('span'));
    };

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
                scope.field = fx.FieldDTO.textBlock;
                element = compileTemplate(scope, directive, $compile);
                
            });
        });

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

        it('Shouldn\'t contain label area', () => {
            expect(getLabelArea(element).length).toBe(0);
        });

        it('Should set value of span correctly', () => {
            expect(getSpanArea(element).html().trim()).toBe(scope.field.value);
        });

        it('Should have a span inside', () => {
            expect(element.find('span').length).toBe(1);
        });
    });

    //MULTI USAGE TESTS


    describe('Testing TextBlock multi usage', () => {
        var $rootScope,
            $compile,
            $timeout,
            element1,
            element2,
            scope,
            directive1 = '<text-block field="field1" />',
            directive2 = '<text-block field="field2" />';

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
                scope.field1 = angular.copy(fx.FieldDTO.textBlock);
                scope.field2 = angular.copy(fx.FieldDTO.textBlock);
                scope.field2.value = 'different value';
                element1 = compileTemplate(scope, directive1, $compile);
                element2 = compileTemplate(scope, directive2, $compile);
            });
        });

        it('Should be able display different values with it\'s sibling', () => {
            expect(getSpanArea(element1).html().trim()).toBe(fx.FieldDTO.textBlock.value);
            expect(getSpanArea(element2).html().trim()).toBe('different value');
        });
    });
} 