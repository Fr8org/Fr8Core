/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    import psa = dockyard.directives.paneSelectAction;
    import m = dockyard.model;
    import designHeaderEvents = dockyard.Fr8Events.DesignerHeader;

    export interface IActionPickerScope extends ng.IScope {
        designerHeaderEl: any,
        containerEl: any,
        panelCallback: {
            reload?: () => void
        },
        visible: boolean,
        planIsRunning: boolean,
        onAddActivity: () => void;
        close: () => void;
    }

    export function ActionPicker($compile: ng.ICompileService): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActionPicker',
            link: (scope: IActionPickerScope, element: any, attr: any) => {
                var containerEl = $('<div class="action-picker-container action-picker-container-hidden"><action-picker-panel on-close="close()" callback="panelCallback" action-group="group" /></div>');
                containerEl.insertAfter($('designer-header')); 
                scope.designerHeaderEl = $('designer-header');
                scope.containerEl = containerEl;
                var planState = attr.planState;
                if (planState === model.PlanState.Active || planState === model.PlanState.Executing) {
                    scope.planIsRunning = true;
                }
                var childScope = scope.$new(false, scope);
                $compile(containerEl.contents())(childScope);

                $(document).bind('click', (event) => {
                    if (!scope.visible) {
                        return;
                    }

                    var isClickedElementChildOfPopup = containerEl[0] === event.target || containerEl.find(event.target).length > 0;
    
                    if (isClickedElementChildOfPopup)
                        return;

                    scope.close();
                });
            },
            controller: ['$scope', '$timeout',
                ($scope: IActionPickerScope, $timeout: ng.ITimeoutService) => {
                    $scope.panelCallback = {};

                    $scope.$on('$destroy', () => {
                        $scope.containerEl.remove();
                    });

                    $scope.$on(<any>designHeaderEvents.PLAN_EXECUTION_STARTED,
                        (event: ng.IAngularEvent) => {
                            $scope.planIsRunning = true;
                        });

                    $scope.$on(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED,
                        (event: ng.IAngularEvent) => {
                            $scope.planIsRunning = false;
                        });

                    $scope.onAddActivity = () => {
                        if ($scope.visible) {
                            return;
                        }

                        $scope.containerEl.removeClass('action-picker-container-hidden');
                        $scope.containerEl.width($scope.designerHeaderEl.width());

                        $scope.panelCallback.reload(); 

                        $timeout(() => {
                            $scope.visible = true;
                        }, 200);
                    };

                    $scope.close = () => {
                        $scope.containerEl.addClass('action-picker-container-hidden');
                        $scope.visible = false;
                    };
                }
            ]
        }
    }


    export interface IActionPickerPanelScope extends ng.IScope {
        categories: Array<interfaces.IActivityCategoryDTO>;
        selectedCategory: model.ActivityCategoryDTO;
        actionGroup: model.ActionGroup;
        form: any;

        selectCategory: (category: model.ActivityCategoryDTO) => void;
        unselectCategory: () => void;
        selectActivityTemplate: (at: interfaces.IActivityTemplateVM) => void;
        
        onClose: () => void;
        callback: {
            reload: () => void
        }
    }


    export function ActionPickerPanel(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActionPickerPanel',
            controller: ['$scope', 'ActivityTemplateHelperService', '$timeout',
                ($scope: IActionPickerPanelScope, ActivityTemplateHelperService: services.IActivityTemplateHelperService, $timeout: ng.ITimeoutService) => {
                    $scope.form = { searchText: '' };

                    $scope.selectCategory = (category: model.ActivityCategoryDTO) => {
                        $timeout(() => {
                            $scope.selectedCategory = category;
                        }, 10);
                    };

                    $scope.unselectCategory = () => {
                        $timeout(() => {
                            $scope.selectedCategory = null;
                        }, 10);
                    }; 

                    $scope.selectActivityTemplate = (at: interfaces.IActivityTemplateVM) => {
                        var eventArgs = new psa.ActivityTypeSelectedEventArgs(at, $scope.actionGroup);
                        $scope.$emit(psa.MessageType[psa.MessageType.PaneSelectAction_ActivityTypeSelected], eventArgs);
                        $scope.onClose();
                    };

                    $scope.callback.reload = () => {
                        $scope.unselectCategory();
                        $scope.form.searchText = '';
                    };

                    var _reload = () => {
                        ActivityTemplateHelperService.getAvailableActivityTemplatesByCategory()
                            .then((res: Array<interfaces.IActivityCategoryDTO>) => {
                                $scope.categories = res;
                            });
                    };

                    _reload();
                }
            ],
            scope: {
                'onClose': '&',
                'callback': '=',
                'actionGroup': '='
            }
        }
    }
}

app.directive('actionPicker', ['$compile', dockyard.directives.ActionPicker]);
app.directive('actionPickerPanel', [dockyard.directives.ActionPickerPanel]);