'use strict';

/*
    The filter converts numeric value to state name
*/
app.filter('ProcessTemplateState', ['ConstantsService', function (ConstantsService) {

    return function(input) {
        switch (input)
        {
            case ConstantsService.ProcessTemplateState.Active:
                return "Active";
                break;
            case ConstantsService.ProcessTemplateState.Inactive:
                return "Inactive";
                break;
            default:
                return "Inactive";
        }
    };
}])