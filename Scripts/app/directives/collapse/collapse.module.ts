namespace dockyard.modules {
    import collapseHeading = dockyard.directives.collapseHeading;
    import collapseContent = dockyard.directives.collapseContent;
    
    angular.module('fr8.collapse', [])
        .directive('fr8CollapseHeading', collapseHeading)
        .directive('fr8CollapseContent', collapseContent);
}
