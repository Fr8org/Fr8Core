/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings/metronic.d.ts" />
var app = angular.module("app", [
    "ui.router",
    "ui.bootstrap",
    "oc.lazyLoad",
    "ngSanitize",
    'ngResource',
    'ui.bootstrap',
    "ngMockE2E",
    "datatables",
    "ngFileUpload",
    "textAngular"
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
    }]);
app.controller('FooterController', ['$scope', function ($scope) {
        $scope.$on('$includeContentLoaded', function () {
            Layout.initFooter();
        });
    }]);
app.config(['$stateProvider', '$urlRouterProvider', '$httpProvider', function ($stateProvider, $urlRouterProvider, $httpProvider) {
        $urlRouterProvider.otherwise("/myaccount");
        $httpProvider.interceptors.push('spinnerHttpInterceptor');
        $stateProvider
            .state('myaccount', {
            url: "/myaccount",
            templateUrl: "/AngularTemplate/MyAccountPage",
            data: { pageTitle: 'My Account', pageSubTitle: '' }
        })
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
            data: { noTitle: true, noContainer: true },
        })
            .state('showIncidents', {
            url: "/showIncidents",
            templateUrl: "/AngularTemplate/ShowIncidents",
            data: { pageTitle: 'Incidents', pageSubTitle: 'This page displays all incidents' },
        })
            .state('showFacts', {
            url: "/showFacts",
            templateUrl: "/AngularTemplate/ShowFacts",
            data: { pageTitle: 'Facts', pageSubTitle: 'This page displays all facts' },
        })
            .state('processTemplateDetails', {
            url: "/processes/{id}/details",
            templateUrl: "/AngularTemplate/ProcessTemplateDetails",
            data: { pageTitle: 'Process Template Details', pageSubTitle: '' }
        })
            .state('managefiles', {
            url: "/managefiles",
            templateUrl: "/AngularTemplate/ManageFileList",
            data: { pageTitle: 'Manage Files', pageSubTitle: '' }
        })
            .state('accounts', {
            url: '/accounts',
            templateUrl: '/AngularTemplate/AccountList',
            data: { pageTitle: 'Manage Dockyard Accounts', pageSubTitle: '' }
        })
            .state('accountDetails', {
            url: '/accounts/{id}',
            templateUrl: '/AngularTemplate/AccountDetails',
            data: { pageTitle: 'Account Details', pageSubTitle: '' }
        });
    }]);
app.run(["$rootScope", "settings", "$state", function ($rootScope, settings, $state) {
        $rootScope.$state = $state;
    }]);
app.constant('spinnerHttpInterceptor', {
    request: function (config) {
        if (config && config.params && config.params['suppressSpinner']) {
            delete (config.params.suppressSpinner);
        }
        else {
            Metronic.startPageLoading({ animate: true });
        }
        return config;
    },
    response: function (config) {
        Metronic.stopPageLoading();
        return config;
    },
    responseError: function (config) {
        Metronic.stopPageLoading();
        return config;
    }
});
//# sourceMappingURL=app.js.map