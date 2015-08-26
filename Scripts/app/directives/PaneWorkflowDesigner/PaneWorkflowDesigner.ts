/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {
    declare var Core: any;
    declare var ProcessBuilder: any;

    export function PaneWorkflowDesigner(): ng.IDirective {

        var onRender = function (eventArgs: RenderEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onRender', eventArgs);
        };


        var onProcessNodeTemplateAdded = function (eventArgs: ProcessNodeTemplateAddedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onCriteriaAdded', eventArgs);

            scope.widget.addCriteria({
                id: eventArgs.id,
                isTempId: eventArgs.isTempId,
                name: eventArgs.name
            });

            scope.$emit(
                MessageType[MessageType.PaneWorkflowDesigner_ProcessNodeTemplateSelecting],
                new ProcessNodeTemplateSelectingEventArgs(eventArgs.id, eventArgs.isTempId)
                );
        };


        var onProcessNodeTemplateRemoved = function (eventArgs: ProcessNodeTemplateRemovedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onCriteriaRemoved', eventArgs);

            scope.widget.removeCriteria(eventArgs.id, eventArgs.isTempId);
        };


        var onActionAdded = function (eventArgs: ActionAddedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onActionAdded', eventArgs);

            scope.widget.addAction(eventArgs.criteriaId, eventArgs.action, eventArgs.actionListType);

            scope.$emit(
                MessageType[MessageType.PaneWorkflowDesigner_ActionSelecting],
                new ActionSelectingEventArgs(eventArgs.criteriaId,
                    eventArgs.action.id, eventArgs.actionListType)
                );
        };


        var onActionRemoved = function (eventArgs: ActionRemovedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onActionRemove', eventArgs);
            scope.widget.removeAction(eventArgs.id, eventArgs.isTempId);
        };

        var onProcessNodeTemplateTempIdReplaced = function (eventArgs: ProcessNodeTemplateTempIdReplacedEventArgs, scope: IPaneWorkflowDesignerScope) {
            scope.widget.replaceCriteriaTempId(eventArgs.tempId, eventArgs.id);
        };

        var onProcessNodeTemplateRenamed = function (eventArgs: ProcessNodeTemplateNameUpdatedEventArgs, scope: IPaneWorkflowDesignerScope) {
            scope.widget.renameCriteria(eventArgs.id, eventArgs.text);
        };

        var onActionTempIdReplaced = function (eventArgs: ActionTempIdReplacedEventArgs, scope: IPaneWorkflowDesignerScope) {
            scope.widget.replaceActionTempId(eventArgs.tempId, eventArgs.id);
        };


        return {
            restrict: 'E',
            template: '<div style="overflow: auto;"></div>',
            scope: {},
            link: (scope: IPaneWorkflowDesignerScope, element: JQuery, attrs: any): void => {
                var factory = new ProcessBuilder.FabricJsFactory();
                var widget = Core.create(ProcessBuilder.Widget,
                    element.children()[0], factory, attrs.width, attrs.height);

                widget.on('startNode:click', function () {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_TemplateSelecting],
                            new TemplateSelectingEventArgs()
                        );
                    });
                });

                widget.on('addCriteriaNode:click', function () {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ProcessNodeTemplateAdding],
                            new ProcessNodeTemplateAddingEventArgs()
                        );
                    });
                });

                widget.on('criteriaNode:click', function (e, criteriaId, isTempId) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ProcessNodeTemplateSelecting],
                            new ProcessNodeTemplateSelectingEventArgs(criteriaId, isTempId)
                        );
                    });
                });

                widget.on('addActionNode:click', function (e, criteriaId, actionType) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ActionAdding],
                            new ActionAddingEventArgs(criteriaId, <model.ActionListType>actionType)
                        );
                    });
                });

                widget.on('actionNode:click', function (e, criteriaId, actionId, actionType) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ActionSelecting],
                            new ActionSelectingEventArgs(criteriaId, actionId, <model.ActionListType>actionType)
                        );
                    });
                });

                scope.widget = widget;

                // Event handlers.
                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_Render],
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ProcessNodeTemplateAdded],
                    (event: ng.IAngularEvent, eventArgs: ProcessNodeTemplateAddedEventArgs) => onProcessNodeTemplateAdded(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ProcessNodeTemplateRemoved],
                    (event: ng.IAngularEvent, eventArgs: ProcessNodeTemplateRemovedEventArgs) => onProcessNodeTemplateRemoved(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ActionAdded],
                    (event: ng.IAngularEvent, eventArgs: ActionAddedEventArgs) => onActionAdded(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ActionRemoved],
                    (event: ng.IAngularEvent, eventArgs: ActionRemovedEventArgs) => onActionRemoved(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ProcessNodeTemplateTempIdReplaced],
                    (event: ng.IAngularEvent, eventArgs: ProcessNodeTemplateTempIdReplacedEventArgs) => onProcessNodeTemplateTempIdReplaced(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ActionTempIdReplaced],
                    (event: ng.IAngularEvent, eventArgs: ActionTempIdReplacedEventArgs) => onActionTempIdReplaced(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ProcessNodeTemplateNameUpdated],
                    (event: ng.IAngularEvent, eventArgs: ProcessNodeTemplateNameUpdatedEventArgs) => onProcessNodeTemplateRenamed(eventArgs, scope));
            }
        };
    }
}

app.directive('paneWorkflowDesigner', dockyard.directives.paneWorkflowDesigner.PaneWorkflowDesigner);
