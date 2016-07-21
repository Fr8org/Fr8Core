

Communicating With a Hub
------------------------

As you develop a Terminal you'll want to connect it to a Hub so you see your Activity in client UI and receive calls from the Hub.

The most straightforward way to do this is simply to register your developmental Terminal with the production Hub at fr8.co. 
This can be done at INSERT WEB URL. The Hub will post a /discover request to your Terminal, and if you respond to it properly, your Activities
will show up in your Client as selectable items. (If you need to poke the Hub to repeat the call, you can do that with a request to [this endpoint](http://dev.fr8.co/swagger/ui/index#!/Terminals/Terminals_ForceDiscover).

When you then add your Activity to a Plan, a /configure call will be generated to your Terminal, and you're off and running.

If you want greater insight into what's happening on the Hub, you can run a Hub in your local development environment. (As of this writing,
