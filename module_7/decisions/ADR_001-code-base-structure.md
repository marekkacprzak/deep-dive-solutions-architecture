# Structure of initial code base

## Status

Accepted

## Context

*What is the issue that we're seeing that is motivating this decision or change?*

As a startup, we need to build the first version of the application quickly. We also don't fully understand the boundaries of the system components. To get started though, we need to decide whether we are going to target a monolithic code base or go all in on microservices. We expected relatively rapid growth, both in team size and customer base but that is unknown at the moment.

## Decision

*What is the change that we're proposing and/or doing?*

Build a single process monolith, but ensure strict adherence to module boundaries and a ports and adapters architecture style

## Consequences

*What becomes easier or more difficult to do because of this change?*

- Changes in direction become easier with a monolithic code base, if we realise something is slightly wrong or a module boundary isn't quite right we can pivot quickly
- Simple deployment model, single containerized application is easy to run on any cloud provider or infrastructure
- Strict modularity and adherence to ports and adapters makes evolvability easier in the future
- **If** the teams and customer base grow as we expect the single code base could start to cause problems quickly