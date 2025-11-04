rdfs:comment "Specialization of stockFlow in which an Economic Event decreases the provider's control over an Economic Resource that is transferred, consumed, or used." ;

# give

    **rdfs:label:** give  
    **rdf:type:** owl:ObjectProperty  
    **rdfs:subPropertyOf:** stockFlow  
    **rdfs:domain:** Economic Event  
    **rdfs:range:** Economic Resource

    ## Definition
    Specialization of stockFlow in which an Economic Event decreases the provider's control over an Economic Resource that is transferred, consumed, or used.

    ## Notes
- Typical of outflow events on the giving side of an exchange.
- Used for delivery, payment, or other relinquishments.
- **Event effect:** decrement of the linked resource.

## Related
    - `stockFlow` (super-property)
    - `duality` (pairs increment/decrement events)
    - `Economic Event` ↔ `Economic Resource` (via this property)

    _Last updated: 2025-11-04_
