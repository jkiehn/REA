rdfs:comment "Specialization of stockFlow in which an Economic Event uses up an Economic Resource so that it no longer exists in its prior form." ;

# consume

    **rdfs:label:** consume  
    **rdf:type:** owl:ObjectProperty  
    **rdfs:subPropertyOf:** stockFlow  
    **rdfs:domain:** Economic Event  
    **rdfs:range:** Economic Resource

    ## Definition
    Specialization of stockFlow in which an Economic Event uses up an Economic Resource so that it no longer exists in its prior form.

    ## Notes
- Common in conversion processes (e.g., consuming raw materials).
- **Event effect:** decrement of the linked resource.

## Related
    - `stockFlow` (super-property)
    - `duality` (pairs increment/decrement events)
    - `Economic Event` ↔ `Economic Resource` (via this property)

    _Last updated: 2025-11-04_
