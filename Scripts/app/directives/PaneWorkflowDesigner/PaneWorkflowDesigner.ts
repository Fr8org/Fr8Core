/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {
    declare var Core: any;
    declare var PlanBuilder: any;

    export function PaneWorkflowDesigner(): ng.IDirective {

        var onRender = function (eventArgs: RenderEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onRender', eventArgs);
        };

        var onActionAdded = function (eventArgs: AddActionEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onActionAdded', eventArgs);
            
            var actionObj = <any>eventArgs.action;

            scope.widget.addAction(eventArgs.criteriaId, eventArgs.action, 1);

            if (eventArgs.doNotRaiseSelectedEvent) return;

            scope.$emit(
                MessageType[MessageType.PaneWorkflowDesigner_ActionSelected],
                new ActionSelectedEventArgs(eventArgs.criteriaId, eventArgs.action.id, 0)
            );
        };


        var onActionRemoved = function (eventArgs: ActionRemovedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onActionRemove', eventArgs);
            scope.widget.removeAction(eventArgs.id, eventArgs.isTempId);
        };

        var onProcessNodeTemplateTempIdReplaced = function (eventArgs: ReplaceTempIdForProcessNodeTemplateEventArgs, scope: IPaneWorkflowDesignerScope) {
            scope.widget.replaceCriteriaTempId(eventArgs.tempId, eventArgs.id);
        };

        var onProcessNodeTemplateRenamed = function (eventArgs: UpdateProcessNodeTemplateNameEventArgs, scope: IPaneWorkflowDesignerScope) {
            scope.widget.renameCriteria(eventArgs.id, eventArgs.text);
        };

        var onActionTempIdReplaced = function (eventArgs: ActionTempIdReplacedEventArgs, scope: IPaneWorkflowDesignerScope) {
            scope.widget.replaceActionTempId(eventArgs.tempId, eventArgs.id);
        };

        var onUpdateActivityTemplateIdForAction = function (eventArgs: UpdateActivityTemplateIdEventArgs, scope: IPaneWorkflowDesignerScope) {
            scope.widget.updateActivityTemplateId(eventArgs.id, eventArgs.activityTemplateId);
        };

        return {
            restrict: 'E',
            template: '<div style="overflow: auto;"></div>',
            scope: {},
            link: (scope: IPaneWorkflowDesignerScope, element: JQuery, attrs: any): void => {
                var factory = new PlanBuilder.FabricJsFactory();
                var widget = Core.create(PlanBuilder.Widget,
                    element.children()[0], factory, attrs.width, attrs.height);

                widget.on('addActionNode:click', function (e, criteriaId, actionType) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ActionAdding],
                            new ActionAddingEventArgs(criteriaId)
                        );
                    });
                });

                widget.on('actionNode:click', function (e, criteriaId, actionId, actionType, activityTemplateId) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ActionSelected],
                            new ActionSelectedEventArgs(criteriaId, actionId, activityTemplateId)
                        );
                    });
                });

                scope.widget = widget;

                // Event handlers.
                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_Render],
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_AddAction],
                    (event: ng.IAngularEvent, eventArgs: AddActionEventArgs) => onActionAdded(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ActionRemoved],
                    (event: ng.IAngularEvent, eventArgs: ActionRemovedEventArgs) => onActionRemoved(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ReplaceTempIdForProcessNodeTemplate],
                    (event: ng.IAngularEvent, eventArgs: ReplaceTempIdForProcessNodeTemplateEventArgs) => onProcessNodeTemplateTempIdReplaced(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_UpdateProcessNodeTemplateName],
                    (event: ng.IAngularEvent, eventArgs: UpdateProcessNodeTemplateNameEventArgs) => onProcessNodeTemplateRenamed(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_UpdateActivityTemplateId],
                    (event: ng.IAngularEvent, eventArgs: UpdateActivityTemplateIdEventArgs) => onUpdateActivityTemplateIdForAction(eventArgs, scope));


            }
        };
    }
}

app.directive('paneWorkflowDesigner', dockyard.directives.paneWorkflowDesigner.PaneWorkflowDesigner);
