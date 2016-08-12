# Part 1 – Introduction  

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

[Basic introduction to Fr8 concepts](http://documentation.fr8.co/how-it-works/) .  

Fr8 is a fully distributed application environment designed from the ground up to specialize in connecting existing cloud services. End Users will mostly experience Fr8 through prebuilt solutions that make cloud integration problems vanish. Power Users will mostly experience Fr8 through the point-and-click Plan Builder, the easiest and most powerful tool for assembling cloud business processes.  

For developers, though, there’s a vast subterranean world to benefit from. Fr8 is the first cloud integration system that is built from the ground up for fully distributed operation. Each Activity in a Plan can be hosted, operated, upgraded, modified, and secured by a different Terminal running on a different platform in a different physical location and written in a different language. All communication is done through simple HTTP endpoints, and all data is JSON. This creates a system with many advantages.  

### FR8 IS PORTABLE  

We here at The Fr8 Company certainly hope that many people use our Hub at fr8.co to run their Plans. But we expect and anticipate that others will run Hubs too. We’re publishing the Fr8 Hub spec and we’ve deliberately kept it as simple as possible to make it easy to process Activities and Containers.  

### FR8 IS OPEN    

Building a Terminal is easy. Only a handful of endpoints need to be supported and we’re making source code available.  

Once a Terminal is up and running, the Actions that it exposes can be made public and it can be hooked into existing Hubs. This allows the Terminal builder to integrate their own Actions with the publicly available Actions. Put another way, if you expose some of your system’s API using a Fr8 Terminal, you get to automatically tie in everything that can be done on Fr8, ranging from sending notifications to carrying out middleware crunching of data, to more.  

This architecture also allows Terminals to be very nuanced. There might end up being, for example, 10 different open-source, publicly available Salesforce.com Fr8 Terminals. Some might focus their Activities on ease-of-use, while other Terminal builders might emphasize activities for power users. Each Fr8 Hub determines which Terminals it wants to coordinate with and make available to its Fr8 users.  

### FR8 IS EXTENSIBLE  

The Fr8 Hub has no knowledge about the Fr8 Terminals it works with, beyond what it discovers when it starts up. That means that individual Terminals and their Activities can be upgraded, patched, and extended independently of the Hubs and client code.  

[Part 2 – Understanding the Fr8 Architectural Model](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/ArchitecturalModel.md)  

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
