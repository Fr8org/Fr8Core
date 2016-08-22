/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamDataChooser {
    'use strict';

    export interface IUpstreamDataChooser extends ng.IScope {
        field: model.UpstreamDataChooser;
        change: () => (field: model.ControlDefinitionDTO) => void;
        onChange: (field: model.ControlDefinitionDTO) => void;
        manifestList: model.DropDownList,
        labelList: model.DropDownList,
        fieldTypeList: model.DropDownList,
        currentAction: model.ActivityDTO;
        isDisabled: boolean;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class UpstreamDataChooser implements ng.IDirective {
        public link: (scope: IUpstreamDataChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IUpstreamDataChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;

        public templateUrl = '/AngularTemplate/UpstreamDataChooser';
        public restrict = 'E';
        public scope = {
            field: '=',
            currentAction: '=',
            change: '&',
            isDisabled:'='
        }

        constructor(CrateHelper: services.CrateHelper) {
            UpstreamDataChooser.prototype.link = (
                scope: IUpstreamDataChooser,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

            }

            UpstreamDataChooser.prototype.controller = (
                $scope: IUpstreamDataChooser,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                var manifestTypeListCrate = CrateHelper.findByManifestTypeAndLabel($scope.currentAction.crateStorage, 'Standard Design-Time Fields', 'Upstream Manifest Type List') || { contents: { Fields: [] } };
                var labelListCrate = CrateHelper.findByManifestTypeAndLabel($scope.currentAction.crateStorage, 'Standard Design-Time Fields', 'Upstream Crate Label List') || { contents: { Fields: [] } };

                $scope.manifestList = <model.DropDownList>{
                    listItems: (<any>manifestTypeListCrate.contents).Fields.map((field) => { return { key: field.value, value: null }; }), selectedKey: $scope.field.selectedManifest
                };
                $scope.labelList = <model.DropDownList>{
                    listItems: (<any>labelListCrate.contents).Fields.map((field) => { return { key: field.value }; }), selectedKey: $scope.field.selectedLabel
                };
                $scope.fieldTypeList = <model.DropDownList>{
                    listItems: CrateHelper.getAvailableFieldTypes($scope.currentAction.crateStorage).map((value) => { return { key: value }; }), selectedKey: $scope.field.selectedFieldType
                };

                $scope.onChange = (field: model.ControlDefinitionDTO) => {
                    $scope.field.selectedManifest = $scope.manifestList.selectedKey;
                    $scope.field.selectedLabel = $scope.labelList.selectedKey;
                    $scope.field.selectedFieldType = $scope.fieldTypeList.selectedKey;

                    if ($scope.change) $scope.change()(field);
                }

            }

            UpstreamDataChooser.prototype.controller['$inject'] = ['$scope', '$element', '$attrs'];

        };

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (CrateHelper: services.CrateHelper) => {
                return new UpstreamDataChooser(CrateHelper);
            };

            directive['$inject'] = ['CrateHelper'];
            return directive;
        }
    }

    app.directive('upstreamDataChooser', UpstreamDataChooser.Factory());
} 