import sys, json
from networkx.readwrite import json_graph
import networkx as nx
from pathlib import Path

data = json.loads(Path("graphify-out/graph.json").read_text())
G = json_graph.node_link_graph(data, edges="links")

question = "when i hit the endpoint to get the clients of a coach it return coach not found even though i am logged in with the correct coach"
mode = "bfs"
terms = [t.lower() for t in ["coach", "clients", "GetCoachClients", "CoachClients", "authenticate", "identity", "user", "JWT", "authorization"] if len(t) > 3]

# Find best-matching start nodes
scored = []
for nid, ndata in G.nodes(data=True):
    label = ndata.get("label", "").lower()
    score = sum(1 for t in terms if t in label)
    if score > 0:
        scored.append((score, nid))
scored.sort(reverse=True)
start_nodes = [nid for _, nid in scored[:5]]

if not start_nodes:
    print("No matching nodes found for query terms")
    sys.exit(0)

print("Starting nodes:", [G.nodes[n].get("label",n) for n in start_nodes])
print()

# BFS: explore all neighbors layer by layer up to depth 3.
frontier = set(start_nodes)
subgraph_nodes = set(start_nodes)
subgraph_edges = []
for _ in range(3):
    next_frontier = set()
    for n in frontier:
        for neighbor in G.neighbors(n):
            if neighbor not in subgraph_nodes:
                next_frontier.add(neighbor)
                subgraph_edges.append((n, neighbor))
    subgraph_nodes.update(next_frontier)
    frontier = next_frontier

# Rank by relevance
def relevance(nid):
    label = G.nodes[nid].get("label", "").lower()
    return sum(1 for t in terms if t in label)

ranked_nodes = sorted(subgraph_nodes, key=relevance, reverse=True)

for nid in ranked_nodes:
    d = G.nodes[nid]
    print(f"  NODE {d.get('label', nid)} [type={d.get('file_type','')} src={d.get('source_file','')} loc={d.get('source_location','')}]")
print()

for u, v in subgraph_edges:
    if u in subgraph_nodes and v in subgraph_nodes:
        _raw = G[u][v]
        d = next(iter(_raw.values()), {}) if isinstance(G, nx.MultiGraph) else _raw
        print(f"  EDGE {G.nodes[u].get('label',u)} --{d.get('relation','')} [{d.get('confidence','')}]--> {G.nodes[v].get('label',v)}")
