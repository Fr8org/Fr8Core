/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {
    declare var Core: any;
    declare var ProcessBuilder: any;

    export function PaneWorkflowDesigner(): ng.IDirective {

        var onRender = function (eventArgs: RenderEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onRender', eventArgs);
        };


        var onCriteriaAdded = function (eventArgs: CriteriaAddedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onCriteriaAdded', eventArgs);

            scope.widget.addCriteria(eventArgs.criteria);

            scope.$emit(
                MessageType[MessageType.PaneWorkflowDesigner_CriteriaSelecting],
                new CriteriaSelectingEventArgs(eventArgs.criteria.id)
            );
        };


        var onCriteriaRemoved = function (eventArgs: CriteriaRemovedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onCriteriaRemoved', eventArgs);

            scope.widget.removeCriteria(eventArgs.criteriaId);
        };


        var onActionAdded = function (eventArgs: ActionAddedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onActionAdded', eventArgs);

            scope.widget.addAction(eventArgs.criteriaId, eventArgs.action);

            scope.$emit(
                MessageType[MessageType.PaneWorkflowDesigner_ActionSelecting],
                new ActionSelectingEventArgs(eventArgs.criteriaId, eventArgs.action.id)
            );
        };


        var onActionRemoved = function (eventArgs: ActionRemovedEventArgs, scope: IPaneWorkflowDesignerScope) {
            console.log('PaneWorkflowDesigner::onActionRemove', eventArgs);

            scope.widget.removeAction(eventArgs.criteriaId, eventArgs.actionId);
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
                            MessageType[MessageType.PaneWorkflowDesigner_TemplateSelected],
                            new TemplateSelectedEventArgs()
                        );
                    });
                });

                widget.on('addCriteriaNode:click', function () {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_CriteriaAdding],
                            new CriteriaAddingEventArgs()
                        );
                    });
                });

                widget.on('criteriaNode:click', function (e, criteriaId) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_CriteriaSelecting],
                            new CriteriaSelectingEventArgs(criteriaId)
                        );
                    });
                });

                widget.on('addActionNode:click', function (e, criteriaId) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ActionAdding],
                            new ActionAddingEventArgs(criteriaId)
                        );
                    });
                });

                widget.on('actionNode:click', function (e, criteriaId, actionId) {
                    scope.$apply(function () {
                        scope.$emit(
                            MessageType[MessageType.PaneWorkflowDesigner_ActionSelecting],
                            new ActionSelectingEventArgs(criteriaId, actionId)
                        );
                    });
                });

                scope.widget = widget;

                // Event handlers.
                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_Render],
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_CriteriaAdded],
                    (event: ng.IAngularEvent, eventArgs: CriteriaAddedEventArgs) => onCriteriaAdded(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_CriteriaRemoved],
                    (event: ng.IAngularEvent, eventArgs: CriteriaRemovedEventArgs) => onCriteriaRemoved(eventArgs, scope));

                scope.$on(MessageType[MessageType.PaneWorkflowDesigner_ActionAdded],
                    (event: ng.IAngularEvent, eventArgs: ActionAddedEventArgs) => onActionAdded(eventArgs, scope));
            }
        };
    }
}

app.directive('paneWorkflowDesigner', dockyard.directives.paneWorkflowDesigner.PaneWorkflowDesigner);
