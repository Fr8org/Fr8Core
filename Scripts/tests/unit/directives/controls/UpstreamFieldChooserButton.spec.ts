/// <reference path="../../../../app/_all.ts" />
/// <reference path="../../../../typings/angularjs/angular-mocks.d.ts" />
module dockyard.tests.unit.directives.controls {

    import fx = utils.fixtures;
    import CrateHelper = dockyard.services.CrateHelper;
    import filterByTagFactory = dockyard.filters.filterByTag.factory;

    var CH = new CrateHelper(filterByTagFactory);

    var compileTemplate = (localScope, rawTemplate, $compile) => {
        var template = angular.element(rawTemplate);
        var elem = $compile(template)(localScope);
        localScope.$digest();
        return elem;
    };

    describe('UpstreamFieldChooserButton Control', () => {
        var $rootScope,
            $compile,
            elem,
            elem1,
            element,
            scope,
            scope1,
            directive = '<upstream-field-chooser-button field="field" current-action="action" change="onChange"></upstream-field-chooser-button>',
            controller,
            modal,
            $httpBackend;

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and routes us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {
            inject((_$compile_, _$rootScope_, $controller, $modal, $injector, $timeout, NgTableParams) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
                scope = $rootScope.$new();
                scope.field = $.extend(true, {}, fx.UpstreamDataChooser.sampleField);
                scope.field.listItems = fx.UpstreamFieldChooser.incomingFields;
                scope.action = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                scope.currentAction = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                

                var element = angular.element('<upstream-field-chooser-button field="field" current-action="action" change="onChange"></upstream-field-chooser-button>');
                elem = $compile(element)(scope);
                scope = elem.isolateScope();
                scope.$digest();
                scope.change = null;
                var openModal = jasmine.createSpyObj('openModal', ['close']);
                modal = jasmine.createSpyObj('modal', ['show', 'hide', 'open']);
                modal.open.and.callFake(function () {
                    return openModal;
                });
                var modalConst = jasmine.createSpy('modal').and.callFake(function () { return modal; });
                controller = $controller('UpstreamFieldChooserButtonController', { $scope: scope, $element: elem, $attrs: null, $modal: modalConst(), $timeout: $timeout, NgTableParams: NgTableParams });

                
            });
        });


        /* SPECS */

        it('should be compiled into a button', () => {
            expect(elem.find('button').length === 1).toBe(true);
        });

        it('should open modal', () => {
            scope.createModal();
            expect(modal.open).toHaveBeenCalled();
        });

        it('should set new upstream field', () => {
            scope.createModal();
            scope.selectItem(scope.field.listItems[0]);
            expect(scope.field.value).toBe(scope.field.listItems[0].key);
        });

        describe('multiple controls', () => {

            var directive1 = '<upstream-field-chooser-button field="field" current-action="action"></upstream-field-chooser><upstream-field-chooser field="field" current-action="action"></upstream-field-chooser-button>';

            beforeEach(() => {
                scope1 = $rootScope.$new();
                scope1.field = $.extend(true, {}, fx.UpstreamDataChooser.fieldWithValues);
                scope1.field.listItems = fx.UpstreamFieldChooser.incomingFields;
                scope1.action = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                scope1.currentAction = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                elem1 = $compile(directive1)(scope1);
                scope1 = elem1.isolateScope();
                scope1.$digest();
                scope1.change = null;
            });

            it('should be compiled correctly into multiple buttons', () => {
                console.log(elem1.find('button').length);
                expect(elem1.find('button').length === 1).toBe(true);
            });

            it('should not change the value of second control if first control value is changed', () => {
                scope.createModal();
                scope.selectItem(scope.field.listItems[0]);
                expect(scope.field.value).toBe(scope.field.listItems[0].key);
                scope1.createModal();
                scope1.selectItem(scope1.field.listItems[1]);
                expect(scope.field.value).toBe(scope.field.listItems[0].key);
                expect(scope1.field.value).toBe(scope1.field.listItems[1].key);
            });

        });

    });

} 