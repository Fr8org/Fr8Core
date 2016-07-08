/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    export interface IActionPickerScope extends ng.IScope {
        designerHeaderEl: any,
        containerEl: any,
        visible: boolean,
        onAddActivity: () => void;
        close: () => void;
    }

    export function ActionPicker($compile: ng.ICompileService): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActionPicker',
            link: (scope: IActionPickerScope, element: any, attr: any) => {
                var containerEl = $('<div class="action-picker-container action-picker-container-hidden"><action-picker-panel on-close="close()" /></div>');
                containerEl.insertAfter($('designer-header')); 

                scope.designerHeaderEl = $('designer-header');
                scope.containerEl = containerEl;

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
                    $scope.$on('$destroy', () => {
                        $scope.containerEl.remove();
                    });

                    $scope.onAddActivity = () => {
                        if ($scope.visible) {
                            return;
                        }

                        $scope.containerEl.removeClass('action-picker-container-hidden');
                        $scope.containerEl.width($scope.designerHeaderEl.width());

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
        categories: Array<model.ActivityCategoryDTO>;
        onClose: () => void;
    }


    export function ActionPickerPanel(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActionPickerPanel',
            controller: ['$scope', '$http',
                ($scope: IActionPickerPanelScope, $http: ng.IHttpService) => {
                    var _reload = () => {
                        $http.get('/api/activity_templates/by_categories')
                            .then((res: ng.IHttpPromiseCallbackArg<Array<model.ActivityCategoryDTO>>) => {
                                $scope.categories = res.data;
                            });
                    };

                    _reload();
                }
            ],
            scope: {
                'onClose': '&'
            }
        }
    }

    // TODO: FR-3969, remove this.
    // import psa = dockyard.directives.paneSelectAction;
    // export function ActionPicker(): ng.IDirective {
    //     return {
    //         restrict: 'E',
    //         templateUrl: '/AngularTemplate/ActionPicker',
    //         link: (scope: IActionPickerScope, element: any, attr: any) => {
    //             $(document).bind('click', (event) => {
    //                 var isClickedElementChildOfPopup = element
    //                     .find(event.target)
    //                     .length > 0;
    // 
    //                 if (isClickedElementChildOfPopup)
    //                     return;
    // 
    //                 scope.$apply(() => {
    //                     scope.activeCategory = null;
    //                     scope.activeTerminal = null;
    //                 });
    //             });
    //         },
    //         controller: ['$scope', '$element', 'WebServiceService', '$timeout',
    //             ($scope: IActionPickerScope, $element: ng.IRootElementService, webServiceService: services.IWebServiceService, $timeout: ng.ITimeoutService) => {
    // 
    //                 $scope.actionCategories = [
    //                     { id: 1, name: "Monitor", description: "Learn when something happen", icon: "eye" },
    //                     { id: 2, name: "Get", description: "In-process Crates from a web service", icon: "download" },
    //                     { id: 3, name: "Process", description: "Carry out work on a Container", icon: "recycle" },
    //                     { id: 4, name: "Forward", description: "Send Crates to a web service", icon: "share" }];
    // 
    //                 $scope.activeCategory = null;
    //                 $scope.activeTerminal = null;
    // 
    //                 // when new activity picker is opened, this method provide it to be shown in the viewport, 
    //                 // by scrolling to the new opened activity picker element 
    //                 var scrollToActivityPicker = () => {
    //                     // method is in the timeout since activity picker appears screen animated, in 500 ms
    //                     $timeout(() => {
    //                         var scrollToElement = $element.find('.action-slider'); // element to be scrolled on
    //                         var leftPositionOfElement = scrollToElement.position().left;
    //                         var leftPositionOfContainer = parseInt(scrollToElement.closest('.action-group').css('left'), 10);
    //                         var windowSize = $(window).width(); // substracted from total width since we want activity to be shown center of the screen
    // 
    //                         $element.closest('.sub-plan-container').animate({
    //                             scrollLeft: leftPositionOfElement + leftPositionOfContainer - (windowSize / 2)
    //                         }, 100);
    //                     }, 500);
    //                 };
    // 
    //                 $scope.setActive = (actionCategoryId) => {
    // 
    //                     if ($scope.activeCategory === actionCategoryId) {
    //                         $scope.activeCategory = null;
    //                     } else {
    //                         $scope.activeCategory = actionCategoryId;
    //                     }
    //                     $scope.webServiceActionList = webServiceService.getActivities({ id: $scope.activeCategory });
    //                     $scope.activeTerminal = null;
    //                     scrollToActivityPicker();
    //                 };
    // 
    //                 $scope.setActiveAction = (action, group) => {
    //                     $scope.activeCategory = null;
    // 
    //                     if (group == undefined) {
    //                         group = null;
    //                     }
    // 
    //                     var eventArgs = new psa.ActivityTypeSelectedEventArgs(action, group);
    //                     $scope.$emit(psa.MessageType[psa.MessageType.PaneSelectAction_ActivityTypeSelected], eventArgs);
    // 
    //                 };
    // 
    //                 $scope.deactivateTerminal = () => {
    //                     $scope.activeTerminal = null;
    //                 };
    // 
    //                 $scope.setActiveTerminal = (terminal) => {
    //                     $scope.activeTerminal = terminal;
    //                 };
    // 
    //                 $scope.sortBuiltinServices = (service) => {
    //                     return (service.webServiceName === 'Built-In Services') ? -1 : 1;
    //                 };
    // 
    //                 $scope.onAddActivity = () => {
    //                     alert('onAddActivity');
    //                 };
    //             }
    //         ]
    //     }
    // }
    // 
    // export interface IActionPickerScope extends ng.IScope {
    //     webServiceActionList: Array<model.WebServiceActionSetDTO>;
    //     actionCategories: any;
    //     activeCategory: any;
    //     activeTerminal: model.WebServiceActionSetDTO;
    //     setActive: (actionCategoryId: any) => void;
    //     setActiveTerminal: (terminal: model.WebServiceActionSetDTO) => void;
    //     deactivateTerminal: () => void;
    //     setActiveAction: (action: any, group: any) => void;
    //     sortBuiltinServices: (service: model.WebServiceActionSetDTO) => number;
    // 
    //     onAddActivity: () => void;
    // }
}

app.directive('actionPicker', ['$compile', dockyard.directives.ActionPicker]);
app.directive('actionPickerPanel', [dockyard.directives.ActionPickerPanel]);