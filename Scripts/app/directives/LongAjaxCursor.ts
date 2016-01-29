/// <reference path="../_all.ts" />
module dockyard.directives.longAjaxCursor {
    'use strict';

    const LOADING_CSS = 'long-ajax-cursor';
    const LOADING_TIMEOUT = 800; // Max time between first request and last response
    const WAIT_TIMEOUT = 300; // Max time between consiquent requests to consider them a set

    var instances = [];
    

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class LongAjaxCursor implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public onLoadStart: () => void;
        public onLoadFinish: () => void;
        public restrict = 'A';

        private $element: ng.IAugmentedJQuery;

        constructor(private $http: ng.IHttpService) {
            LongAjaxCursor.prototype.link = (
                scope: ng.IScope,
                $element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                instances.push(this);
                this.$element = $element;

                $element.on('$destroy', () => {
                    var index = instances.indexOf(this);
                    if (index >= 0) instances.splice(index, 1);
                });
            }

            var requestTimer, waitTimer;
            this.onLoadStart = () => {
                if (!requestTimer) requestTimer = setTimeout(() => {
                    requestTimer = null;
                    addLoadingClass();
                }, LOADING_TIMEOUT);

                clearTimeout(waitTimer);
            };

            this.onLoadFinish = () => {
                if ($http.pendingRequests.length === 0) {
                    clearTimeout(waitTimer);
                    waitTimer = setTimeout(() => {
                        clearTimeout(requestTimer);
                        requestTimer = null;
                        removeLoadingClass();
                    }, WAIT_TIMEOUT);
                }
            };

            var addLoadingClass = () => {
                this.$element.addClass(LOADING_CSS);
            };

            var removeLoadingClass = () => {
                this.$element.removeClass(LOADING_CSS);
            };

        };

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($http: ng.IHttpService) => {
                return new LongAjaxCursor($http);
            };

            directive['$inject'] = ['$http'];
            return directive;
        }
    }

    app.config(['$httpProvider', ($httpProvider) => {
        $httpProvider.interceptors.push(['$q', '$window', ($q: ng.IQService, $window: ng.IWindowService) => {
            return {
                request: function (config: ng.IRequestConfig) {
                    instances.forEach((instance) => {
                        instance.onLoadStart();
                    });
                    return config;
                },
                response: function (config: ng.IRequestConfig) {
                    instances.forEach((instance) => {
                        instance.onLoadFinish();
                    });
                    return config;
                },
                responseError: function (config) {
                    instances.forEach((instance) => {
                        instance.onLoadFinish();
                    });
                    return $q.reject(config);
                }
            }
        }]);
    }]);

    app.directive('longAjaxCursor', LongAjaxCursor.Factory());
}