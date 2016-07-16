[Back](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PlatformIdependentTerminalDeveloperGuide.md)

## Choosing a Development Approach

There are two ways you can develop a Fr8 Terminal:

### 1) The "Local Hub" Approach

Build and run a local Hub and a set of local Terminals on your own Development machine. Test your new Terminal in conjunction with those local resources.

Pros:
Maximum debug visibility into the Terminal <--> Hub Conversation
No need to worry about establishing a publicly-visible IP so the public Hubs can see your development Terminal

Cons:
Hub code is currently only available in C#/.NET
Building and running a Hub is non-trivial

### 2) The "Public Hub" Approach

Test your development Terminal against the public fr8.co Hub by registering it for discovery

Pros: 
Lightest local footprint
Can Test against all public Activities without having to start and manage their Terminals
SDK's exist for Java, Ruby, .NET and Python.

Cons:
Need to establish a public IP that can be seen from the production Fr8.co Hub
Not as much debug visibility
