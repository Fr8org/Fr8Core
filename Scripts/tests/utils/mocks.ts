module dockyard.tests.utils {


    //The class contains methods to create mocks for complex objects
    export class Factory {
        //Creates a mock for ProcessBuilderController $scope
        public static GetProcessBuilderScope(rootScope: interfaces.IAppRootScope): dockyard.controllers.IProcessBuilderScope {
            var scope = <dockyard.controllers.IProcessBuilderScope>rootScope.$new();
            scope.routeId = 0;
            scope.subroutes = null;
            scope.fields = null;

            return scope;
        }
    } 

    /*
        Mock for ActionService
    */
    export class ActionServiceMock {
        constructor($q: ng.IQService) {
            this.get = jasmine.createSpy('get');
            this.save = jasmine.createSpy('save');
            this.configure = jasmine.createSpy('configure');

            if ($q) {
                this.save.and.callFake(() => {
                    var def: any = $q.defer();
                    def.resolve();

                    def.promise.$promise = def.promise;

                    return def.promise;
                });
            }
        }
        public save: any;
        public get: any;
        public configure: any;
        
    }


    export class ProcessTemplateServiceMock {
        constructor($q: ng.IQService) {

            this.get = jasmine.createSpy('get').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve(fixtures.ProcessBuilder.newProcessTemplate);
                def.promise.$promise = def.promise;
                return def.promise;
            });
            this.getFull = jasmine.createSpy('getFull').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve(fixtures.ProcessBuilder.fullProcessTemplate);
                def.promise.$promise = def.promise;
                return def.promise;
            });
        }
        public save: any;
        public get: any;
        public getFull: any;
        public saveCurrent: any;
    }

    export class ProcessBuilderServiceMock {
        constructor($q: ng.IQService) {
            this.save = jasmine.createSpy('save').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve();

                def.promise.$promise = def.promise;

                return def.promise;
            });
            this.saveCurrent = jasmine.createSpy('saveCurrent').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve(fixtures.ProcessBuilder.processBuilderState);
                def.promise.$promise = def.promise;
                return def.promise;
            });
        }
        public save: any;
        public get: any;
        public saveCurrent: any;
    }

    export class $ModalMock {
        constructor($q: ng.IQService) {
            this.open = jasmine.createSpy('open').and.returnValue({
                result: $q.when(fixtures.ActivityTemplate.activityTemplateDO)
            });
        }
        public open: any;
    }
}