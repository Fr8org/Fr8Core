'use strict';

app.directive('processBuilder', function () {
    var linkFn = function (scope, element, attrs) {
        var factory = new ProcessBuilder.FabricJsFactory();
        var widget = Core.create(ProcessBuilder.Widget,
            element, factory, attrs.width, attrs.height);

        scope.$on('addCriteria', function (event, args) {
            widget.addCriteria({ id: args.criteriaId });
        });

        scope.$on('removeCriteria', function (event, args) {
            widget.removeCriteria(args.criteriaId);
        });

        scope.$on('addAction', function (event, args) {
            widget.addAction(args.criteriaId, { id: args.actionId });
        });

        scope.$on('removeAction', function (event, args) {
            widget.removeAction(args.criteriaId, args.actionId);
        });

        widget.on('startNode:click', function () {
            scope.$emit('startNode:click', {
                directiveId: attrs.id
            });
        });

        widget.on('addCriteriaNode:click', function () {
            scope.$emit('addCriteriaNode:click', {
                directiveId: attrs.id
            });
        });

        widget.on('criteriaNode:click', function (e, criteriaId) {
            scope.$emit('criteriaNode:click', {
                directiveId: attrs.id,
                criteriaId: criteriaId
            });
        });

        widget.on('addActionNode:click', function (e, criteriaId) {
            scope.$emit('addActionNode:click', {
                directiveId: attrs.id,
                criteriaId: criteriaId
            });
        });

        widget.on('actionNode:click', function (e, criteriaId, actionId) {
            scope.$emit('actionNode:click', {
                directiveId: attrs.id,
                criteriaId: criteriaId,
                actionId: actionId
            });
        });
    };

    return {
        restrict: 'E',
        template: '<div></div>',
        link: linkFn
    };
});