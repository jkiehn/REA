rdfs:comment "Relates a Commitment or Economic Event to the Type (EventType or ResourceType) that characterizes it or its associated resource." ;

# specifies

**rdfs:label:** specifies  
**rdf:type:** owl:ObjectProperty

## Definition
Relates a Commitment or Economic Event to the Type (EventType or ResourceType) that characterizes it or its associated resource.

## RDFS
- **rdfs:domain:** Commitment | Economic Event  
- **rdfs:range:** EventType | ResourceType

## Notes
- Use as a direct link for simple schemas.  
- When you need metadata on the instance↔type link itself (e.g., validity periods, provenance), use `Typification` as a reified class.

## Related
- `Typification` (reified alternative)
- `Commitment`, `Economic Event`, `EventType`, `ResourceType`

_Last updated: 2025-11-04_
