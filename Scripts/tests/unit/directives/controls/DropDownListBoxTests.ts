/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias


    describe('Testing DropDownListBox control', () => {
        var $rootScope,
            $compile,
            $timeout,
            element,
            scope,
            directive = '<drop-down-list-box field="field" change="onChange" />';

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
                scope.onChange = jasmine.createSpy("onChange function");
                //we copy data to prevent changes on ongoing tests
                scope.field = angular.copy(fx.ActionDesignDTO.dropDownListBox);
                element = compileTemplate(scope, directive);
            });
        });

        var compileTemplate = (localScope, rawTemplate) => {
            var template = angular.element(rawTemplate);
            var elem = $compile(template)(localScope);
            localScope.$digest();
            return elem;
        };

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
} 
