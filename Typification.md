rdfs:comment "A reified association that links an instance (e.g., an Economic Event or Economic Resource) to a corresponding Type (EventType or ResourceType), optionally with role-specific metadata." ;

# Typification

**rdfs:label:** Typification  
**rdf:type:** owl:Class

## Definition
A reified association that links an instance (e.g., an Economic Event or Economic Resource) to a corresponding Type (EventType or ResourceType), optionally with role-specific metadata.

## Intended Meaning
Makes the instance↔type link explicit as a first-class object so additional properties (e.g., effective dates, policy references) can be attached.

## Related
- `specifies` (shortcut property when reification is not required)
- `EventType`, `ResourceType` (the target classifications)

_Last updated: 2025-11-04_
