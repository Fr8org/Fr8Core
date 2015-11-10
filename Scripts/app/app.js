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
    "textAngular",
    "ui.select",
    "pusher-angular",
    "ngToast",
    "frapontillo.bootstrap-switch"
]);
/* Configure ocLazyLoader(refer: https://github.com/ocombe/ocLazyLoad) */
app.config(['$ocLazyLoadProvider', function ($ocLazyLoadProvider) {
        $ocLazyLoadProvider.config({
            cssFilesInsertBefore: 'ng_load_plugins_before' // load the above css files before a LINK element with this ID. Dynamic CSS files must be loaded between core and theme css files
        });
    }]);
/* Setup global settings */
app.factory('settings', ['$rootScope', function ($rootScope) {
        // supported languages
        var settings = {
            layout: {
                pageAutoScrollOnLoad: 1000 // auto scroll to top on page load
            },
            layoutImgPath: Metronic.getAssetsPath() + 'admin/layout/img/',
            layoutCssPath: Metronic.getAssetsPath() + 'admin/layout/css/'
        };
        $rootScope.settings = settings;
        return settings;
    }]);
/* Setup App Main Controller */
app.controller('AppController', ['$scope', '$rootScope', function ($scope, $rootScope) {
        $scope.$on('$viewContentLoaded', function () {
            Metronic.initComponents(); // init core components
            //Layout.init(); //  Init entire layout(header, footer, sidebar, etc) on page load if the partials included in server side instead of loading with ng-include directive 
        });
    }]);
/***
Layout Partials.
By default the partials are loaded through AngularJS ng-include directive. In case they loaded in server side(e.g: PHP include function) then below partial
initialization can be disabled and Layout.init() should be called on page load complete as explained above.
***/
/* Setup Layout Part - Header */
app.controller('HeaderController', ['$scope', function ($scope) {
        $scope.$on('$includeContentLoaded', function () {
            Layout.initHeader(); // init header
        });
    }]);
/* Setup Layout Part - Footer */
app.controller('FooterController', ['$scope', function ($scope) {
        $scope.$on('$includeContentLoaded', function () {
            Layout.initFooter(); // init footer
        });
    }]);
/* Setup Rounting For All Pages */
app.config(['$stateProvider', '$urlRouterProvider', '$httpProvider', function ($stateProvider, $urlRouterProvider, $httpProvider) {
        // Install a HTTP request interceptor that causes 'Processing...' message to display
     $httpProvider.interceptors.push(function ($q) {

            return {
                request: function (config) {
                    // Show page spinner If there is no request parameter suppressSpinner.
                    if (config && config.params && config.params['suppressSpinner']) {
                        // We don't want this parameter to be sent to backend so remove it if found.
                        delete (config.params.suppressSpinner);
                    }
                    else {
                    }
                    return config;
                },
                response: function (config) {
                    Metronic.stopPageLoading();
                    return config;
                },
                responseError: function (config) {
 Metronic.stopPageLoading();
                    return $q.reject(config);
                }
            };
        });
        // Redirect any unmatched url
        $urlRouterProvider.otherwise("/myaccount");
        $stateProvider
            .state('myaccount', {
            url: "/myaccount",
            templateUrl: "/AngularTemplate/MyAccountPage",
            data: { pageTitle: 'My Account', pageSubTitle: '' }
        })
            .state('routeList', {
            url: "/processes",
            templateUrl: "/AngularTemplate/RouteList",
            data: { pageTitle: 'Routes', pageSubTitle: 'This page displays all Routes' }
        })
            .state('routeForm', {
            url: "/processes/{id}",
            templateUrl: "/AngularTemplate/RouteForm",
            data: { pageTitle: 'Route', pageSubTitle: 'Add a new Route' },
        })
            .state('processBuilder', {
            url: "/processes/{id}/builder",
            templateUrl: "/AngularTemplate/ProcessBuilder",
            data: { pageTitle: '' },
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
            .state('routeDetails', {
            url: "/processes/{id}/details",
            templateUrl: "/AngularTemplate/RouteDetails",
            data: { pageTitle: 'Route Details', pageSubTitle: '' }
        })
            .state('managefiles', {
            url: "/managefiles",
            templateUrl: "/AngularTemplate/ManageFileList",
            data: { pageTitle: 'Manage Files', pageSubTitle: '' }
        })
            .state('accounts', {
            url: '/accounts',
            templateUrl: '/AngularTemplate/AccountList',
            data: { pageTitle: 'Manage Accounts', pageSubTitle: '' }
        })
            .state('accountDetails', {
            url: '/accounts/{id}',
            templateUrl: '/AngularTemplate/AccountDetails',
            data: { pageTitle: 'Account Details', pageSubTitle: '' }
        })
            .state('containerDetails', {
            url: "/container/{id}/details",
            templateUrl: "/AngularTemplate/containerDetails",
            data: { pageTitle: 'Container  Details', pageSubTitle: '' }
        })
            .state('solution', {
            url: "/solution/{solutionName}",
            templateUrl: "/AngularTemplate/Solution",
            data: { pageTitle: 'Create a Solution', pageSubTitle: '' }
        })
            .state('containers', {
            url: "/containers",
            templateUrl: "/AngularTemplate/ContainerList",
            data: { pageTitle: 'Containers', pageSubTitle: 'This page displays all Containers ' },
        })
            .state('webservices', {
            url: "/webservices",
            templateUrl: "/AngularTemplate/WebServiceList",
            data: { pageTitle: 'Web Services', pageSubTitle: '' }
        });
    }]);
/* Init global settings and run the app */
app.run(["$rootScope", "settings", "$state", function ($rootScope, settings, $state) {
        $rootScope.$state = $state; // state to be accessed from view
    }]);
//# sourceMappingURL=app.js.map