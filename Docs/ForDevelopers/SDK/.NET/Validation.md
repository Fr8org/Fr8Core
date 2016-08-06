# Validation

See [here](/Docs/ForDevelopers/OperatingConcepts/ActivitiesValidation.md) for general information about validation. 

In .NET SDK validation if performed by [ValidationManager](/Docs/ForDevelopers/SDK/.NET/Reference/ValidationManager.md) service that is accessible from **Validate** method in any activity derived from [TerminalActivityBase](/Docs/ForDevelopers/SDK/.NET/Reference/TerminalActivityBase.md).

To implement validation logic you have to override **Validate** method. Default implementation in the .NET SDK base classes is empty so you don't have to invoke it in the overridden method.

Here is how typical implementation of validation logic looks like:
```c#
protected override Task Validate()
{
	if (string.IsNullOrEmpty(ActivityUI.ChannelSelector.Value))
	{
		ValidationManager.SetError("Channel or user is not specified", ActivityUI.ChannelSelector);
	}

	ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.MessageSource, "Can't post empty message to Slack");

	return Task.FromResult(0);
}
```

In .NET SDK validation code is invoked not only during processing of */configure* and */activate* requests, but also before activity execution. In case of errors, activity execution will be terminated and the error will be returned to the Hub. The error text is composed from validation error messages in the human readable form. Such approach allows you to have one common place with validation logic. 

> **Important!**  
> Never duplicate validation logic inside the **Run** method. 

## Dealing with upstream values

Some UI controls can be configured by the user to use values from upstream instead of explicit values. It is important to correctly handle such cases. For example, if you want to check if the *TextSource* has a valid e-mail value you can't just write:  
```c#
protected override Task Validate()
{
	if (ValidateEMail(ActivityUI.EMail.TextValue))
	{
		ValidationManager.SetError("Invalid e-mail", ActivityUI.EMail);
	}

	return Task.FromResult(0);
}
```

because if *ActivityUI.MessageSource* is configured to use upstream value then *TextValue* will be empty during the design-time and you will generate false validation error. You should write a bit more complex logic instead:  
```c#
protected override Task Validate()
{
	if (!ActivityUI.EMail.HasUpstreamValue && ValidateEMail(ActivityUI.EMail.TextValue))
	{
		ValidationManager.SetError("Invalid e-mail", ActivityUI.EMail);
	}

	return Task.FromResult(0);
}
```  
Here we will throw validation error only if there is no upstream value set and *TextValue* is not a valid e-mail. 


## Dealing with run-time
You may wonder how our validation check will work during the run-time in case of upstream value source: *ActivityUI.EMail.HasUpstreamValue* is true, so *ValidateEMail* will not be called, and even if is called somehow then *TextValue* is empty and the check will fail. The answer is simple. During the run-time the Hub will extract actual value from the payload and assign this value to *TextValue* and reconfigure this TextSource control as if is using explicit value. In other words, your activity will think that *TextSource* was configured to use explicit value. So our validation logic will be executed without any issues in all possible cases.

> **Important**:   
> There is no need to implement any special handling for run-time validation scenario.

## Extending ValidationManager
In general, if you find that you write the same validation logic again and again in your activities or you want to make validation logic resuable, consider creation of extension method for **ValidationManager**. Here is how our e-mail validation example can be rewritten:

```c#
public static class MyValidationExtensions
{
	public static void ValidateEmail(this ValidationManager validationManager, TextSource textSource, string errorMessage = null)
	{
		if (!textSource.HasUpstreamValue && ValidateEMail(textSource.TextValue))
		{
			validationManager.SetError(errorMessage ?? "Invalid e-mail", textSource);
		}
	}
}
```


```c#
protected override Task Validate()
{
	ValidationManager.ValidateEmail (ActivityUI.EMail);

	return Task.FromResult(0);
}
```  