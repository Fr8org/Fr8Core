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

    describe('Testing DropDownListBox control', () => {
        var $rootScope,
            $compile,
            $timeout,
            element,
            scope,
            directive = '<drop-down-list-box field="field" change="onChange" />';

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
                scope.field = angular.copy(fx.FieldDTO.dropDownListBox);
                element = compileTemplate(scope, directive, $compile);
            });
        });

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });
        
        it('Should set selected item', () => {
            var dirScope = element.isolateScope();
            dirScope.setSelectedItem(scope.field.listItems[1]);
            expect(dirScope.field.value).toBe(scope.field.listItems[1].value);
            expect(dirScope.selectedItem).toBe(scope.field.listItems[1]);
        });

        it('Should call onChange function when new item is selected', () => {
            var dirScope = element.isolateScope();
            dirScope.setSelectedItem(scope.field.listItems[1]);
            expect(scope.onChange).toHaveBeenCalled();
        });

        it('Should set initial selection', () => {
            var dirScope = element.isolateScope();
            expect(dirScope.selectedItem).toBe(scope.field.listItems[2]);
        });

    });

    //MULTI USAGE TESTS

    describe('Testing DropDownListBox multi usage', () => {
        var $rootScope,
            $compile,
            $timeout,
            element1,
            element2,
            scope,
            directive1 = '<drop-down-list-box field="field1" change="onChange1" />',
            directive2 = '<drop-down-list-box field="field2" change="onChange2" />';

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
                scope.onChange1 = jasmine.createSpy("onChange function");
                scope.onChange2 = jasmine.createSpy("onChange function");
                //we copy data to prevent changes on ongoing tests
                scope.field1 = angular.copy(fx.FieldDTO.dropDownListBox);
                scope.field2 = angular.copy(fx.FieldDTO.dropDownListBox);
                element1 = compileTemplate(scope, directive1, $compile);
                element2 = compileTemplate(scope, directive2, $compile);
            });
        });
    
        it('Should update it\'s value but not any different control', () => {
            var dirScope = element1.isolateScope();
            dirScope.setSelectedItem(scope.field1.listItems[1]);
            expect(dirScope.field.value).toBe(scope.field1.listItems[1].value);
            expect(dirScope.selectedItem).toBe(scope.field1.listItems[1]);
            var dir2Scope = element2.isolateScope();
            expect(dir2Scope.field.value).toBe(scope.field2.listItems[2].value);
            expect(dir2Scope.selectedItem).toBe(scope.field2.listItems[2]);
        });

        it('Should call only own change function', () => {
            var dirScope = element1.isolateScope();
            dirScope.setSelectedItem(scope.field1.listItems[1]);
            expect(scope.onChange1).toHaveBeenCalled();
            expect(scope.onChange2).not.toHaveBeenCalled();
        });

    });
}
