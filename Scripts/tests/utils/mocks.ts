module dockyard.tests.utils {


    //The class contains methods to create mocks for complex objects
    export class Factory {
        //Creates a mock for ProcessBuilderController $scope
        public static GetProcessBuilderScope(rootScope: interfaces.IAppRootScope): interfaces.IProcessBuilderScope {
            var scope = <interfaces.IProcessBuilderScope>rootScope.$new();
            scope.processTemplateId = 0;
            scope.processNodeTemplates = null;
            scope.fields = null;

            return scope;
        }
    } 

    /*
        Mock for ActionService
    */
    export class ActionServiceMock {
        constructor($q: ng.IQService) {
            this.save = jasmine.createSpy('save').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve();

                def.promise.$promise = def.promise;

                return def.promise;
            });

            this.get = jasmine.createSpy('get');
        }
        public save: any;
        public get: any;
    }
}