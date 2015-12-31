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

    describe('UpstreamFieldChooser Control', () => {
        var $rootScope,
            $compile,
            $element,
            scope,
            directive = '<upstream-field-chooser field="field" current-action="action" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and routes us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        beforeEach(() => {
            inject((_$compile_, _$rootScope_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;

                scope = $rootScope.$new();
                scope.field = $.extend(true, {}, fx.UpstreamDataChooser.sampleField);
                scope.action = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                $element = compileTemplate(scope, directive, $compile);
            });
        });


        /* SPECS */

        it('should be compiled into a table', () => {
            expect($element.find('table').length === 1).toBe(true);
        });

        it('should add a row for each field in the set', () => {
            var $rows = $element.find('tbody tr:not(.ng-table-group)');
            var crate = CH.findByLabel(fx.UpstreamDataChooser.sampleAction.crateStorage, 'Upstream Terminal-Provided Fields');
            var fields = (<any>crate.contents).Fields;

            expect(fields.length).toBe($rows.length);

            $rows.each(function () {
                var $row = $(this);
                var html = $row.html();
                var found = false;
                for (var i = 0; i < fields.length; i++) {
                    if (html.indexOf(fields[i].key) !== -1 && html.indexOf(fields[i].sourceCrateManifest.Type) !== -1) {
                        found = true;
                        break;
                    }
                }
                expect(found).toBe(true);
            });
        });

        it('should set the value of the control field to the clicked field row', () => {
            var $rows = $element.find('tbody tr:not(.ng-table-group)');
            var $toSelect = $rows.eq(2);
            var name = $toSelect.find('td').eq(0).text();

            $toSelect.click();
            scope.$digest();

            expect(scope.field.value).toBe(name);
        });

        it('should apply the current field value to the table selection', () => {
            var $rows = $element.find('tbody tr:not(.ng-table-group)');
            var $selected = $rows.filter('.active');

            expect($selected.length > 0).toBe(true);
            expect($selected.find('td').eq(0).text()).toBe(scope.field.value);
        });

        describe('multiple controls', () => {

            var directive1 = '<upstream-field-chooser field="field" current-action="action"></upstream-field-chooser><upstream-field-chooser field="field1" current-action="action1"></upstream-field-chooser>';

            beforeEach(() => {
                scope.field = $.extend(true, {}, fx.UpstreamDataChooser.sampleField);
                scope.action = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                scope.field1 = $.extend(true, {}, fx.UpstreamDataChooser.fieldWithValues);
                scope.action1 = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                $element = compileTemplate(scope, directive1, $compile);
            });

            it('should be compiled correctly into multiple tables', () => {
                expect($element.find('table').length === 2).toBe(true);
            });

            it('should not change the value of second control if first control value is changed', () => {
                var $rows = $element.find('tbody').eq(0).find('tr:not(.ng-table-group)');
                var $toSelect = $rows.eq(2);
                var currValue = scope.field1.value;

                $toSelect.click();
                scope.$digest();

                expect(scope.field1.value).toBe(currValue);
            });

        });

    });

} 