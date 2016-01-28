module dockyard.tests.utils {


    //The class contains methods to create mocks for complex objects
    export class Factory {
        //Creates a mock for RouteBuilderController $scope
        public static GetRouteBuilderScope(rootScope: interfaces.IAppRootScope): dockyard.controllers.IRouteBuilderScope {
            var scope = <dockyard.controllers.IRouteBuilderScope>rootScope.$new();
            scope.planId = '0';
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


    export class RouteServiceMock {
        constructor($q: ng.IQService) {

            this.get = jasmine.createSpy('get').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve(fixtures.RouteBuilder.newRoute);
                def.promise.$promise = def.promise;
                return def.promise;
            });
            this.getFull = jasmine.createSpy('getFull').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve(fixtures.RouteBuilder.fullRoute);
                def.promise.$promise = def.promise;
                return def.promise;
            });
        }
        public save: any;
        public get: any;
        public getFull: any;
        public saveCurrent: any;
    }

    export class RouteBuilderServiceMock {
        constructor($q: ng.IQService) {
            this.save = jasmine.createSpy('save').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve();

                def.promise.$promise = def.promise;

                return def.promise;
            });
            this.saveCurrent = jasmine.createSpy('saveCurrent').and.callFake(() => {
                var def: any = $q.defer();
                def.resolve(fixtures.RouteBuilder.routeBuilderState);
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