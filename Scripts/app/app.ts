/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings/metronic.d.ts" />
var app = angular.module("app", [
    "templates",
    "ui.router",
    "ui.bootstrap",
    "oc.lazyLoad",
    "ngSanitize",
    'ngResource',
    'ui.bootstrap',
    "datatables",
    "ngFileUpload",
    "textAngular",
    "ui.select",
    "pusher-angular",
    "ngToast",
    "frapontillo.bootstrap-switch",
    "ApplicationInsightsModule",
    "dndLists",
    "ngTable",
    "mb-scrollbar",
    "ngMessages",
    "ivh.treeview",
    "ngMaterial",
    "angularResizable",
    "mdColorPicker",
    "md.data.table",
    "popoverToggle",
    'jsonFormatter'
]);

/* For compatibility with older versions of script files. Can be safely deleted later. */
app.constant('urlPrefix', '/api');

/* Configure ocLazyLoader(refer: https://github.com/ocombe/ocLazyLoad) */
app.config(['$ocLazyLoadProvider', ($ocLazyLoadProvider) => {
    $ocLazyLoadProvider.config({
        cssFilesInsertBefore: 'ng_load_plugins_before' // load the above css files before a LINK element with this ID. Dynamic CSS files must be loaded between core and theme css files
    });
}]);

/* Setup global settings */
app.factory('settings', ['$rootScope', ($rootScope) => {
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
app.controller('AppController', ['$scope', '$rootScope', '$window', function ($scope, $rootScope, $window) {
    $scope.$on('$viewContentLoaded', () => {
        Metronic.initComponents(); // init core components
        //Layout.init(); //  Init entire layout(header, footer, sidebar, etc) on page load if the partials included in server side instead of loading with ng-include directive 
    });
    $scope.displayDeveloperMenu = JSON.parse($window.sessionStorage.getItem("displayDeveloperMenu"));
    if ($scope.displayDeveloperMenu) {
        $scope.displayDeveloperMenuText = "Hide Developer Menu";
    } else {
        $scope.displayDeveloperMenuText = "Show Developer Menu";
    }

    $scope.switchDeveloperMenu = () => {
        if ($scope.displayDeveloperMenu) {
            $window.sessionStorage.setItem("displayDeveloperMenu", false);
            $scope.displayDeveloperMenuText = "Show Developer Menu";
            $scope.displayDeveloperMenu = false;
        } else {
            $window.sessionStorage.setItem("displayDeveloperMenu", true);
            $scope.displayDeveloperMenuText = "Hide Developer Menu";
            $scope.displayDeveloperMenu = true;
        }
    };
}]);

app.config(['$mdThemingProvider', ($mdThemingProvider) => {
    $mdThemingProvider.definePalette('fr8Theme', {
        '50': '26a69a',
        '100': '26a69a',
        '200': '26a69a',
        '300': '26a69a',
        '400': '26a69a',
        '500': '26a69a',
        '600': '26a69a',
        '700': '26a69a',
        '800': '26a69a',
        '900': '26a69a',
        'A100': '26a69a',
        'A200': '26a69a',
        'A400': '26a69a',
        'A700': '26a69a',
        'contrastDefaultColor': 'light',
        'contrastDarkColors': ['50', '100',
            '200', '300', '400', 'A100'],
        'contrastLightColors': undefined
    });
    $mdThemingProvider.theme('default')
        .primaryPalette('fr8Theme');
}]);

/***
Layout Partials.
By default the partials are loaded through AngularJS ng-include directive. In case they loaded in server side(e.g: PHP include function) then below partial 
initialization can be disabled and Layout.init() should be called on page load complete as explained above.
***/

/* Setup Layout Part - Header */
app.controller('HeaderController', ['$scope', '$http', '$window', '$state', 'TerminalService', 'PlanService', ($scope, $http, $window, $state, TerminalService, PlanService) => {
    if ($state.current.name === 'plan' || $state.current.name === 'plan.details') {
        $scope.showPlanBuilderHeader = true;
    }
    else {
        $scope.showPlanBuilderHeader = false;
    }

    Layout.initHeader(); // init header       



    $scope.addPlan = function () {
        var plan = new dockyard.model.PlanDTO();
        plan.planState = dockyard.model.PlanState.Inactive;
        plan.visibility = { hidden: false, public: false };
        //plan.visibility = dockyard.model.PlanVisibility.Standard;
        var result = PlanService.save(plan);

        result.$promise
            .then(() => {
                $state.go('plan', { id: result.id });
                //window.location.href = 'plans/' + result.plan.id + '/builder';
            });
    };

    $scope.terminals = TerminalService.getAll();

    $scope.goToPlanDirectory = function (planDirectoryUrl) {
        $http.post('/api/authentication/authenticatePlanDirectory', {})
            .then(function (res) {
                var token = res.data.token;
                var url = planDirectoryUrl + '/AuthenticateByToken?token=' + token;
                $window.open(url, '_blank');
            });
    };

    $scope.runManifestRegistryMonitoring = () => { $http.post('/api/manifest_registry/runMonitoring', {}); };
}]);

/* Setup Layout Part - Footer */
app.controller('FooterController', ['$scope', ($scope) => {
    $scope.$on('$includeContentLoaded', () => {
        Layout.initFooter(); // init footer
    });
}]);

/* Set Application Insights */
app.config(['applicationInsightsServiceProvider', function (applicationInsightsServiceProvider) {
    var options;
    //Temporary instr key (for local instances) until the real one is loaded
    applicationInsightsServiceProvider.configure('e08e940f-1491-440c-8d39-f38e9ff053db', options, true);

    $.get('/api/v1/configuration/instrumentation-key').then((instrumentationKey: string) => {
        console.log(instrumentationKey);
        if (instrumentationKey.indexOf('0000') === -1) { // if not local instance ('Debug' configuration)
            options = { applicationName: 'HubWeb' };
            applicationInsightsServiceProvider.configure(instrumentationKey, options, true);
        } else {
            // don't send telemetry 
            options = {
                applicationName: '',
                autoPageViewTracking: false,
                autoLogTracking: false,
                autoExceptionTracking: false,
                sessionInactivityTimeout: 1
            };
            applicationInsightsServiceProvider.configure(instrumentationKey, options, false);
        }
    });
}]);


/* Setup Routing For All Pages */
app.config(['$stateProvider', '$urlRouterProvider', '$httpProvider', '$locationProvider', ($stateProvider: ng.ui.IStateProvider, $urlRouterProvider, $httpProvider: ng.IHttpProvider, $locationProvider: ng.ILocationProvider) => {


    $locationProvider.html5Mode(true);

    $httpProvider.interceptors.push('fr8VersionInterceptor');

    // Install a HTTP request interceptor that causes 'Processing...' message to display
    $httpProvider.interceptors.push(['$q', '$window', ($q: ng.IQService, $window: ng.IWindowService) => {
        return <any>{
            request: (config: ng.IRequestConfig) => {
                // Show page spinner If there is no request parameter suppressSpinner.
                if (config && config.params && config.params['suppressSpinner']) {
                    // We don't want this parameter to be sent to backend so remove it if found.
                    delete (config.params.suppressSpinner);
                }
                return config;
            },
            response: (config: ng.IRequestConfig) => {
                Metronic.stopPageLoading();
                return config;
            },
            responseError: (config) => {
                //Andrei Chaplygin: not applicable as this is a valid response from methods signalling that user is authorized but doesn't have sufficient priviligies
                //All unauthorized requests are handled (and redirected to login page) by built-in functionality (authorize attributes)
                if (config.status === 403) {
                    $window.location.href = $window.location.origin + '/Account/InterceptLogin'
                        + '?returnUrl=' + encodeURIComponent($window.location.pathname + $window.location.search);
                }
                Metronic.stopPageLoading();
                return $q.reject(config);
            }
        }
    }]);


    // Redirect any unmatched url
    $urlRouterProvider.otherwise("/myaccount");

    $stateProvider
        .state('myaccount',
        {
            url: "/myaccount",
            templateUrl: "/AngularTemplate/MyAccountPage",
            data: { pageTitle: 'My Account', pageSubTitle: '' }
        })
        // Plan list
        .state('planList',
        {
            url: "/plans",
            templateUrl: "/AngularTemplate/PlanList",
            data: { pageTitle: 'Plans', pageSubTitle: 'This page displays all Plans'}
        })

        .state('plan',
        {
            url: "/plans/{id}/builder?viewMode&view",
            views: {
                'header@': {
                    templateUrl: ($stateParams: ng.ui.IStateParamsService) => {
                        if ($stateParams['viewMode'] === 'kiosk') {
                            return "/AngularTemplate/KioskModeOrganizationHeader";
                        }
                    }
                },
                'maincontainer@': {
                    templateUrl: ($stateParams: ng.ui.IStateParamsService) => {
                        if ($stateParams['viewMode'] === 'kiosk') {
                            return "/AngularTemplate/MainContainer";
                        }
                        return "/AngularTemplate/MainContainer_AS";
                    },
                    controller: 'PlanBuilderController'
                },
                '@plan': {
                    templateUrl: ($stateParams: ng.ui.IStateParamsService) => {
                        if ($stateParams['viewMode'] === 'kiosk') {
                            return "/AngularTemplate/PlanBuilder_SimpleKioskMode";
                        }
                        return "/AngularTemplate/PlanBuilder";
                    }
                },
                'footer@': {
                    templateUrl: ($stateParams: ng.ui.IStateParamsService) => {
                        if ($stateParams['viewMode'] === 'kiosk') {
                            return "/AngularTemplate/Empty";
                        }
                        return "/AngularTemplate/Footer";
                    }
                }
            }
        })

        .state('plan.details',
        {
            url: "/details",
            views: {
                '@plan': {
                    templateUrl: "/AngularTemplate/PlanDetails"
                }
            },
            data: { pageTitle: 'Plan Details', pageSubTitle: '' }
        })

        .state('appList',
        {
            url: "/apps",
            templateUrl: "/AngularTemplate/AppList",
            data: { pageTitle: 'Apps', pageSubTitle: 'All the Apps created for your organization' }
        })  

        .state('showIncidents',
        {
            url: "/showIncidents",
            templateUrl: "/AngularTemplate/ShowIncidents",
            data: { pageTitle: 'Incidents', pageSubTitle: 'This page displays all incidents' }
        })
        .state('showFacts',
        {
            url: "/showFacts",
            templateUrl: "/AngularTemplate/ShowFacts",
            data: { pageTitle: 'Facts', pageSubTitle: 'This page displays all facts' },
        })


        // Manage files
        .state('managefiles',
        {
            url: "/managefiles",
            templateUrl: "/AngularTemplate/ManageFileList",
            data: { pageTitle: 'Manage Files', pageSubTitle: '' }
        })
        .state('fileDetail',
        {
            url: "/managefiles/{id}",
            templateUrl: "/AngularTemplate/FileDetails",
            data: { pageTitle: 'File details', pageSubTitle: '' }
        })
        .state('accounts',
        {
            url: '/accounts',
            templateUrl: '/AngularTemplate/AccountList',
            data: { pageTitle: 'Manage Accounts', pageSubTitle: '' }
        })
        .state('accountDetails',
        {
            url: '/accounts/{id}',
            templateUrl: '/AngularTemplate/AccountDetails',
            data: { pageTitle: 'Account Details', pageSubTitle: '' }
        })
        .state('containerDetails',
        {
            url: "/container/{id}/details",
            templateUrl: "/AngularTemplate/containerDetails",
            data: { pageTitle: 'Container  Details', pageSubTitle: '' }
        })
        .state('configureSolution',
        {
            url: "/solution/{solutionName}",
            controller: 'PlanBuilderController',
            templateUrl: "/AngularTemplate/PlanBuilder",
            data: { pageTitle: 'Create a Solution', pageSubTitle: '' }
        })
        .state('containers',
        {
            url: "/containers",
            templateUrl: "/AngularTemplate/ContainerList",
            data: { pageTitle: 'Containers', pageSubTitle: 'This page displays all Containers ' },
        })
        .state('webservices',
        {
            url: "/webservices",
            templateUrl: "/AngularTemplate/WebServiceList",
            data: { pageTitle: 'Web Services', pageSubTitle: '' }
        })

        .state('terminals', {
            url: "/terminals",
            templateUrl: "/AngularTemplate/TerminalList",
            data: { pageTitle: 'Terminals', pageSubTitle: '' }
        })
        .state("terminalDetails", {
            url: "/terminals/{id}",
            templateUrl: "/AngularTemplate/TerminalDetail",
            data: {pageTitle: 'Terminal Details', pageSubTitle: ''}    
        })
        .state('manageAuthTokens',
        {
            url: '/manageAuthTokens',
            templateUrl: '/AngularTemplate/ManageAuthTokens',
            data: { pageTitle: 'Manage Auth Tokens', pageSubTitle: '' }
        })
        .state('changePassword',
        {
            url: '/changePassword',
            templateUrl: '/AngularTemplate/ChangePassword',
            data: { pageTitle: 'Change Password', pageSubTitle: '' }
        })
        .state('reports',
        {
            url: "/reports",
            templateUrl: "/AngularTemplate/PlanReportList",
            data: { pageTitle: 'Reports', pageSubTitle: 'This page displays all Reports' }
        })
        .state("pageDefinitions",
        {
            url: "/page_definitions",
            templateUrl: "/AngularTemplate/PageDefinitionList",
            data: { pageTitle: "Manage Page Definitions", pageSubTitle: "" }
        })
        .state("adminTools",
        {
            url: "/admin_tools",
            templateUrl: "/AngularTemplate/AdminTools",
            data: { pageTitle: "Admin tools", pageSubTitle: "" }
        });

}]);

/* Init global settings and run the app */
app.run(["$rootScope", "settings", "$state", function ($rootScope, settings, $state) {
    $rootScope.$state = $state; // state to be accessed from view
}]);

app.constant('fr8ApiVersion', 'v1');

app.factory('fr8VersionInterceptor', ['fr8ApiVersion', (fr8ApiVersion: string) => {
    var apiPrefix: string = '/api/';
    return {
        'request': (config: ng.IRequestConfig) => {
            //this is an api call, we should append a version to this
            if (config.url.indexOf(apiPrefix) > -1) {
                config.url = config.url.slice(0, 5) + fr8ApiVersion + "/" + config.url.slice(5);
            }
            return config;
        }
    };
}]);


app.config(['ivhTreeviewOptionsProvider', ivhTreeviewOptionsProvider => {
    ivhTreeviewOptionsProvider.set({
        twistieCollapsedTpl: '<span class="glyphicon glyphicon-chevron-right"></span>',
        twistieExpandedTpl: '<span class="glyphicon glyphicon-chevron-down"></span>',
        twistieLeafTpl: '',
        defaultSelectedState: false
    });
}]);

//We delay application bootstrapping until we load activity templates from server

var bootstrapModule = angular.module('activityTemplateBootstrapper', []);
// the bootstrapper service loads the config and bootstraps the specified app
bootstrapModule.factory('bootstrapper', ['$http', '$log','$q', ($http: ng.IHttpService, $log: ng.ILogService, $q: ng.IQService) => {
    return {
        bootstrap: (appName) => {
            var deferred = $q.defer();
            $http.get('/api/v1/activity_templates/')
                .success((activityTemplates: Array<dockyard.interfaces.IActivityCategoryDTO>) => {
                    // set all returned values as constants on the app
                    var myApp = angular.module(appName);
                    myApp.constant('ActivityTemplates', activityTemplates);
                    angular.bootstrap(document, [appName]);
                    deferred.resolve();
                })
                .error(() => {
                    $log.warn('Could not initialize application, activity templates could not be loaded.');
                    deferred.reject();
                });
            return deferred.promise;
        }
    };
}]);
// create a div which is used as the root of the bootstrap app
var appContainer = document.createElement('div');
bootstrapModule.run(['bootstrapper',(bootstrapper) => {
    bootstrapper.bootstrap('app').then(() => {
        // removing the container will destroy the bootstrap app
        appContainer.remove();
    });
}]);
// make sure the DOM is fully loaded before bootstrapping.
angular.element(document).ready(() => {
    angular.bootstrap(appContainer, ['activityTemplateBootstrapper']);
});