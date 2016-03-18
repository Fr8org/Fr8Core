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

    var getButton = (curElement,selector) => {
        return angular.element(curElement.find(selector));
    };

    //emulate the click 
    var triggerHandler = (scope, curElement, selector) => {
        getButton(curElement, selector).click();
        scope.$apply();
    };

    describe('Testing FieldList control', () => {
        var $rootScope,
            $compile,
            $timeout,
            element,
            scope,
            directive = '<field-list field="field" change="onChange" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and plans us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);


        beforeEach(() => {

            inject((_$compile_, _$rootScope_, _$timeout_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
                $timeout = _$timeout_;

                scope = $rootScope.$new();
                scope.onChange = jasmine.createSpy("onChange function");

                //we copy data to prevent changes on ongoing tests
                scope.field = angular.copy(fx.FieldDTO.fieldList);
                element = compileTemplate(scope, directive, $compile);
            });
        });

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

        it('Should have initial row of fields. ', () => {
            var currentScope = element.isolateScope();
            expect(currentScope.field.value).not.toBe(null);
        });

        it('Should add the new row of fields', () => {
            var currentScope = element.isolateScope();
            triggerHandler(scope, element, '.field-list-add-button');
            var fieldList = JSON.parse(currentScope.field.value);

            expect(fieldList.length).toBe(2);
            expect(currentScope.field.value).not.toBe(null);
        });

        it('Should remove the existing row of fields', () => {
            var currentScope = element.isolateScope();
            triggerHandler(scope, element, '.field-list-remove-button');
            var fieldList = JSON.parse(currentScope.field.value);
            expect(fieldList.length).toBe(0);
        });
    });


    // MULTI USAGE 

    describe('Testing FieldList multi usage', () => {
        var $rootScope,
            $compile,
            $timeout,
            element1,
            element2,
            scope,
            directive1 = '<field-list field="field1" change="onChange1" />',
            directive2 = '<field-list field="field2" change="onChange2" />';

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
                scope.onChange1 = jasmine.createSpy("onChange1 function");
                scope.onChange2 = jasmine.createSpy("onChange2 function");

                //we copy data to prevent changes on ongoing tests
                scope.field1 = angular.copy(fx.FieldDTO.fieldList);
                scope.field2 = angular.copy(fx.FieldDTO.fieldList);

                element1 = compileTemplate(scope, directive1, $compile);
                element2 = compileTemplate(scope, directive2, $compile);
            });
        });

        it('Should create new row of fields in its own scope', () => {

            triggerHandler(scope, element1, '.field-list-add-button');
            expect(scope.field1.value).not.toBe(null);
            var fieldList1 = JSON.parse(scope.field1.value);
            expect(fieldList1.length).toBe(2);

            triggerHandler(scope, element2, '.field-list-add-button');
            var fieldList2 = JSON.parse(scope.field2.value);
            expect(fieldList2.length).toBe(2);
        });

        it('Should remove row of fields in its own scope.', () => {
            var curScope1 = element1.isolateScope();
            triggerHandler(scope, element1, '.field-list-remove-button');
            var fieldList1 = JSON.parse(curScope1.field.value);
            expect(fieldList1.length).toBe(0);

            var curScope2 = element1.isolateScope();
            triggerHandler(curScope2, element2, '.field-list-remove-button');
            var fieldList2 = JSON.parse(curScope2.field.value);
            expect(fieldList2.length).toBe(0);
        });
    });
}
