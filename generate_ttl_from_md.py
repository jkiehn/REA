import os
import re
from pathlib import Path
from typing import Dict, List, Optional

# ===== Configuration =====
BASE_URI = "http://example.org/rea#"   # change to your real base URI
PREFIX = "rea"                         # the prefix used in TTL
MD_FOLDER = "rea_md"                   # folder with your .md files
OUTPUT_TTL = "rea_ontology.ttl"        # target TTL file


class Entity:
    def __init__(self, local_name: str):
        self.local_name = local_name
        self.label: Optional[str] = None
        self.comment: Optional[str] = None
        self.rdf_type: Optional[str] = None  # e.g. "owl:Class", "owl:ObjectProperty"
        self.subClassOf: List[str] = []
        self.subPropertyOf: List[str] = []
        self.domain: List[str] = []
        self.range: List[str] = []

    def iri(self) -> str:
        return f"{PREFIX}:{self.local_name}"


def read_md_files(folder: str) -> Dict[str, str]:
    """Return {stem: markdown_text} for all .md in folder."""
    result = {}
    p = Path(folder)
    for md_file in p.glob("*.md"):
        result[md_file.stem] = md_file.read_text(encoding="utf-8")
    return result


def extract_label(md: str) -> Optional[str]:
    # Pattern: **rdfs:label:** Something
    m = re.search(r"\*\*rdfs:label:\*\*\s*(.+)", md)
    if m:
        return m.group(1).strip()
    # Fallback: top-level heading "# Something"
    m = re.search(r"^#\s+(.+)$", md, flags=re.MULTILINE)
    if m:
        return m.group(1).strip()
    return None


def extract_comment_line(md: str) -> Optional[str]:
    # First line should be rdfs:comment "..." ;
    first_line = md.splitlines()[0].strip()
    if first_line.startswith("rdfs:comment"):
        m = re.search(r'rdfs:comment\s+"(.*)"\s*;', first_line)
        if m:
            return m.group(1).strip()
    return None


def extract_rdf_type(md: str) -> Optional[str]:
    # Pattern: **rdf:type:** owl:Class
    m = re.search(r"\*\*rdf:type:\*\*\s*([A-Za-z0-9_:]+)", md)
    if m:
        return m.group(1).strip()
    return None


def extract_multi_values(line_value: str) -> List[str]:
    """
    Split domain/range/subClassOf/subPropertyOf values on '|' or ','
    and clean each token.
    """
    parts = re.split(r"[|,]", line_value)
    cleaned = [p.strip() for p in parts if p.strip()]
    return cleaned


def normalise_name(name: str) -> str:
    """Fallback: turn 'Economic Event' -> 'EconomicEvent' etc."""
    return name.replace(" ", "").replace("-", "")


def build_label_index(md_files: Dict[str, str]) -> Dict[str, str]:
    """Map labels -> local_name (filename stem)."""
    index = {}
    for stem, text in md_files.items():
        label = extract_label(text)
        if label:
            index[label] = stem
    return index


def parse_entities(md_files: Dict[str, str]) -> Dict[str, Entity]:
    """
    Parse all markdowns into Entity objects:
    - pick up comment, label, type, subClassOf, subPropertyOf, domain, range.
    """
    label_index = build_label_index(md_files)
    entities: Dict[str, Entity] = {}

    for stem, text in md_files.items():
        ent = entities.get(stem) or Entity(stem)
        entities[stem] = ent

        # rdfs:comment
        comment = extract_comment_line(text)
        if comment:
            ent.comment = comment

        # rdfs:label
        label = extract_label(text)
        if label:
            ent.label = label

        # rdf:type
        rdf_type = extract_rdf_type(text)
        if rdf_type:
            ent.rdf_type = rdf_type

        # Scan all lines for other bold key-value pairs
        for line in text.splitlines():
            line = line.strip()
            m = re.search(
                r"\*\*(rdfs:subClassOf|rdfs:subPropertyOf|rdfs:domain|rdfs:range):\*\*\s*(.+)",
                line
            )
            if not m:
                continue
            key = m.group(1)
            value = m.group(2).strip()

            targets = extract_multi_values(value)
            resolved: List[str] = []

            for t in targets:
                # prefer mapping via label_index (exact label match)
                if t in label_index:
                    resolved.append(label_index[t])
                else:
                    # fallback: normalise to something like an identifier
                    resolved.append(normalise_name(t))

            if key == "rdfs:subClassOf":
                ent.subClassOf.extend(resolved)
            elif key == "rdfs:subPropertyOf":
                ent.subPropertyOf.extend(resolved)
            elif key == "rdfs:domain":
                ent.domain.extend(resolved)
            elif key == "rdfs:range":
                ent.range.extend(resolved)

    # Default type assumption: owl:Class if not specified
    for ent in entities.values():
        if ent.rdf_type is None:
            ent.rdf_type = "owl:Class"

    return entities


def write_ttl(entities: Dict[str, Entity], out_path: str):
    with open(out_path, "w", encoding="utf-8") as f:
        # Prefixes
        f.write(f"@prefix {PREFIX}: <{BASE_URI}> .\n")
        f.write("@prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .\n")
        f.write("@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .\n")
        f.write("@prefix owl:  <http://www.w3.org/2002/07/owl#> .\n")
        f.write("@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .\n\n")

        # Ontology header (optional)
        f.write(f"{PREFIX}: a owl:Ontology ;\n")
        f.write(f'    rdfs:comment "REA ontology skeleton generated from Markdown." ;\n')
        f.write("    .\n\n")

        # Entities
        for ent in sorted(entities.values(), key=lambda e: e.local_name.lower()):
            triples = []

            # rdf:type
            triples.append(f"rdf:type {ent.rdf_type}")

            # label
            if ent.label:
                triples.append(f'rdfs:label "{ent.label}"')

            # comment
            if ent.comment:
                # Escape quotes in comment
                cmt = ent.comment.replace('"', '\\"')
                triples.append(f'rdfs:comment "{cmt}"')

            # subClassOf
            for sc in ent.subClassOf:
                triples.append(f"rdfs:subClassOf {PREFIX}:{sc}")

            # subPropertyOf
            for sp in ent.subPropertyOf:
                triples.append(f"rdfs:subPropertyOf {PREFIX}:{sp}")

            # domain
            for d in ent.domain:
                triples.append(f"rdfs:domain {PREFIX}:{d}")

            # range
            for r in ent.range:
                triples.append(f"rdfs:range {PREFIX}:{r}")

            # Emit block
            f.write(f"{ent.iri()} ")
            if triples:
                f.write(";\n    ".join(triples))
                f.write(" .\n\n")
            else:
                # Entity with no extra info
                f.write("a owl:Class .\n\n")


def main():
    md_files = read_md_files(MD_FOLDER)
    entities = parse_entities(md_files)
    write_ttl(entities, OUTPUT_TTL)
    print(f"Wrote ontology skeleton to {OUTPUT_TTL}")


if __name__ == "__main__":
    main()
