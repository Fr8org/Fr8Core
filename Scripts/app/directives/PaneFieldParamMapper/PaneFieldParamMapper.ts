/// <reference path="../../_all.ts" />
/// <reference path="../../../typings/angularjs/angular.d.ts"/>


module dockyard.directives.PaneFieldMapping {
    'use strict';

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneFieldMapping implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/Views/AngularTemplate/PaneFieldParamMapper.html';
        public restrict = 'E';
        public scope = {
            actionId: "@",
            mode: "@",
            mappedValue: "="
        };
        public controller = ['$scope', '$http', ($scope, $http) => {

            var transform = (map) => {

            };

            function init() {
                var loadedActions = false;
                var loadedFields = false;
                var urlPrefix = '/apimock';
                var mappedValue = {};
                $scope.mappedValue = mappedValue;

                $http.get(urlPrefix + '/actionparams')
                    .then((response) => {
                        loadedActions = true;
                        if ($scope.mode === 'param') {
                            $scope.toBeMappedTo = response.data;
                            $scope.HeadingRight = "Document Fields";
                            $scope.HeadingLeft = "Action Params";
                            return;
                        }
                        $scope.toBeMappedFrom = response.data;
                        $scope.HeadingLeft = "Document Fields";
                        $scope.HeadingRight = "Action Params";
                        return;

                    });

                $http.get(urlPrefix + '/documentfields')
                    .then((response) => {

                        loadedFields = true;
                        if ($scope.mode === 'param') {
                            $scope.toBeMappedFrom = response.data;
                            return;
                        }
                        $scope.toBeMappedTo = response.data;
                    });


                $scope.doneLoading = () => loadedActions && loadedFields;

                $scope.showHeading = () => {
                    if (loadedActions && loadedFields) {
                        return $scope.toBeMappedTo.length > 0 && $scope.toBeMappedFrom.length > 0;
                    }
                    return false;
                }
            }

            $scope.mapChanged = () => {
                transform($scope.toBeMappedFrom);
                //console.log($scope.toBeMappedFrom);
            };


            init();


        }];

        public static factory() {
            var directive = () => {
                return new PaneFieldMapping();

            };

            return directive;
        }
    }
    app.run([
        '$httpBackend', httpBackend => {

            var actions = [
                { Name: 'Action Param1', Id: 11 },
                { Name: 'Action Param2', Id: 12 },
                { Name: 'Action Param3', Id: 13 },
                { Name: 'Action Param4', Id: 14 }
            ];

            var documentFields = [
                { Name: 'Field1', Id: 21  },
                { Name: 'Field2', Id: 22  },
                { Name: 'Field3', Id: 23  },
                { Name: 'Field4', Id: 24  },
                { Name: 'Field5', Id: 25  },
                { Name: 'Field6', Id: 26  }
            ];

            httpBackend
                .whenGET('/apimock/actionparams')
                .respond(actions);

            httpBackend
                .whenGET('/apimock/documentfields')
                .respond(documentFields);
        }
    ]);
    app.directive('paneFieldParamMapper', PaneFieldMapping.factory());
}