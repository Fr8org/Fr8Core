namespace dockyard.directives {
    'use strict';
    
    export class Fr8CollapseCtrl {
        isCollapsed: boolean = true;
        toggle(): void {
            this.isCollapsed = !this.isCollapsed;
        };
    }
    
    export interface IFr8CollapseScope extends ng.IScope {
        ctrl: Fr8CollapseCtrl;
        domEl: ng.IAugmentedJQuery;
    }
    
    export function collapseHeading(): ng.IDirective {
        return {
            controller: Fr8CollapseCtrl,
            controllerAs: 'fr8CollapseCtrl'
        }
    }

    export function collapseContent($compile): ng.IDirective {
        return {
            require: '^^fr8CollapseHeading',
            scope: {},
            transclude: true,
            link: (scope: IFr8CollapseScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes,
                ctrl: Fr8CollapseCtrl,
                transclude: ng.ITranscludeFunction): void => {
                let template = `
                    <md-list-item class="md-2-line fr8-collapse-content" ng-class="{'fr8-collapse-collapsed': ctrl.isCollapsed}">
                        <div class="md-list-item-text" layout="column"></div>
                        <md-divider ng-hide="ctrl.isCollapsed"></md-divider>
                    </md-list-item>`;
                scope.ctrl = ctrl;
                scope.domEl = $compile(template)(scope);
                
                // Replace "height: auto" with computed height to be able to animate actual height instead of max-height
                scope.$watch('ctrl.isCollapsed', (newValue, oldValue) => {
                    if (newValue && !oldValue) {
                        scope.domEl.css('height', scope.domEl.height() + 'px');
                    } else {
                        scope.domEl.css('height', 'auto');
                    }
                });
                transclude((clone) => {
                    scope.domEl.children('.md-list-item-text').append(clone);
                    element.parents('md-list-item').after(scope.domEl);
                    element.before($compile('<md-icon md-font-icon="fa-chevron-circle-down" class="fa fa-lg md-secondary fr8-collapse-marker"></md-icon>')(scope))
                });
            }
        }
    }
    collapseContent.$inject = ['$compile'];
}
