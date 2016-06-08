# PART 2 – UNDERSTANDING THE FR8 ARCHITECTURAL MODEL

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
### Hubs and Terminals: Where Work Gets Done  

Fr8 Plans are assembled out of individual Activities. The Web Services that host and process Fr8 Plans are called Fr8 Hubs. The Fr8 Company operates a major Hub at www.fr8.co, but anyone can run their own Hub by creating a web service that supports the [Fr8 Hub Specification.](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Specifications/Fr8HubSpecification.md)  

When a Hub is ready for the next Activity in the Plan to be executed, it makes a call to the Activity’s Fr8 Terminal. Terminals carry out Activity processing. Many Terminals focus on a single web service. For example, in the Plan above, the Activity “Get Google Sheet Data” will be executed by the Fr8 Google Terminal, the Activity “Send DocuSign Envelope” will be executed by the Fr8 DocuSign Terminal, and the Activity “Loop” will be execute by the Fr8 Core Terminal.  

### Fr8 Workflow  

![Main Flow](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/ArchitecturalModel_MainFlow.png)

[Part 3 – The Fr8 Data Model](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DataModel.md)  

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
