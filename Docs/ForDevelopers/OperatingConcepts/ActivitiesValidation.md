# Validation
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)
## Summary
Every time when Hub configures activity or activates it prior to running a plan which results in respective calls to terminal's `activities\configure` and `activities\activate` endpoints, activity gets a chance to report Hub any validation errors it has. 

Hub parses the crate list returned by activity and looks for `ValidationResultCM` manifest. If it founds one it checks for its `validationErrors` property value. If this value contains at least one error message then Hub marks related activity UI controls with specified error messages and in case of `activities\activate` method it stops further processing thus plan doesn't move to `Active` status and its activities don't get executed.
## ValidationResultCM manifest structure
More information on what is  manifest can be found [here](http://documentation.fr8.co/developers/objects/crates-manifest/)

Below is the sample contents of `ValidationResultCM` manifest

    {  
      "validationErrors":[  
         {  
            "controlNames":[  
               "TemplateSelector"
            ],
            "errorMessage":"Template was not selected"
         }
      ]
   }
   
   
## Validate Method in Activity Classes
   The Validate method in Activity classes gets called upon every followup /configure call
   unless *DisableValidationOnFollowup* variable setted up to *true* inside Activity constructor.
