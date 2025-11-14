rdfs:comment "A Business Process is a coordinated set of Business Events that collectively support a major step in the enterprise’s operations, transforming inputs into outputs and contributing to value creation." ;

# Business Process

**rdfs:label:** Business Process  
**rdf:type:** owl:Class

## Definition
A Business Process is a coordinated set of Business Events that collectively support a major step in the enterprise’s operations, transforming inputs into outputs and contributing to value creation.

## Intended Meaning
Business Processes sit above workflows in the REA granularity hierarchy and represent major operational segments such as *acquisition/payment*, *conversion*, *revenue*, *fulfillment*, etc.

## Key Characteristics
- Composed of one or more **Workflows**.  
- Represents a major operational segment of the value chain.  
- Includes multiple Business Events arranged into procedural structure.  
- Connected to REA state-machine phases (planning → identification → negotiation → actualization → post-actualization).

## Related
- `Workflow` (subunit of a Business Process)  
- `ValueChain` (Business Processes are the middle layer)  
- `EconomicEvent` (results from/in the process)

_Last updated: 2025-11-14_
