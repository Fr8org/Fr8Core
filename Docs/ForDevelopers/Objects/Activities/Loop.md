# Loop activity
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

Loop activity is used to iterate throught the crates data. Once the crate is selected Loop activity signals a copy of a selected crate with a name "Row of {Name of selected crate}". Loop activity also signals the same fields that are signaled for the selected crate. At execution Loop activity does following:

1. Loop activity finds an "upper" collection in the selected crate structure and determines the collection size, then it creates or increments an iteration index in Operational Status crate. If index is larger then the size of a collection - loop activity sends a `SkipChildren` Activity Response to Hub. 
2. After that it clones the selected crate with a name "Row of {Name of selected crate}" and replaces collection elements with a single element according to current iteration index. After that cloned crate is put into the Payload and execution scope goes back to Hub.
3. After Loop's children activities execution is completed Hub sends loop activity a run call with a parameter "scope" set to "childActivities". Upon that Loop activity sends back `JumpToActivity` ActivityResponse, with it's own Id as a parameter. Which makes the execution go back to point #1