# PART 3 – THE FR8 DATA MODEL  

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/docs/Home.md)  
### Crates and Containers: Where Data Lives

Fr8 does not use a central database. Instead, data lives in Crates, which are stored in Containers. When a Hub executes a Plan, it creates a Container to hold the user’s data and routes the Container as it travels from Activity to Activity. The Container is handed off to the next Terminal, which carries out processing, updates the Crates in the Container, and returns the Container to the Hub. All Crate and Container data is represented in JSON. Here’s an example of a Crate containing a single DocuSign Envelope:  
  ![Crate](https://github.com/Fr8org/Fr8Core/blob/master/docs/img/DataModel_Crate.png) 

The Manifest information refers to a shared registry of Crate specifications. This allows Activity A to store data in a Crate using a particular manifest, and know that unknown Activity B, downstream, will be able to look for that Crate and effectively process it.  

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/docs/Home.md)  