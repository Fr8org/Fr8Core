/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings/metronic.d.ts" />
var app = angular.module("app", [
    "ui.router",
    "ui.bootstrap",
    "oc.lazyLoad",
    "ngSanitize",
    'ngResource',
    'ui.bootstrap'
]);
app.config(['$ocLazyLoadProvider', function ($ocLazyLoadProvider) {
        $ocLazyLoadProvider.config({
            cssFilesInsertBefore: 'ng_load_plugins_before'
        });
    }]);
app.factory('settings', ['$rootScope', function ($rootScope) {
        var settings = {
            layout: {
                pageAutoScrollOnLoad: 1000
            },
            layoutImgPath: Metronic.getAssetsPath() + 'admin/layout/img/',
            layoutCssPath: Metronic.getAssetsPath() + 'admin/layout/css/'
        };
        $rootScope.settings = settings;
        return settings;
    }]);
app.controller('AppController', ['$scope', '$rootScope', function ($scope, $rootScope) {
        $scope.$on('$viewContentLoaded', function () {
            Metronic.initComponents();
        });
    }]);
app.controller('HeaderController', ['$scope', function ($scope) {
        $scope.$on('$includeContentLoaded', function () {
            Layout.initHeader();
        });
    }]);
app.controller('PageHeadController', ['$scope', function ($scope) {
        $scope.$on('$includeContentLoaded', function () {
            Demo.init();
        });
    }]);
app.controller('FooterController', ['$scope', function ($scope) {
        $scope.$on('$includeContentLoaded', function () {
            Layout.initFooter();
        });
    }]);
app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $urlRouterProvider.otherwise("/processes");
        $stateProvider
            .state('processTemplates', {
            url: "/processes",
            templateUrl: "/AngularTemplate/ProcessTemplateList",
            data: { pageTitle: 'Process Templates', pageSubTitle: 'This page displays all process templates' }
        })
            .state('processTemplate', {
            url: "/processes/{id}",
            templateUrl: "/AngularTemplate/ProcessTemplateForm",
            data: { pageTitle: 'Process Templates', pageSubTitle: 'Add a new Process Template' },
        })
            .state('processBuilder', {
            url: "/processes/{id}/builder",
            templateUrl: "/AngularTemplate/ProcessBuilder",
            data: { pageTitle: 'Process Builder', pageSubTitle: 'Configure your process here' },
        });
    }]);
app.run(["$rootScope", "settings", "$state", function ($rootScope, settings, $state) {
        $rootScope.$state = $state;
    }]);
//# sourceMappingURL=app.js.map