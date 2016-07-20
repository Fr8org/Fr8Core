Roadmap


Planned Work Items

- Smarter Dependency Tracking
Right now, when an Activity is configured, the system calls configure on all downstream Activities to make sure there are no broken references. Replace
that with a smarter approach that tracks dependencies between Activities and only calls configure on downstream dependencies.

--Separate out Plan Execution for better scaling
Current architecture will make hard to scale to accommodate the growing load. We can fully decouple plan execution from plan configuration + UI support logic. Linkage between two parts must be as simple as possible (for example POST with plan JSON + optional payload). This will hit several goals at once:
Will make possible to scale UI and plan configuration logic on per-user basis (each ASP session is binded to the certain node)
Will make possible to scale heavy plan execution logic on per-container basis. We can even implement a variant of plan execution core using free platforms like Java or NodeJS.
Can solve problem when executing plan is being editing. 
Plan execution nodes can have simplified authentication logic: it doesn't have to deal with sessions, or cookies, so we can avoid wasting resources on ASP authentication DB calls during execution.
Splitting the core into two independent systems will make each one more clear and simple, thus make them easier for understanding.Wasting time on ASP authorization-related DB calls on every API access.


-- Address  concurrency issues.  

-- Refactor solution configuration to make it more reliable and atomic.

-- A LOT of sync DB calls related to logging. Logging should be transparent. All logging (incidents and facts are not an exception) should be done in-parallel with main code execution.

-- Fr8 Warehouse (Multi-Tenant Storage)
    Lack of versioning support in MT.
    Lack of indexes in MT (we can search by any primitive field, but can't create DB indexes on the  selected fields).
    Lack of full-support of complex objects (currently we just serialize all complex objects to JSON string).
 
 
