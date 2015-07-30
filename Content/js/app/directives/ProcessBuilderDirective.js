'use strict';

MetronicApp.directive('processBuilder', function () {
    var linkFn = function (scope, element, attrs) {
        var factory = new ProcessBuilder.FabricJsFactory();
        var widget = Core.create(ProcessBuilder.Widget,
            element, factory, attrs.width, attrs.height);

        widget.on('startNode:click', function () {
            if (!attrs.startClickCallback) { return; }
            scope[attrs.startClickCallback].call(this);
        });

        widget.on('addCriteriaNode:click', function () {
            if (!attrs.addCriteriaClickCallback) { return; }

            console.log(attrs.addCriteriaClickCallback);
            console.log(scope);

            scope[attrs.addCriteriaClickCallback].call(this);
        });

        widget.on('criteriaNode:click', function (e, criteriaId) {
            if (!attrs.criteriaClickCallback) { return; }
            scope[attrs.criteriaClickCallback].call(this, criteriaId);
        });

        widget.on('addActionNode:click', function (e, criteriaId) {
            if (!attrs.addActionClickCallback) { return; }
            scope[attrs.addActionClickCallback].call(this, criteriaId);
        });

        widget.on('actionNode:click', function (e, criteriaId, actionId) {
            if (!attrs.actionClickCallback) { return; }
            scope[attrs.actionClickCallback].call(this, criteriaId, actionId);
        });

        scope[attrs.id] = widget;
    };

    return {
        restrict: 'E',
        template: '<div></div>',
        link: linkFn
    };
});