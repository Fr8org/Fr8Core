/// <reference path="../../_all.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectAction;
        (function (paneSelectAction) {
            'use strict';
            (function (MessageType) {
                MessageType[MessageType["PaneSelectAction_ActionUpdated"] = 0] = "PaneSelectAction_ActionUpdated";
                MessageType[MessageType["PaneSelectAction_Render"] = 1] = "PaneSelectAction_Render";
                MessageType[MessageType["PaneSelectAction_Hide"] = 2] = "PaneSelectAction_Hide";
                MessageType[MessageType["PaneSelectAction_UpdateAction"] = 3] = "PaneSelectAction_UpdateAction";
                MessageType[MessageType["PaneSelectAction_ActionTypeSelected"] = 4] = "PaneSelectAction_ActionTypeSelected";
                MessageType[MessageType["PaneSelectAction_InitiateSaveAction"] = 5] = "PaneSelectAction_InitiateSaveAction";
            })(paneSelectAction.MessageType || (paneSelectAction.MessageType = {}));
            var MessageType = paneSelectAction.MessageType;
            var ActionTypeSelectedEventArgs = (function () {
                function ActionTypeSelectedEventArgs(action) {
                    // Clone Action to prevent any issues due to possible mutation of source object
                    this.action = angular.extend({}, action);
                }
                return ActionTypeSelectedEventArgs;
            })();
            paneSelectAction.ActionTypeSelectedEventArgs = ActionTypeSelectedEventArgs;
            var ActionUpdatedEventArgs = (function (_super) {
                __extends(ActionUpdatedEventArgs, _super);
                function ActionUpdatedEventArgs(criteriaId, actionId, isTempId, actionName) {
                    _super.call(this, criteriaId, actionId);
                    this.isTempId = isTempId;
                    this.actionName = actionName;
                }
                return ActionUpdatedEventArgs;
            })(directives.ActionEventArgsBase);
            paneSelectAction.ActionUpdatedEventArgs = ActionUpdatedEventArgs;
            var RenderEventArgs = (function () {
                function RenderEventArgs(processNodeTemplateId, id, isTemp, actionListId) {
                    this.processNodeTemplateId = processNodeTemplateId;
                    this.id = id;
                    this.isTempId = isTemp;
                    this.actionListId = actionListId;
                }
                return RenderEventArgs;
            })();
            paneSelectAction.RenderEventArgs = RenderEventArgs;
            var UpdateActionEventArgs = (function (_super) {
                __extends(UpdateActionEventArgs, _super);
                function UpdateActionEventArgs(criteriaId, actionId, isTempId) {
                    _super.call(this, criteriaId, actionId);
                    this.isTempId = isTempId;
                }
                return UpdateActionEventArgs;
            })(directives.ActionEventArgsBase);
            paneSelectAction.UpdateActionEventArgs = UpdateActionEventArgs;
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var PaneSelectAction = (function () {
                function PaneSelectAction($rootScope, ActionService) {
                    var _this = this;
                    this.$rootScope = $rootScope;
                    this.ActionService = ActionService;
                    this.templateUrl = '/AngularTemplate/PaneSelectAction';
                    this.scope = {
                        currentAction: '='
                    };
                    this.restrict = 'E';
                    PaneSelectAction.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    PaneSelectAction.prototype.controller = function ($scope, $element, $attrs, $http) {
                        _this.PopulateData($scope, $http);
                        $scope.$watch(function (scope) { return scope.currentAction; }, _this.onActionChanged, true);
                        $scope.actionTypeSelected = function () {
                            var currentSelectedActivity;
                            var activities = $scope.actionTypes;
                            //find the selected activity
                            currentSelectedActivity = activities.filter(function (e) { return e.id == $scope.currentAction.activityTemplateId; })[0];
                            if (currentSelectedActivity != null || currentSelectedActivity != undefined) {
                                $scope.currentAction.activityTemplateName = currentSelectedActivity.name;
                                // Ensure that we do not send CrateStorage of previously selected storage to server.
                                $scope.currentAction.crateStorage = new dockyard.model.CrateStorage();
                                //Check for component activity
                                if (currentSelectedActivity.componentActivities != null) {
                                    var componentActivities = angular.fromJson(currentSelectedActivity.componentActivities);
                                    $scope.componentActivities = componentActivities;
                                    //Default configuration for the first child component activity will be shown
                                    $scope.childActivityStepId = componentActivities[0].id;
                                    $scope.childActivity = angular.extend({}, $scope.currentAction);
                                    $scope.childActivity.activityTemplateId = $scope.childActivityStepId;
                                    var eventArgs = new ActionTypeSelectedEventArgs($scope.childActivity);
                                    $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                                }
                                else {
                                    $scope.componentActivities = null;
                                    var eventArgs = new ActionTypeSelectedEventArgs($scope.currentAction);
                                    $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                                }
                            }
                            else {
                                $scope.componentActivities = null;
                            }
                        };
                        $scope.childActivityTypeSelected = function (childActionTemplateId) {
                            if (childActionTemplateId != null) {
                                $scope.$emit(MessageType[MessageType.PaneSelectAction_InitiateSaveAction], eventArgs);
                                $scope.childActivity.activityTemplateId = childActionTemplateId;
                                var eventArgs = new ActionTypeSelectedEventArgs($scope.childActivity);
                                $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                            }
                        };
                        $scope.$on(MessageType[MessageType.PaneSelectAction_Render], _this.onRender);
                        $scope.$on(MessageType[MessageType.PaneSelectAction_Hide], _this.onHide);
                        $scope.$on(MessageType[MessageType.PaneSelectAction_UpdateAction], _this.onUpdate);
                    };
                }
                PaneSelectAction.prototype.onActionChanged = function (newValue, oldValue, scope) {
                };
                PaneSelectAction.prototype.onRender = function (event, eventArgs) {
                    var scope = event.currentScope;
                    scope.isVisible = true;
                };
                PaneSelectAction.prototype.onHide = function (event, eventArgs) {
                    event.currentScope.isVisible = false;
                };
                PaneSelectAction.prototype.onUpdate = function (event, eventArgs) {
                    $.notify("Greetings from Select Action Pane. I've got a message about my neighbor saving its data so I saved my data, too.", "success");
                };
                PaneSelectAction.prototype.PopulateData = function ($scope, $http) {
                    $scope.actionTypes = [];
                    $http.get('/activities/available')
                        .then(function (resp) {
                        angular.forEach(resp.data, function (it) {
                            $scope.actionTypes.push(new dockyard.model.ActivityTemplate(it.id, it.name, it.version, it.componentActivities));
                        });
                    });
                };
                //The factory function returns Directive object as per Angular requirements
                PaneSelectAction.Factory = function () {
                    var directive = function ($rootScope, ActionService) {
                        return new PaneSelectAction($rootScope, ActionService);
                    };
                    directive['$inject'] = ['$rootScope', 'ActionService'];
                    return directive;
                };
                return PaneSelectAction;
            })();
            app.directive('paneSelectAction', PaneSelectAction.Factory());
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=paneselectaction.js.map