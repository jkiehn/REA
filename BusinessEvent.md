rdfs:comment "A Business Event is a task or activity at a level of detail below the Economic Event, representing the procedural steps that management wishes to plan, control, and evaluate as part of workflow execution." ;

# Business Event

**rdfs:label:** Business Event  
**rdf:type:** owl:Class

## Definition
A Business Event is a task or activity at a level of detail below the Economic Event, representing the procedural steps that management wishes to plan, control, and evaluate as part of workflow execution.

## Intended Meaning
Business Events represent the fine-grained procedural actions that make up a **workflow**, such as *receive goods*, *inspect goods*, *prepare invoice*, *approve payment*, etc.  
They do **not** directly affect resource/agent claims — that role belongs to Economic Events.

## Key Characteristics
- Lowest-level element of workflow.  
- Procedural activity rather than an economic change.  
- Can be modeled in **BPMN 2.0** for execution semantics.  
- Is part of one Workflow.

## Related
- `Workflow` (aggregates Business Events)  
- `EconomicEvent` (higher-level economic occurrence)

_Last updated: 2025-11-14_
