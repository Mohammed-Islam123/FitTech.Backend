import sys, json, math, random
from pathlib import Path
from collections import Counter, defaultdict

sys.stdout.reconfigure(encoding='utf-8')

graph_path = Path('graphify-out/graph.json')
graph = json.loads(graph_path.read_text(encoding='utf-8'))

# Graph uses 'links' key (not 'edges')
nodes = graph['nodes']
links = graph.get('links', [])
edges = [{'source': l['source'], 'target': l['target']} for l in links]

doc_path = "docs/salle de sport.pdf"

# Check existing semantic nodes for this file
existing = [n for n in nodes if n.get('type') == 'semantic' and doc_path in n.get('source_files', [])]

if existing:
    print(f"Already has {len(existing)} semantic nodes for {doc_path}, skipping")
else:
    from sentence_transformers import SentenceTransformer
    model = SentenceTransformer('all-MiniLM-L6-v2')

    doc_text = """Fiche de presentation du projet
Systeme de gestion d'une Salle de sport connectee « FitTech »
Titre complet
AMAR BENSABER Djamel
Encadreur
Resume Systeme de gestion d'une salle de sport connectee
Une salle de sport moderne, "FitTech", souhaite developper un systeme integre de
gestion des abonnements, des reservations de cours et du suivi d'activite.
Le systeme doit etre compose d'une application mobile pour les membres, d'une
interface web pour les coachs et les administrateurs, et d'une infrastructure materielle
(capteurs, lecteurs NFC, machines connectees).
Besoins fonctionnels detailles :
- Gestion des membres et abonnements :
  a. Inscription d'un nouveau membre : saisie des informations personnelles
(nom, prenom, date de naissance, email, telephone, photo), choix de
l'abonnement, paiement en ligne ou sur place.
  b. Types d'abonnements : Mensuel (renouvellement automatique, resiliable a tout moment), Annuel (engagement 12 mois, paiement en une fois ou mensualise), A la seance (achat d'un pack de seances, par exemple 10 entrees), Essai gratuit (1 seance)
  c. Profil sante : chaque membre peut (optionnellement) renseigner des objectifs (perte de poids, prise de masse, endurance) et des restrictions medicales (problemes de dos, allergies, etc.)
  d. Gestion de la carte NFC : chaque membre recoit une carte NFC unique qui sert de cle d'acces a la salle.
  e. Espace membre : consultation des informations personnelles, historique des seances, factures, et possibilite de mettre en pause l'abonnement (maximum 2 mois par an).
- Reservation de cours :
  a. Les coachs creent des cours (spinning, crossfit, etc.) avec un nombre maximum de participants, date/heure, duree et niveau requis.
  b. Les membres peuvent reserver un cours via l'application mobile, jusqu'a 7 jours a l'avance.
  c. Liste d'attente avec auto-inscription et notification.
  d. Annulation : jusqu'a 2 heures avant le cours.
  e. Presence validee par le coach via tablette. Absence non-annulee = avertissement, apres 3 = suspension 1 semaine.
- Acces et suivi d'activite :
  a. Acces NFC a la salle avec enregistrement entree/sortie.
  b. Machines connectees avec capteurs (distance, calories, rythme cardiaque).
  c. Tableau de bord de performance avec statistiques et recommandations.
- Gestion du personnel :
  a. Coachs : profil, planning, consultation profils sante.
  b. Administrateurs : gestion abonnements, membres, coachs, rapports financiers.
- Notification et communication :
  a. Notifications automatiques (rappel cours, alerte objectif, offres, annulation).
  b. Communication interne messages membres -> coachs.
- Autres fonctionnalites :
  a. Gestion des equipements (maintenance).
  b. Vente de produits (boutique en ligne).
- Technologies proposees :
  - Mobile : Android/iOS (Swift)
  - Web : Laravel/NodeJS + Angular/JQuery
  - BDD : MySQL/PostgreSQL
  - Backend : SpringBoot, Spring Security, JSF (PrimeFaces) + Angular
"""

    # Generate embedding for full doc
    emb = model.encode(doc_text[:512]).tolist()

    # Split into chunks for better coverage
    chunks = [doc_text[i:i+1500] for i in range(0, len(doc_text), 1500)]
    new_nodes = []
    for i, chunk in enumerate(chunks):
        c_emb = model.encode(chunk[:512]).tolist()
        node = {
            "id": f"semantic:{doc_path}#chunk{i}",
            "type": "semantic",
            "label": f"FitTech Project Spec (chunk {i+1}/{len(chunks)})",
            "content": chunk[:300],
            "embedding": c_emb,
            "source_files": [doc_path],
            "tokens": len(chunk) // 4,
            "chunk_index": i,
            "total_chunks": len(chunks)
        }
        new_nodes.append(node)

    graph['nodes'].extend(new_nodes)

    # Add edges from semantic nodes to related god nodes
    god_keywords = ['membership', 'member', 'subscription', 'coach', 'trainer', 'plan', 'payment',
                    'notification', 'nfc', 'access', 'cour', 'reservation', 'booking', 'equipment',
                    'inventory', 'product']
    new_links = []
    for gn in graph['nodes']:
        if gn.get('type') == 'god' or gn.get('type') == 'concept':
            label = gn.get('label', '').lower()
            for kw in god_keywords:
                if kw in label:
                    for nn in new_nodes:
                        new_links.append({
                            "source": nn['id'],
                            "target": gn['id'],
                            "type": "MENTIONS"
                        })
                    break

    graph.setdefault('links', []).extend(new_links)
    links = graph['links']
    edges_data = [{'source': l['source'], 'target': l['target']} for l in links]

    print(f"Added {len(new_nodes)} semantic nodes, {len(new_links)} links")
    total_nodes = len(graph['nodes'])
    total_links = len(links)
    print(f"Total: {total_nodes} nodes, {total_links} links")

# --- Community Detection ---
import networkx as nx
import community as community_louvain

G = nx.Graph()
for n in graph['nodes']:
    G.add_node(n['id'])
for l in graph.get('links', []):
    G.add_edge(l['source'], l['target'])

partition = community_louvain.best_partition(G)
for n in graph['nodes']:
    n['community'] = partition.get(n['id'], -1)

# --- Community Summary ---
community_nodes_map = defaultdict(list)
for n in graph['nodes']:
    community_nodes_map[n['community']].append(n)

community_summary = {}
for cid, c_nodes in community_nodes_map.items():
    types = Counter(n.get('type', 'unknown') for n in c_nodes)
    labels = [n.get('label', '')[:60] for n in c_nodes[:20]]
    sub = G.subgraph([n['id'] for n in c_nodes])
    internal = sub.number_of_edges()
    ext_edges = 0
    cid_set = set(n['id'] for n in c_nodes)
    for l in graph.get('links', []):
        src_in = l['source'] in cid_set
        tgt_in = l['target'] in cid_set
        if src_in != tgt_in:
            ext_edges += 1
    cohesion = round(internal / (internal + ext_edges + 1), 3)
    community_summary[str(cid)] = {
        "size": len(c_nodes),
        "types": dict(types),
        "cohesion": cohesion,
        "sample_labels": labels[:5]
    }

# --- Generate Report ---
report = f"""# Graphify Report (Incremental Update)

**Date:** 2026-05-22
**Total Nodes:** {len(graph['nodes'])}
**Total Edges (Links):** {len(graph.get('links', []))}
**Communities:** {len(community_nodes_map)}

## New Files
- `docs/salle de sport.pdf` - FitTech project specification document (French)

## Community Summary
"""
for cid in sorted(community_nodes_map.keys(), key=lambda x: len(community_nodes_map[x]), reverse=True):
    cs = community_summary[str(cid)]
    report += f"""
### Community {cid} ({cs['size']} nodes, cohesion: {cs['cohesion']})
- Types: {cs['types']}
- Key nodes: {', '.join(cs['sample_labels'])}
"""

Path('graphify-out/GRAPH_REPORT.md').write_text(report, encoding='utf-8')
print("\nReport saved to GRAPH_REPORT.md")

# God nodes
god_nodes = [n for n in graph['nodes'] if n.get('type') == 'god']
print(f"God nodes: {len(god_nodes)}")

# --- HTML Viz ---
html = '<html><head><title>Graphify Graph</title><style>body{font-family:sans-serif;margin:20px}svg{width:100%;height:90vh}</style></head><body>'
html += f'<h1>Graphify Graph</h1><p>{len(graph["nodes"])} nodes, {len(graph.get("links", []))} links, {len(community_nodes_map)} communities</p>'
html += '<svg viewBox="0 0 900 700" xmlns="http://www.w3.org/2000/svg">'

random.seed(42)
try:
    pos = nx.spring_layout(G, k=2, iterations=50, seed=42)
except Exception as e:
    print(f"Spring layout warning: {e}")
    pos = {n['id']: (random.gauss(0,1), random.gauss(0,1)) for n in graph['nodes']}

xs = [p[0] for p in pos.values()]
ys = [p[1] for p in pos.values()]
minx, maxx = min(xs), max(xs)
miny, maxy = min(ys), max(ys)

# Draw edges first
for l in graph.get('links', []):
    if l['source'] in pos and l['target'] in pos:
        x1 = (pos[l['source']][0] - minx) / (maxx - minx) * 800 + 50
        y1 = (pos[l['source']][1] - miny) / (maxy - miny) * 600 + 50
        x2 = (pos[l['target']][0] - minx) / (maxx - minx) * 800 + 50
        y2 = (pos[l['target']][1] - miny) / (maxy - miny) * 600 + 50
        html += f'<line x1="{x1:.1f}" y1="{y1:.1f}" x2="{x2:.1f}" y2="{y2:.1f}" stroke="#ddd" stroke-width="0.5"/>'

community_colors = ['#e6194b','#3cb44b','#ffe119','#4363d8','#f58231','#911eb4','#46f0f0','#f032e6','#bcf60c','#fabebe','#008080','#e6beff','#9a6324','#800000','#aaffc3','#808000','#ffd8b1','#000075','#808080','#ffffff']
for n in graph['nodes']:
    if n['id'] not in pos:
        continue
    x, y = pos[n['id']]
    nx_ = (x - minx) / (maxx - minx) * 800 + 50
    ny = (y - miny) / (maxy - miny) * 600 + 50
    c = community_colors[n.get('community', 0) % len(community_colors)]
    r = 5 if n.get('type') == 'semantic' else 8 if n.get('type') == 'god' else 3
    html += f'<circle cx="{nx_:.1f}" cy="{ny:.1f}" r="{r}" fill="{c}" stroke="#333" stroke-width="1"><title>{n.get("id","")}: {n.get("label","")[:80]}</title></circle>'

html += '</svg></body></html>'
Path('graphify-out/graph.html').write_text(html, encoding='utf-8')
print("HTML viz saved to graph.html")

# Save graph
graph_path.write_text(json.dumps(graph, ensure_ascii=False, indent=2), encoding='utf-8')
print(f"Graph saved to {graph_path}")

# Incremental state
inc = {"new_total": 1, "all_files_processed": True}
Path('graphify-out/.graphify_incremental.json').write_text(json.dumps(inc, ensure_ascii=False), encoding='utf-8')
print("Incremental state updated")

# Final summary
print(f"\n=== Final Summary ===")
print(f"1. Semantic nodes added: {len([n for n in graph['nodes'] if n.get('type') == 'semantic' and doc_path in n.get('source_files', [])])}")
print(f"2. Total: {len(graph['nodes'])} nodes, {len(graph.get('links', []))} links")
print(f"3. Communities detected: {len(community_nodes_map)}")
print(f"4. GRAPH_REPORT.md and graph.html updated successfully")
