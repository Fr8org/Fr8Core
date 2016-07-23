# StackLocalData

Stack-local data for the current stack frame.  

**Namespace**: Fr8.Infrastructure.Data.Manifests.ActivityCallStack  
**Assembly**: Fr8Infrastructure.NET

## Properties
| Name                            |Description                                                                          |
|---------------------------------|------------------------------------------------------------------------------------ |
| Type| Gets or sets the type of the data. |
| Data| Gets or sets the data in the form of [JToken](http://www.newtonsoft.com/json/help/html/t_newtonsoft_json_linq_jtoken.htm). |

## Methods
| Name                            |Description                                                                          |
|---------------------------------|------------------------------------------------------------------------------------ |
| ReadAs\<T>() | Reads stack-local data and tries to deserialize the data into type **T**. If no data is set then default value of **T** is returned. |

## Remarks
There is no restrictions on what values **Type** property can has. You can set **Type** to anything you need to decide how to work with **Data** content. If you have the only possible content, you can set **Type** to **null**.