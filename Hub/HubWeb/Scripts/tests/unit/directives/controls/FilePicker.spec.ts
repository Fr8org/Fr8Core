/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.unit.directives.controls {
    import fx = utils.fixtures; // just an alias
    import filePickerEvents = dockyard.Fr8Events.FilePicker;

    var compileTemplate = (localScope, rawTemplate, $compile) => {
        var template = angular.element(rawTemplate);
        var elem = $compile(template)(localScope);
        localScope.$digest();
        return elem;
    };

    describe('Testing FilePicker control', () => {
        var $rootScope: ng.IRootScopeService,
            $compile: ng.ICompileService,
            $timeout: ng.ITimeoutService,
            $q: ng.IQService,
            fileService,
            element,
            scope,
            directive = '<file-picker change="onChange" field="field" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and routes us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {
            
            inject((_$compile_, _$rootScope_, _$timeout_, _$q_, _FileService_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
                $timeout = _$timeout_;
                $q = _$q_;
                fileService = _FileService_;

                scope = $rootScope.$new();
                scope.onChange = jasmine.createSpy("onChange function");
                scope.field = angular.copy(fx.FieldDTO.filePickerField);
                element = compileTemplate(scope, directive, $compile);
                
            });
        });

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

        it('Should upload file on selection', () => {
            var deferred = $q.defer();
            spyOn(fileService, 'uploadFile').and.returnValue(deferred.promise);
            element.isolateScope().OnFileSelect({});
            expect(fileService.uploadFile).toHaveBeenCalled();
        });

        it('Should broadcast and call onChange on upload success', () => {
            var deferred = $q.defer();
            spyOn(fileService, 'uploadFile').and.returnValue(deferred.promise);
            spyOn(scope.$root, '$broadcast');

            element.isolateScope().OnFileSelect({});
            var uploadedFile: interfaces.IFileDescriptionDTO = {
                id: 123456789,
                originalFileName: 'testFile.xls'
            };
            deferred.resolve(uploadedFile);
            (<dockyard.model.FileDTO>uploadedFile).cloudStorageUrl = 'testStorageURL';
            //resolve promise
            scope.$digest();
            expect(scope.$root.$broadcast.calls.count()).toBe(1);
            expect(scope.onChange.calls.count()).toBe(1);
            expect(scope.$root.$broadcast.calls.argsFor(0)).toEqual([<any>filePickerEvents.FP_SUCCESS, uploadedFile]);
        });


        it('Should set selected file on upload success', () => {
            expect(element.isolateScope().selectedFile).toEqual(null);
            var deferred = $q.defer();
            spyOn(fileService, 'uploadFile').and.returnValue(deferred.promise);
            spyOn(scope.$root, '$broadcast');

            element.isolateScope().OnFileSelect({});
            var uploadedFile: interfaces.IFileDescriptionDTO = {
                id: 123456789,
                originalFileName: 'testFile.xls'
            };
            deferred.resolve(uploadedFile);
            (<dockyard.model.FileDTO>uploadedFile).cloudStorageUrl = 'testStorageURL';
            //resolve promise
            scope.$digest();
            expect(element.isolateScope().selectedFile).toEqual(uploadedFile);
            expect(element.isolateScope().field.value).toEqual('testStorageURL');
        });
    });

    ///MULTI-USAGE TESTS

    describe('Testing FilePicker multi usage', () => {

        var $rootScope,
            $compile,
            $timeout,
            $q,
            fileService,
            element1,
            element2,
            scope,
            directive1 = '<file-picker field="field1" change="onChange" />',
            directive2 = '<file-picker field="field2" change="onChange" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and routes us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {

            inject((_$compile_, _$rootScope_, _$timeout_, _$q_, _FileService_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
                $timeout = _$timeout_;
                $q = _$q_;
                fileService = _FileService_;

                scope = $rootScope.$new();
                scope.onChange = jasmine.createSpy("onChange function");
                scope.field1 = angular.copy(fx.FieldDTO.filePickerField);
                scope.field2 = angular.copy(fx.FieldDTO.filePickerField);
                element1 = compileTemplate(scope, directive1, $compile);
                element2 = compileTemplate(scope, directive2, $compile);

            });
        });

        it('Should update it\'s value but not any different control', () => {
            var deferred = $q.defer();
            spyOn(fileService, 'uploadFile').and.returnValue(deferred.promise);
            spyOn(scope.$root, '$broadcast');

            element1.isolateScope().OnFileSelect({});
            var uploadedFile: interfaces.IFileDescriptionDTO = {
                id: 123456789,
                originalFileName: 'testFile.xls'
            };
            deferred.resolve(uploadedFile);
            (<dockyard.model.FileDTO>uploadedFile).cloudStorageUrl = 'testStorageURL';
            //resolve promise
            scope.$digest();
            expect(element1.isolateScope().selectedFile).toEqual(uploadedFile);
            expect(element1.isolateScope().field.value).toEqual('testStorageURL');

            expect(element2.isolateScope().selectedFile).toEqual(null);
            expect(element2.isolateScope().field.value).toEqual(null);
        });
    });
}