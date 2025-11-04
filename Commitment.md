rdfs:comment "A promise or obligation by an Economic Agent to perform an Economic Event in the future under specified terms." ;

# Commitment

**rdfs:label:** Commitment  
**rdf:type:** owl:Class

## Definition
A promise or obligation by an Economic Agent to perform an Economic Event in the future under specified terms.

## Intended Meaning
Commits agents to future increments or decrements of resources; later fulfilled (fully or partially) by actual Economic Events.

## Key Characteristics
- References the prospective Economic Event type and terms (e.g., quantity, price, due date)
- Can be reciprocal with another Commitment
- Can be partially or fully fulfilled by one or more Economic Events

## Related
- `specifies` (Commitment → EventType/Resource/Terms)
- `reciprocity` (Commitment ↔ Commitment)
- `fulfills` (Economic Event → Commitment)

_Last updated: 2025-11-04_
