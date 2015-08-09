module dockyard.tests.mocks {

    //The class contains methods to create mocks for certain objects
    export class TypedScopeFactory {

        //Creates a mock for ProcessBuilderController $scope
        public static GetProcessBuilderScope(rootScope: interfaces.IAppRootScope): interfaces.IProcessBuilderScope {
            var scope = <interfaces.IProcessBuilderScope>rootScope.$new();
            scope.processTemplateId = 0;
            scope.criteria = null;
            scope.fields = null;

            return scope;
        }
    } 
}
