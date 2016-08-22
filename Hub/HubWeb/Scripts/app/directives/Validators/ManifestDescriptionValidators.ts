module dockyard.directives.validators {
    'use strict';

    export class UniqueVersionName implements ng.IDirective {

        // static $inject = ['$q', 'ManifestRegistryService'];
        static instance($q: ng.IQService, ManifestRegistryService: dockyard.services.IManifestRegistryService): ng.IDirective {
            var directive = ($q: ng.IQService, ManifestRegistryService: dockyard.services.IManifestRegistryService) => new UniqueVersionName($q, ManifestRegistryService);
            directive.$inject = ['$q', 'ManifestRegistryService'];
            return directive;
        }

        restrict = 'A';
        require = 'ngModel';
        link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes, ngModel) => void;

        constructor(private $q: ng.IQService, private ManifestRegistryService: dockyard.services.IManifestRegistryService) {
            this.link = this._link.bind(this);
        }


        _link(scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes, ngModel) {
            
            ngModel.$asyncValidators.uniqueVersionName = function (modelValue) {
                return this.$q(function (resolve, reject) {
                    this.ManifestRegistryService.checkVersionAndName(modelValue).then(function (result) {
                        ngModel.$setValidity('unique-version-name', result.data);
                        if (result.data) {
                            resolve();
                        } else {
                            reject();
                        }
                    });
                });
            }
        }
    }
}