rdfs:comment "Phenomena that reflect changes in scarce means (economic resources) resulting from production, exchange, consumption, and distribution." ;

# Economic Event

**rdfs:label:** Economic Event  
**rdf:type:** owl:Class

## Definition
Phenomena that reflect changes in scarce means (economic resources) resulting from production, exchange, consumption, and distribution.

## Intended Meaning
Represents occurrents that cause increments or decrements of Economic Resources.

## Key Characteristics
- Has time (timestamp) and amount attributes in applications
- Connected to resources via `stockFlow`
- Paired with a requited event via `duality` (increment/decrement roles)

## Related
- `stockFlow` (to Economic Resource)
- `duality` (to another Economic Event)
- `insideParticipate` / `outsideParticipate` (to Economic Agent)

## Source
REA Monograph (v0.90, 2019), Chapter 1, "The Basic Elements of the REA Ontology".

_Last updated: 2025-11-04_