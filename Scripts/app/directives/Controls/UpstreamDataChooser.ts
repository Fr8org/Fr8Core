/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamDataChooser {
    'use strict';

    export interface IUpstreamDataChooser extends ng.IScope {
        field: model.UpstreamDataChooser;
        onChange: () => void;
        manifestList: {
            listItems: string[];
        },
        labelList: {
            listItems: string[];
        },
        fieldTypeList: {
            listItems: any;
        },
        currentAction: model.ActionDTO;
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
            currentAction: '='
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

                var manifestTypeListCrate = CrateHelper.findByManifestTypeAndLabel($scope.currentAction.crateStorage, 'Standard Design-Time Fields', 'Upstream Manifest Type List');
                var labelListCrate = CrateHelper.findByManifestTypeAndLabel($scope.currentAction.crateStorage, 'Standard Design-Time Fields', 'Upstream Crate Label List');

                $scope.manifestList = {
                    listItems: (<any>manifestTypeListCrate.contents).Fields.map((field) => { return { key: field.value }; })
                };
                $scope.labelList = { listItems: (<any>labelListCrate.contents).Fields.map((field) => { return { key: field.value }; }) };

                $scope.fieldTypeList = {
                    listItems: CrateHelper.getAvailableFieldTypes($scope.currentAction.crateStorage).map((value) => { return { key: value }; })
                };


                $scope.onChange = () => {

                }
            }

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