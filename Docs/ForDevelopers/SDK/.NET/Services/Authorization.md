
This provides platform-specific information related to Authorization [Error Handling](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Authorization/ErrorHandling.md)

There are two possible ways to do it depending on whether your action uses the EnhancedTerminalActivity<T> base class or not.  In both cases, this involves throwing the TerminalBase.Errors.AuthorizationTokenExpiredOrInvalidException in the code which makes API call to the 3rd party service. If you do not pass any specific message while creating this exception, it will cause the standard message to display to User in the Activity Stream:

[Dockyard Development Wiki > How to Handle Authentication and Authorization Errors in Terminal Actions > image2016-5-3 23:30:56.png]

This message should be fine for most cases, and if reauthorizing of the 3rd party service does not involve any specific actions, you should not pass any other message to the exception. However, if you do pass the message, you should be aware that it will be displayed to User as-is. Therefore, you should not pass any technical or debugging information. Also, you should get any custom error message approved by Alex.  

 The action inherits from EnhancedTerminalActivity<T>

1. Determine how your 3rd party service SDK reports authorization token issues.
2. When such case is detected (usually, in a Service class, not directly in the Activity class), throw TerminalBase.Errors.AuthorizationTokenExpiredOrInvalidException
3. Make sure that exceptions in the calling code are not suppressed and the exception, when raised, can be bubbled up to EnhancedTerminalActivity<T>.

The action inherits from BaseTerminalActivity

1. Determine how your 3rd party service SDK reports authorization token issues. 
2. When such case is detected (usually, in a Service class, not directly in the Activity class), throw TerminalBase.Errors.AuthorizationTokenExpiredOrInvalidException
3. In the Activity class add the code which intercepts the exception and returns the appropriate response to the Hub.  

```
    try 
    {
        // code which calls service class method
    }
    catch (TerminalBase.Errors.AuthorizationTokenExpiredOrInvalidException ex) 
    {
        return InvalidTokenError(payloadCrates);
    }
```

