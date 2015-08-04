'use strict';

app.directive('processBuilder', function () {
    var linkFn = function (scope, element, attrs) {
        var factory = new ProcessBuilder.FabricJsFactory();
        var widget = Core.create(ProcessBuilder.Widget,
            element, factory, attrs.width, attrs.height);

        // scope.$on('addCriteria', function (event, args) {
        //     widget.addCriteria({ id: args.criteriaId });
        // });
        // 
        // scope.$on('removeCriteria', function (event, args) {
        //     widget.removeCriteria(args.criteriaId);
        // });
        // 
        // scope.$on('addAction', function (event, args) {
        //     widget.addAction(args.criteriaId, { id: args.actionId });
        // });
        // 
        // scope.$on('removeAction', function (event, args) {
        //     widget.removeAction(args.criteriaId, args.actionId);
        // });

        //scope.widget = widget;

        widget.on('addCriteriaNode:click', function () {
            scope.$apply(function () {
                scope.addCriteria();
            });
        });

        widget.on('criteriaNode:click', function (e, criteriaId) {
            // scope.$emit('processBuilder:criteriaNode', {
            //     directiveId: attrs.id,
            //     criteriaId: criteriaId
            // });

            scope.$apply(function () {
                scope.selectCriteria({ criteriaId: criteriaId });
            });
        });

        widget.on('addActionNode:click', function (e, criteriaId) {
            scope.$emit('processBuilder:addActionNode', {
                directiveId: attrs.id,
                criteriaId: criteriaId
            });
        });

        widget.on('actionNode:click', function (e, criteriaId, actionId) {
            scope.$emit('processBuilder:actionNode', {
                directiveId: attrs.id,
                criteriaId: criteriaId,
                actionId: actionId
            });
        });


        scope.$watchCollection('criteriaList',
            function (newList, oldList) {
                var collect = function (original, missing) {
                    var result = [];
                    var i, j, found;

                    for (i = 0; i < original.length; ++i) {
                        found = false;
                        for (j = 0; j < missing.length; ++j) {
                            if (missing[j].id === original[i].id) {
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            result.push(original[i]);
                        }
                    }

                    return result;
                };

                newList = newList || [];
                oldList = oldList || [];

                // Collecting added criteria.
                var addedCriteria = collect(newList, oldList);

                addedCriteria.forEach(function (it) {
                    widget.addCriteria(it);
                });

                // Collecting removed criteria.
                var removedCriteria = collect(oldList, newList);

                removedCriteria.forEach(function (it) {
                    widget.removeCriteria(it.id);
                });
            }
        );
    };

    return {
        restrict: 'E',
        template: '<div></div>',
        link: linkFn,
        scope: {
            'criteriaList': '=',
            'addCriteria': '&onAddCriteria',
            'selectCriteria': '&onSelectCriteria'
        }
    };
});