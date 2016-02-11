/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamDataChooser {
    'use strict';

    export interface IUpstreamFieldChooser extends ng.IScope {
        field: model.UpstreamDataChooser;
        currentAction: model.ActivityDTO;
        upstreamFields: model.FieldDTO[];
        tableParams: any;
        selectField: (field: model.FieldDTO) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class UpstreamFieldChooser implements ng.IDirective {
        public link: (scope: IUpstreamFieldChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IUpstreamFieldChooser, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;

        public templateUrl = '/AngularTemplate/UpstreamFieldChooser';
        public restrict = 'E';
        public scope = {
            field: '=',
            currentAction: '='
        }

        private CrateHelper: services.CrateHelper;

        constructor(CrateHelper: services.CrateHelper, NgTableParams) {
            this.CrateHelper = CrateHelper;

            UpstreamFieldChooser.prototype.link = (
                scope: IUpstreamFieldChooser,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

            }

            UpstreamFieldChooser.prototype.controller = (
                $scope: IUpstreamFieldChooser,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                $scope.upstreamFields = (<any>CrateHelper.findByLabel($scope.currentAction.crateStorage, 'Upstream Terminal-Provided Fields').contents).Fields;

                $scope.tableParams = new NgTableParams({ count: 50 }, { data: $scope.upstreamFields, counts: [], groupBy: 'sourceCrateLabel', groupOptions: { isExpanded: false } });

                $scope.selectField = (field) => {
                    $scope.field.value = field.key;
                };
            };

            UpstreamFieldChooser.prototype.controller['$inject'] = ['$scope', '$element', '$attrs'];

        };


        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (CrateHelper: services.CrateHelper, NgTableParams) => {
                return new UpstreamFieldChooser(CrateHelper, NgTableParams);
            };

            directive['$inject'] = ['CrateHelper', 'NgTableParams'];
            return directive;
        }
    }

    app.directive('upstreamFieldChooser', UpstreamFieldChooser.Factory());
} 