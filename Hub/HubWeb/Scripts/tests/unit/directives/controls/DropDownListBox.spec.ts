module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias
    import dd = dockyard.directives;

    let compileTemplate = (localScope, rawTemplate, $compile) => {
        let template = angular.element(rawTemplate);
        let elem = $compile(template)(localScope);
        localScope.$digest();
        return elem;
    };

    describe('DropDownListBox Control', () => {
        let $rootScope: ng.IRootScopeService,
            $compile: ng.ICompileService,
            element: ng.IAugmentedJQuery,
            scope: any,
            dirScope: dd.dropDownListBox.IDropDownListBoxScope,
            directive = '<drop-down-list-box field="field" change="onChange" />';

        beforeEach(module('app', 'templates'));

        beforeEach(() => {
            inject((_$compile_, _$rootScope_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
                
                scope = $rootScope.$new();
                scope.onChange = jasmine.createSpy("onChange function");
                //we copy data to prevent changes on ongoing tests
                scope.field = angular.copy(fx.FieldDTO.dropDownListBox);
                element = compileTemplate(scope, directive, $compile);
                dirScope = <dd.dropDownListBox.IDropDownListBoxScope>element.isolateScope();
            });
        });

        it('should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });
        
        it('should set selected item', () => {
            dirScope.setSelectedItem(scope.field.listItems[1]);
            expect(dirScope.field.value).toBe(scope.field.listItems[1].value);
            expect(dirScope.selectedItem).toBe(scope.field.listItems[1]);
        });

        it('should call onChange function when new item is selected', () => {
            dirScope.setSelectedItem(scope.field.listItems[1]);
            expect(scope.onChange).toHaveBeenCalled();
        });

        it('should update the field object when an item is selected', () => {
            dirScope.setSelectedItem(scope.field.listItems[1]);
            expect(scope.field.value).toBe(scope.field.listItems[1].value);
            expect(scope.field.selectedKey).toBe(scope.field.listItems[1].key);
        });

        it('should clear the field object when selection is cleared', () => {
            dirScope.setSelectedItem(undefined);
            expect(scope.field.value).toBe(null);
            expect(scope.field.selectedKey).toBe(null);
        });

        it('should set initial selection', () => {
            expect(dirScope.selectedItem).toBe(scope.field.listItems[2]);
        });

    });

    //MULTI USAGE TESTS

    describe('DropDownListBox multi usage', () => {
        let $rootScope: ng.IRootScopeService,
            $compile: ng.ICompileService,
            element1: ng.IAugmentedJQuery,
            element2: ng.IAugmentedJQuery,
            scope: any,
            dirScope: dd.dropDownListBox.IDropDownListBoxScope,
            directive1 = '<drop-down-list-box field="field1" change="onChange1" />',
            directive2 = '<drop-down-list-box field="field2" change="onChange2" />';

        beforeEach(module('app', 'templates'));

        beforeEach(() => {
            inject((_$compile_, _$rootScope_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;

                scope = $rootScope.$new();
                scope.onChange1 = jasmine.createSpy("onChange function");
                scope.onChange2 = jasmine.createSpy("onChange function");
                //we copy data to prevent changes on ongoing tests
                scope.field1 = angular.copy(fx.FieldDTO.dropDownListBox);
                scope.field2 = angular.copy(fx.FieldDTO.dropDownListBox);
                element1 = compileTemplate(scope, directive1, $compile);
                element2 = compileTemplate(scope, directive2, $compile);
                dirScope = <dd.dropDownListBox.IDropDownListBoxScope>element1.isolateScope();
            });
        });
    
        it('should update it\'s value but not any different control', () => {
            dirScope.setSelectedItem(scope.field1.listItems[1]);
            expect(dirScope.field.value).toBe(scope.field1.listItems[1].value);
            expect(dirScope.selectedItem).toBe(scope.field1.listItems[1]);
            let dir2Scope = <dd.dropDownListBox.IDropDownListBoxScope>element2.isolateScope();
            expect(dir2Scope.field.value).toBe(scope.field2.listItems[2].value);
            expect(dir2Scope.selectedItem).toBe(scope.field2.listItems[2]);
        });

        it('should call only own change function', () => {
            dirScope.setSelectedItem(scope.field1.listItems[1]);
            expect(scope.onChange1).toHaveBeenCalled();
            expect(scope.onChange2).not.toHaveBeenCalled();
        });

    });
}
