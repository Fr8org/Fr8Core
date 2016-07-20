# ACTIVITIES – REGISTRATION

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

## The Basics

Activities are hosted by [Terminals](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Terminals.md). When a Fr8 [Hub](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Fr8Hubs.md) starts up, it asks each Terminal that it is working with to provide it with up-to-date information on available Activities. This makes it easy for Terminals to upgrade and enhance Activities, and bring new Activities online, without requiring changes to the Hub.

## Going Deeper

Each Activity is described via an ActivityTemplate.

## For Developers

At Startup the Hub  looks in its root directory for a host file called fr8terminals.txt. This file has URLs in it. The Server reads in the urls and calls GET /discover to each one. The entries in this host file correspond to terminals. So for example, one entry might be fr8.co/terminals:46281, which would be a running instance of an AzureSqlServer terminal.

Each terminal responds with some information about themselves, and an ActivityTemplate for each Action they support.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
