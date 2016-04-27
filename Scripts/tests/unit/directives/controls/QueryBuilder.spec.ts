module dockyard.tests.unit.directives.controls {
    import dd = dockyard.directives;
    
    describe('Query Builder', () => {
        let $compile: ng.ICompileService,
            $rootScope: ng.IRootScopeService,
            scope: dd.paneConfigureAction.IConfigurationControlScope,
            elScope: dd.IQueryBuilderScope,
            element: ng.IAugmentedJQuery,
            emptyCondition: dd.IQueryCondition;

        beforeEach(module('app'));

        beforeEach(inject(($injector) => {
            $compile = $injector.get('$compile');
            $rootScope = $injector.get('$rootScope');
            scope = <dd.paneConfigureAction.IConfigurationControlScope> $rootScope.$new();
            scope.currentAction = angular.copy(dockyard.tests.utils.fixtures.ActivityDTO.typedFieldsVM);
        }));
        
        it('loads a condition from the field object', () => {
            scope.field = <model.ControlDefinitionDTO> {
                value: '[{ "field": "CreatedById", "operator": "gt", "value": "test value" }]'
            };
            element = $compile("<query-builder current-action='currentAction' field='field'>")(scope);
            scope.$digest();
            elScope = <dd.IQueryBuilderScope> element.isolateScope();
            expect(elScope.conditions.length).toBe(1);
            expect(elScope.conditions[0].value).toEqual('test value');
        });

        it('adds an empty codition if there are none in the field object', () => {
            scope.field = <model.ControlDefinitionDTO> {
                value: '[]'
            };
            element = $compile("<query-builder current-action='currentAction' field='field'>")(scope);
            scope.$digest();
            elScope = <dd.IQueryBuilderScope> element.isolateScope();
            emptyCondition = {
                field: null,
                operator: elScope.defaultOperator,
                value: null
            };
            expect(elScope.conditions.length).toBe(1);
            expect(elScope.conditions[0]).toEqual(emptyCondition);
        });
    });
}
