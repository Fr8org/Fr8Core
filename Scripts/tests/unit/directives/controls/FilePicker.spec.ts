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

    describe('Testing FilePicker control', () => {
        var $rootScope,
            $compile,
            $timeout,
            element,
            scope,
            directive = '<file-picker field="field" />';

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
                scope.currentAction = fx.ActionDesignDTO.noAuthActionVM;
                scope.field = fx.ActionDesignDTO.filePickerField;
                element = compileTemplate(scope, directive, $compile);
                
            });
        });

        it('Should have an isolateScope', () => {
            expect(element.isolateScope()).not.toBe(null);
        });

    });

    ///MULTI-USAGE TESTS

    //describe('Testing TextBox multi usage', () => {

    //    var $rootScope,
    //        $compile,
    //        $timeout,
    //        element1,
    //        element2,
    //        scope,
    //        directive1 = '<configuration-control current-action="currentAction" field="field1" />',
    //        directive2 = '<configuration-control current-action="currentAction" field="field2" />';

    //    beforeEach(module('app', 'templates'));

    //    app.run(['$httpBackend', (_$httpBackend_) => {
    //        //we need this because stateProvider loads on test startup and routes us to default state 
    //        //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
    //        _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
    //    }]);

    //    beforeEach(() => {

    //        inject((_$compile_, _$rootScope_, _$timeout_) => {
    //            $rootScope = _$rootScope_;
    //            $compile = _$compile_;
    //            $timeout = _$timeout_;

    //            scope = $rootScope.$new();
    //            scope.currentAction = fx.ActionDesignDTO.noAuthActionVM;
    //            scope.field1 = angular.copy(fx.ActionDesignDTO.textField);
    //            scope.field2 = angular.copy(fx.ActionDesignDTO.textField);
    //            element1 = compileTemplate(scope, directive1, $compile);
    //            element2 = compileTemplate(scope, directive2, $compile);

    //        });
    //    });

    //    it('Should update it\'s value but not any different control', () => {
    //        changeText(scope, element1, 'super-complex-test-value'); 
    //        expect(element1.isolateScope().field.value).toBe('super-complex-test-value');
    //        expect(element2.isolateScope().field.value).toBe(fx.ActionDesignDTO.textField.value);
    //    });

    //    it('Should call only own change function', () => {
    //        element1.isolateScope().onChange = jasmine.createSpy("onChange function");
    //        element2.isolateScope().onChange = jasmine.createSpy("onChange function");
    //        changeText(scope, element1, 'super-complex-test-value');
    //        expect(element1.isolateScope().onChange).toHaveBeenCalled();
    //        expect(element2.isolateScope().onChange).not.toHaveBeenCalled();
    //    });
    //});
}