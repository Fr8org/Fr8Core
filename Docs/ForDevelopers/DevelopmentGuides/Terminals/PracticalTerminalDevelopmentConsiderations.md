Practical Terminal Development Considerations
============================================


Managing Overlap
-----------------

One of our goals, particularly in the first year of Fr8, is to expand the breadth of Terminal and Activity coverage, and go from dozens of Terminals to hundreds.
In an ideal world, one might envision a quarter of these being written in C#, a quarter in Ruby, etc...
There would be no overlap: the annointed Github Terminal would be in Java and the annointed Trello Terminal would in Ruby, etc. This would
prevent duplication of effort created, for example, if four different developers craft a Github Terminal in four different languages.

However, we expect reality to be much messier. We envision scenarios like this:
dev A: I built a cool Terminal in Go and want to create a Plan that incorporates the Github "Get Issue" Activity, but it doesn't have a property I need, and I don't
want to write Java. 
dev A: Ok, I will post a Github issue requesting that the java guys improve that activity
dev A: Hmm, several days have passed. F#@# it, I'm going to write my own Github terminal in Go. 
dev A: My fine Go Github Fr8 Terminal is done, and I now want to get the Fr8.co guys to deploy it so I can use it

One of Fr8's core philosophies is to Embrace Mess. We anticipate that there will be multiple versions of a Terminal, offering similar but not-identical functionality
in multiple languages. We think that's a good thing. We acknowledge that it will create some management and UI challenges. We point out that mobile App stores
have similar problems, as do search engines. We blithely wave our hands and speak vaguely about reputation systems, ranking, categorization, and filters. Our audience perhaps not fully convinced, we nonetheless head for the bar.

While Mess is inevitable, some practical conventions will assist:

If you are working on a Terminal, make sure you create a Github Issue in the appropriate repo, with a name like "Building Foobario Terminal in Ruby"
If you are planning on starting a Terminal, take a brief look at the other language repos to see if someone is working on it already
Never feel like you can't build a Terminal just because it exists in another language. 

If a Terminal does exist, consider adding Activities to it rather than starting from scratch. 

