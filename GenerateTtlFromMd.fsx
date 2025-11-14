open System
open System.IO
open System.Text.RegularExpressions

// =====================
// Configuration
// =====================

let baseUri   = "http://example.org/rea#"   // change to your real base URI
let prefix    = "rea"
let mdFolder  = "rea_md"                    // folder with .md files
let outputTtl = "rea_ontology.ttl"


// =====================
// Domain model
// =====================

type Entity =
    { LocalName     : string
      mutable Label : string option
      mutable Comment : string option
      mutable RdfType : string option
      SubClassOf    : ResizeArray<string>
      SubPropertyOf : ResizeArray<string>
      Domain        : ResizeArray<string>
      Range         : ResizeArray<string> }

    member this.Iri = sprintf "%s:%s" prefix this.LocalName


// =====================
// Helpers
// =====================

let readMdFiles (folder : string) =
    Directory.GetFiles(folder, "*.md")
    |> Array.map (fun path ->
        let stem = Path.GetFileNameWithoutExtension(path)
        stem, File.ReadAllText(path)
    )
    |> dict

let tryMatch (pattern : string) (text : string) (options : RegexOptions) =
    let m = Regex.Match(text, pattern, options)
    if m.Success then Some m else None

let extractLabel (md : string) =
    // **rdfs:label:** Something
    match tryMatch @"\*\*rdfs:label:\*\*\s*(.+)" md RegexOptions.Multiline with
    | Some m -> Some (m.Groups.[1].Value.Trim())
    | None ->
        // fallback: first heading "# Something"
        match tryMatch @"^#\s+(.+)$" md RegexOptions.Multiline with
        | Some m2 -> Some (m2.Groups.[1].Value.Trim())
        | None -> None

let extractCommentLine (md : string) =
    let firstLine =
        md.Split([|'\n'|], StringSplitOptions.None)
        |> Array.tryHead
        |> Option.defaultValue ""
        |> fun s -> s.Trim()

    if firstLine.StartsWith("rdfs:comment") then
        match tryMatch @"rdfs:comment\s+""(.*)""\s*;" firstLine RegexOptions.None with
        | Some m -> Some (m.Groups.[1].Value.Trim())
        | None   -> None
    else None

let extractRdfType (md : string) =
    match tryMatch @"\*\*rdf:type:\*\*\s*([A-Za-z0-9_:]+)" md RegexOptions.Multiline with
    | Some m -> Some (m.Groups.[1].Value.Trim())
    | None   -> None

let extractMultiValues (value : string) =
    Regex.Split(value, "[|,]")
    |> Array.map (fun v -> v.Trim())
    |> Array.filter (fun v -> v <> "")
    |> Array.toList

let normaliseName (name : string) =
    name.Replace(" ", "").Replace("-", "")

let buildLabelIndex (mdFiles : Collections.Generic.IDictionary<string,string>) =
    mdFiles
    |> Seq.choose (fun kv ->
        let stem, text = kv.Key, kv.Value
        match extractLabel text with
        | Some label -> Some (label, stem)
        | None -> None
    )
    |> dict


// =====================
// Parsing
// =====================

let parseEntities (mdFiles : Collections.Generic.IDictionary<string,string>) =
    let labelIndex = buildLabelIndex mdFiles
    let entities = Collections.Generic.Dictionary<string, Entity>()

    let getOrCreateEntity stem =
        match entities.TryGetValue(stem) with
        | true, ent -> ent
        | false, _ ->
            let ent =
                { LocalName   = stem
                  Label       = None
                  Comment     = None
                  RdfType     = None
                  SubClassOf  = ResizeArray()
                  SubPropertyOf = ResizeArray()
                  Domain      = ResizeArray()
                  Range       = ResizeArray() }
            entities.[stem] <- ent
            ent

    for kv in mdFiles do
        let stem, text = kv.Key, kv.Value
        let ent = getOrCreateEntity stem

        // comment
        match extractCommentLine text with
        | Some c -> ent.Comment <- Some c
        | None -> ()

        // label
        match extractLabel text with
        | Some l -> ent.Label <- Some l
        | None -> ()

        // rdf:type
        match extractRdfType text with
        | Some t -> ent.RdfType <- Some t
        | None -> ()

        // scan other lines for rdfs:* keys
        for line in text.Split([|'\n'|], StringSplitOptions.None) do
            let line = line.Trim()
            match tryMatch @"\*\*(rdfs:subClassOf|rdfs:subPropertyOf|rdfs:domain|rdfs:range):\*\*\s*(.+)" line RegexOptions.None with
            | Some m ->
                let key = m.Groups.[1].Value
                let rawValue = m.Groups.[2].Value.Trim()
                let targets = extractMultiValues rawValue

                let resolve (t : string) =
                    if labelIndex.ContainsKey(t) then
                        labelIndex.[t]
                    else
                        normaliseName t

                let resolved = targets |> List.map resolve

                match key with
                | "rdfs:subClassOf"    -> resolved |> List.iter ent.SubClassOf.Add
                | "rdfs:subPropertyOf" -> resolved |> List.iter ent.SubPropertyOf.Add
                | "rdfs:domain"        -> resolved |> List.iter ent.Domain.Add
                | "rdfs:range"         -> resolved |> List.iter ent.Range.Add
                | _ -> ()
            | None -> ()
    
    // default rdf:type to owl:Class if missing
    for ent in entities.Values do
        if ent.RdfType.IsNone then
            ent.RdfType <- Some "owl:Class"

    entities


// =====================
// TTL Writing
// =====================

let writeTtl (entities : Collections.Generic.Dictionary<string,Entity>) (outPath : string) =
    use w = new StreamWriter(outPath, false, Text.Encoding.UTF8)

    // Prefixes
    w.WriteLine(sprintf "@prefix %s: <%s> ." prefix baseUri)
    w.WriteLine("@prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .")
    w.WriteLine("@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .")
    w.WriteLine("@prefix owl:  <http://www.w3.org/2002/07/owl#> .")
    w.WriteLine("@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .")
    w.WriteLine()

    // Ontology header
    w.WriteLine(sprintf "%s: a owl:Ontology ;" prefix)
    w.WriteLine(@"    rdfs:comment ""REA ontology skeleton generated from Markdown."" ;")
    w.WriteLine("    .")
    w.WriteLine()

    // Emit entities
    let sorted =
        entities.Values
        |> Seq.sortBy (fun e -> e.LocalName.ToLowerInvariant())

    for ent in sorted do
        let triples = ResizeArray<string>()

        // rdf:type
        match ent.RdfType with
        | Some t -> triples.Add(sprintf "rdf:type %s" t)
        | None   -> triples.Add("rdf:type owl:Class")

        // rdfs:label
        match ent.Label with
        | Some l -> triples.Add(sprintf "rdfs:label \"%s\"" l)
        | None   -> ()

        // rdfs:comment
        match ent.Comment with
        | Some c ->
            let esc = c.Replace("\"", "\\\"")
            triples.Add(sprintf "rdfs:comment \"%s\"" esc)
        | None -> ()

        // subClassOf
        for sc in ent.SubClassOf do
            triples.Add(sprintf "rdfs:subClassOf %s:%s" prefix sc)

        // subPropertyOf
        for sp in ent.SubPropertyOf do
            triples.Add(sprintf "rdfs:subPropertyOf %s:%s" prefix sp)

        // domain
        for d in ent.Domain do
            triples.Add(sprintf "rdfs:domain %s:%s" prefix d)

        // range
        for r in ent.Range do
            triples.Add(sprintf "rdfs:range %s:%s" prefix r)

        // Write block
        w.Write(ent.Iri + " ")
        if triples.Count > 0 then
            let first = triples.[0]
            w.Write(first)
            for i = 1 to triples.Count - 1 do
                w.Write(" ;\n    ")
                w.Write(triples.[i])
            w.WriteLine(" .")
            w.WriteLine()
        else
            w.WriteLine("a owl:Class .")
            w.WriteLine()

    w.Flush()


// =====================
// Main
// =====================

[<EntryPoint>]
let main argv =
    if not (Directory.Exists mdFolder) then
        eprintfn "Folder '%s' not found. Create it and put your .md files there." mdFolder
        1
    else
        let mdFiles = readMdFiles mdFolder
        if mdFiles.Count = 0 then
            eprintfn "No .md files found in '%s'." mdFolder
            1
        else
            let entities = parseEntities mdFiles
            writeTtl entities outputTtl
            printfn "Wrote ontology skeleton to %s" outputTtl
            0
