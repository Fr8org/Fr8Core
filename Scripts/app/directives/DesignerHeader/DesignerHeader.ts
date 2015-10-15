/// <reference path="../../_all.ts" />

module dockyard.directives.designerHeader {
    'use strict';

    export interface IDesignerHeaderScope extends ng.IScope {
        onStateChange(): void;
        processTemplate: model.ProcessTemplateDTO
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class DesignerHeader implements ng.IDirective {
        public link: (scope: IDesignerHeaderScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IDesignerHeaderScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;

        public templateUrl = '/AngularTemplate/DesignerHeader';
        public scope = {
            processTemplate: '='
        };
        public restrict = 'E';

        constructor(private ProcessTemplateService: services.IProcessTemplateService) {
            DesignerHeader.prototype.link = (
                scope: IDesignerHeaderScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            DesignerHeader.prototype.controller = (
                $scope: IDesignerHeaderScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                $scope.onStateChange = () => {
                    if ($scope.processTemplate.processTemplateState === model.ProcessState.Inactive) {
                        ProcessTemplateService.activate($scope.processTemplate);
                    } else {
                        ProcessTemplateService.deactivate($scope.processTemplate);
                    }
                };
            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (ProcessTemplateService: services.IProcessTemplateService) => {
                return new DesignerHeader(ProcessTemplateService);
            };

            directive['$inject'] = ['ProcessTemplateService'];
            return directive;
        }
    }

    app.directive('designerHeader', DesignerHeader.Factory());
}