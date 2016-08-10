SDK Development Guide
============================

When someone creates a new SDK for Fr8, they basically are building 3 things: A Terminal,
an Activity in that Terminal, and some amount of base class support to minimize the amount of work that has to be done inside the Terminal itself. 

One of the core tents of Fr8losophy is "Fr8 Terminal Designers should do as little work as possible" because so much heavy lifting is handled by 
base classes and helpers in the platform-specific SDK.

As of this writing, there's tremendous disparity in the quality and depth of the available SDK resources. The .NET SDK is really rich because
we did all the original coding in C# so we could tap into a huge developer base. The other SDKs are just getting going. 

If you are extending Fr8 to a new platform by building the first Terminal in that language, you're de facto creating the SDK for that language. 
Let us know about it via a Github Issue and a Slack Channel. We're eager to support you. 
