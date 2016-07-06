Best Practices for Developing with Crates

1. Use Labels, and use them well
In medium complexity Plans, the user may end up choosing from a large set of upstream data sources. It's important to be able to distinguish between them
The Label property of Crates is designed for this. Choose a Label that is evocative.

![alt text](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/labels.png "This isn't actually a fabulous example. Could be clearer.")


2. When generating payload at run-time, consider duplicating your data and providing it in both high-level and low-level forms

Consider a real world example that recently occured. A developer was building the first Asana Terminal and, as part of it, the first Asana Activity "Get Tasks". It's a simple Activity that (in its first implementation) just returns Task Names and IDs. The developer now had to decide how to write that data into the Payload Container. They wanted to accomplish a couple of different goals. They wanted, on the one hand, to provide the data in a high-level structure that would be useful to other future activities that look to manipulate Asana Tasks. But they also were cognizant of the fact that none of the existing activities, including critical tools such as Loop and Save to Excel, have any idea what an "Asana Task" is,and wouldn't know how to process JSON in that form. 

So, they decided to create two different crates. One was a crate of table data, using the StandardTableDataCM. This is a popular manifest, used by pretty much all the activities capable of doins something useful with lists of data. But for the other crate, they registered a new manifest called AsanaTasksCM, and stored the data in that format. This was essentially an investment for the future, a pioneer setting up the first homestead in a new wild area. 

In general, Fr8 encourages this kind of unDRY data behavior, because it's important to make life easy for users.
