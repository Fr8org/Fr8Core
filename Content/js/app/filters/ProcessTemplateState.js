'use strict';

MetronicApp.filter('ProcessTemplateState', function() {
    return function(input) {
        switch (input)
        {
            case 1:
                return "Active";
                break;
            case 0:
                return "Inactive";
                break;
            default:
                return "Inactive";
        }
    };
})