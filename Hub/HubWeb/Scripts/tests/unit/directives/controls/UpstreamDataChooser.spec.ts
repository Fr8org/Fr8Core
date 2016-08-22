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

    describe('UpstreamDataChooser Control', () => {
        var $rootScope,
            $compile,
            $element,
            $element1,
            scope,
            callbackCallCounter,
            directive = '<upstream-data-chooser field="field" current-action="action" change="changeCallback" />',
            directive1 = '<upstream-data-chooser field="field1" current-action="action1" change="changeCallback" />';

        beforeEach(module('app', 'templates'));

        app.run(['$httpBackend', (_$httpBackend_) => {
            //we need this because stateProvider loads on test startup and routes us to default state 
            //which is myaccount and has template URL with /AngularTemplate/MyAccountPage
            _$httpBackend_.expectGET('/AngularTemplate/MyAccountPage').respond(200, '<div></div>');
        }]);

        function onChange() {
            callbackCallCounter++;
        }

        beforeEach(() => {
            inject((_$compile_, _$rootScope_) => {
                $rootScope = _$rootScope_;
                $compile = _$compile_;
                callbackCallCounter = 0;

                scope = $rootScope.$new();
                scope.field = $.extend(true, {}, fx.UpstreamDataChooser.sampleField);
                scope.action = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                scope.field1 = $.extend(true, {}, fx.UpstreamDataChooser.fieldWithValues);
                scope.action1 = $.extend(true, {}, fx.UpstreamDataChooser.sampleAction);
                $element = compileTemplate(scope, directive, $compile);
                $element1 = compileTemplate(scope, directive1, $compile);

                scope.changeCallback = onChange;
            });
        });


        /* SPECS */

        it('should be compiled into 3 dropdown lists', () => {
            expect($element.find('drop-down-list-box').length).toBe(3);
        });

        it('should set the list of items in manifest drop-down from "Upstream Manifest Type List" crate', () => {
            var crate = CH.findByLabel(fx.UpstreamDataChooser.sampleAction.crateStorage, 'Upstream Manifest Type List');
            var manifestList = (<any>crate.contents).Fields.map(f => f.value);
            var $dropDown = $element.find('drop-down-list-box').eq(0);
            var ddScope = $dropDown.children().scope();
            var ddList = ddScope.field.listItems.map(item => item.key);

            expect(ddList).toEqual(manifestList);
        });

        it('should set the list of items in label drop-down from "Upstream Crate Label List" crate', () => {
            var crate = CH.findByLabel(fx.UpstreamDataChooser.sampleAction.crateStorage, 'Upstream Crate Label List');
            var labelList = (<any>crate.contents).Fields.map(f => f.value);
            var $dropDown = $element.find('drop-down-list-box').eq(1);
            var ddScope = $dropDown.children().scope();
            var ddList = ddScope.field.listItems.map(item => item.key);

            expect(ddList).toEqual(labelList);
        });

        it('should set the list of items in field type drop-down from "Upstream Terminal-Provided Fields" crate', () => {
            var crate = CH.findByLabel(fx.UpstreamDataChooser.sampleAction.crateStorage, 'Upstream Terminal-Provided Fields');
            var tagList = (<any>crate.contents).Fields.map(f => f.tags);
            var filteredList = [];
            tagList.forEach((item) => {
                if (item && filteredList.indexOf(item) === -1) filteredList.push(item);
            });

            var $dropDown = $element.find('drop-down-list-box').eq(2);
            var ddScope = $dropDown.children().scope();
            var ddList = ddScope.field.listItems.map(item => item.key);

            expect(ddList).toEqual(filteredList);
        });

        it('should apply the value of selected manifest', () => {
            var $dropDown1 = $element.find('drop-down-list-box').eq(0);
            var ddScope1 = $dropDown1.children().scope();

            expect(ddScope1.field.selectedKey).toBe(fx.UpstreamDataChooser.sampleField.selectedManifest);

            var $dropDown2 = $element1.find('drop-down-list-box').eq(0);
            var ddScope2 = $dropDown2.children().scope();

            expect(ddScope2.field.selectedKey).toBe(fx.UpstreamDataChooser.fieldWithValues.selectedManifest);
        });

        it('should apply the value of selected label', () => {
            var $dropDown1 = $element.find('drop-down-list-box').eq(1);
            var ddScope1 = $dropDown1.children().scope();

            expect(ddScope1.field.selectedKey).toBe(fx.UpstreamDataChooser.sampleField.selectedLabel);

            var $dropDown2 = $element1.find('drop-down-list-box').eq(1);
            var ddScope2 = $dropDown2.children().scope();

            expect(ddScope2.field.selectedKey).toBe(fx.UpstreamDataChooser.fieldWithValues.selectedLabel);
        });

        it('should apply the value of selected field type', () => {
            var $dropDown1 = $element.find('drop-down-list-box').eq(2);
            var ddScope1 = $dropDown1.children().scope();

            expect(ddScope1.field.selectedKey).toBe(fx.UpstreamDataChooser.sampleField.selectedFieldType);

            var $dropDown2 = $element1.find('drop-down-list-box').eq(2);
            var ddScope2 = $dropDown2.children().scope();

            expect(ddScope2.field.selectedKey).toBe(fx.UpstreamDataChooser.fieldWithValues.selectedFieldType);
        });

        it('should render the error message if no items are available', () => {
            var scope = $rootScope.$new();
            scope.field = $.extend(true, {}, fx.UpstreamDataChooser.sampleField);
            scope.action = $.extend(true, {}, fx.UpstreamDataChooser.actionWithoutListCrates);
            var $element = compileTemplate(scope, directive, $compile);

            expect($element.find('drop-down-list-box').length).toBe(0);
        });

        describe('multiple usage', () => {
            var mElement;

            beforeEach(() => {
                var mDirective = '<div><upstream-data-chooser field="field" current-action="action" change="changeCallback"></upstream-data-chooser><upstream-data-chooser field="field1" current-action="action1" change="changeCallback"></upstream-data-chooser></div>';
                mElement = compileTemplate(scope, mDirective, $compile);
            });

            it('should compile into two controls', () => {
                expect(mElement.find('upstream-data-chooser').length).toBe(2);
                expect(mElement.find('drop-down-list-box').length).toBe(6);
            });

            it('should apply values of correct scope to correct control', () => {
                var udcScope1 = mElement.find('upstream-data-chooser').eq(0).children().scope();
                var udcScope2 = mElement.find('upstream-data-chooser').eq(1).children().scope();
                expect(udcScope1.field.selectedManifest).toBe(fx.UpstreamDataChooser.sampleField.selectedManifest);
                expect(udcScope1.field.selectedLabel).toBe(fx.UpstreamDataChooser.sampleField.selectedLabel);
                expect(udcScope1.field.selectedFieldType).toBe(fx.UpstreamDataChooser.sampleField.selectedFieldType);

                expect(udcScope2.field.selectedManifest).toBe(fx.UpstreamDataChooser.fieldWithValues.selectedManifest);
                expect(udcScope2.field.selectedLabel).toBe(fx.UpstreamDataChooser.fieldWithValues.selectedLabel);
                expect(udcScope2.field.selectedFieldType).toBe(fx.UpstreamDataChooser.fieldWithValues.selectedFieldType);
            });

            it('should not change the other scopes when changing this one', () => {
                var udcScope1 = mElement.find('upstream-data-chooser').eq(0).children().scope();
                var udcScope2 = mElement.find('upstream-data-chooser').eq(1).children().scope();
                var curValue = udcScope2.field.selectedFieldType;

                udcScope1.field.selectedFieldType = 'EmailAddress';
                udcScope1.$digest();
                expect(udcScope2.field.selectedFieldType).toBe(curValue);
            });

        });

    });
} 