<<<<<<< HEAD
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
                MessageType[MessageType["PaneSelectAction_ActionAdd"] = 6] = "PaneSelectAction_ActionAdd";
                MessageType[MessageType["PaneSelectAction_ActivityTypeSelected"] = 7] = "PaneSelectAction_ActivityTypeSelected";
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
            var ActivityTypeSelectedEventArgs = (function () {
                function ActivityTypeSelectedEventArgs(activityTemplate) {
                    // Clone Action to prevent any issues due to possible mutation of source object
                    this.activityTemplate = angular.extend({}, activityTemplate);
                }
                return ActivityTypeSelectedEventArgs;
            })();
            paneSelectAction.ActivityTypeSelectedEventArgs = ActivityTypeSelectedEventArgs;
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
            var ActionRemovedEventArgs = (function () {
                function ActionRemovedEventArgs(id, isTempId) {
                    this.id = id;
                    this.isTempId = isTempId;
                }
                return ActionRemovedEventArgs;
            })();
            paneSelectAction.ActionRemovedEventArgs = ActionRemovedEventArgs;
            var ActionAddEventArgs = (function () {
                function ActionAddEventArgs() {
                }
                return ActionAddEventArgs;
            })();
            paneSelectAction.ActionAddEventArgs = ActionAddEventArgs;
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var PaneSelectAction = (function () {
                function PaneSelectAction($modal, ActivityTemplateService) {
                    var _this = this;
                    this.$modal = $modal;
                    this.ActivityTemplateService = ActivityTemplateService;
                    this.scope = {
                        field: '='
                    };
                    this.restrict = 'E';
                    PaneSelectAction.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    PaneSelectAction.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                        $scope.$on(MessageType[MessageType.PaneSelectAction_ActionAdd], angular.bind(_this, _this.onActionAdd));
                    };
                }
                PaneSelectAction.prototype.onActionAdd = function () {
                    var _this = this;
                    //we should list available actions to user and let him select one
                    this.ActivityTemplateService.getAvailableActivities().$promise.then(function (categoryList) {
                        //we should open a modal to let user select one of our activities
                        _this.$modal.open({
                            animation: true,
                            templateUrl: 'AngularTemplate/PaneSelectActionModal',
                            //this is a simple modal controller, so i didn't have an urge to seperate this
                            //but resolve is used to make future seperation easier
                            controller: ['$modalInstance', '$scope', 'activityCategories', function ($modalInstance, $modalScope, activityCategories) {
                                    $modalScope.activityCategories = activityCategories;
                                    $modalScope.activityTypeSelected = function (activityType) {
                                        $modalInstance.close(activityType);
                                    };
                                    $modalScope.cancel = function () {
                                        $modalInstance.dismiss();
                                    };
                                }],
                            resolve: {
                                'activityCategories': function () { return categoryList; }
                            },
                            windowClass: 'select-action-modal'
                        }).result.then(function (selectedActivity) {
                            //now we should emit an activity type selected event
                            var eventArgs = new ActivityTypeSelectedEventArgs(selectedActivity);
                            _this._$scope.$emit(MessageType[MessageType.PaneSelectAction_ActivityTypeSelected], eventArgs);
                        });
                    });
                };
                //The factory function returns Directive object as per Angular requirements
                PaneSelectAction.Factory = function () {
                    var directive = function ($modal, ActivityTemplateService) {
                        return new PaneSelectAction($modal, ActivityTemplateService);
                    };
                    directive['$inject'] = ['$modal', 'ActivityTemplateService'];
                    return directive;
                };
                return PaneSelectAction;
            })();
            app.directive('paneSelectAction', PaneSelectAction.Factory());
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
=======
/// <reference path="../../_all.ts" />
var __extends = this.__extends || function (d, b) {
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
                MessageType[MessageType["PaneSelectAction_ActionAdd"] = 6] = "PaneSelectAction_ActionAdd";
                MessageType[MessageType["PaneSelectAction_ActivityTypeSelected"] = 7] = "PaneSelectAction_ActivityTypeSelected";
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
            var ActivityTypeSelectedEventArgs = (function () {
                function ActivityTypeSelectedEventArgs(activityTemplate) {
                    // Clone Action to prevent any issues due to possible mutation of source object
                    this.activityTemplate = angular.extend({}, activityTemplate);
                }
                return ActivityTypeSelectedEventArgs;
            })();
            paneSelectAction.ActivityTypeSelectedEventArgs = ActivityTypeSelectedEventArgs;
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
            var ActionRemovedEventArgs = (function () {
                function ActionRemovedEventArgs(id, isTempId) {
                    this.id = id;
                    this.isTempId = isTempId;
                }
                return ActionRemovedEventArgs;
            })();
            paneSelectAction.ActionRemovedEventArgs = ActionRemovedEventArgs;
            var ActionAddEventArgs = (function () {
                function ActionAddEventArgs() {
                }
                return ActionAddEventArgs;
            })();
            paneSelectAction.ActionAddEventArgs = ActionAddEventArgs;
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var PaneSelectAction = (function () {
                function PaneSelectAction($modal, ActivityTemplateService) {
                    var _this = this;
                    this.$modal = $modal;
                    this.ActivityTemplateService = ActivityTemplateService;
                    this.scope = {
                        field: '='
                    };
                    this.restrict = 'E';
                    PaneSelectAction.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    PaneSelectAction.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                        $scope.$on(MessageType[MessageType.PaneSelectAction_ActionAdd], angular.bind(_this, _this.onActionAdd));
                    };
                }
                PaneSelectAction.prototype.onActionAdd = function () {
                    var _this = this;
                    //we should list available actions to user and let him select one
                    this.ActivityTemplateService.getAvailableActivities().$promise.then(function (categoryList) {
                        //we should open a modal to let user select one of our activities
                        _this.$modal.open({
                            animation: true,
                            templateUrl: 'AngularTemplate/PaneSelectActionModal',
                            //this is a simple modal controller, so i didn't have an urge to seperate this
                            //but resolve is used to make future seperation easier
                            controller: ['$modalInstance', '$scope', 'activityCategories', function ($modalInstance, $modalScope, activityCategories) {
                                    $modalScope.activityCategories = activityCategories;
                                    $modalScope.activityTypeSelected = function (activityType) {
                                        $modalInstance.close(activityType);
                                    };
                                    $modalScope.cancel = function () {
                                        $modalInstance.dismiss();
                                    };
                                }],
                            resolve: {
                                'activityCategories': function () { return categoryList; }
                            },
                            windowClass: 'select-action-modal'
                        }).result.then(function (selectedActivity) {
                            //now we should emit an activity type selected event
                            var eventArgs = new ActivityTypeSelectedEventArgs(selectedActivity);
                            _this._$scope.$emit(MessageType[MessageType.PaneSelectAction_ActivityTypeSelected], eventArgs);
                        });
                    });
                };
                //The factory function returns Directive object as per Angular requirements
                PaneSelectAction.Factory = function () {
                    var directive = function ($modal, ActivityTemplateService) {
                        return new PaneSelectAction($modal, ActivityTemplateService);
                    };
                    directive['$inject'] = ['$modal', 'ActivityTemplateService'];
                    return directive;
                };
                return PaneSelectAction;
            })();
            app.directive('paneSelectAction', PaneSelectAction.Factory());
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
>>>>>>> parent of e4a2f3e... Delete  "Create Accounts", Rename "Manage Dockyard Accounts" to "Manage Accounts", Move Find Object to Tools menu and rename it Find, add authorize attributes
//# sourceMappingURL=PaneSelectAction.js.map