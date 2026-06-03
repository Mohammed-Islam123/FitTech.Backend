import json
from networkx.readwrite import json_graph
import networkx as nx
from pathlib import Path

data = json.loads(Path('graphify-out/graph.json').read_text(encoding='utf-8'))
G = json_graph.node_link_graph(data, edges='links')

# Find edges connecting Identity/Notification/MassTransit/Wolverine
keywords = ['identity', 'notification', 'masstransit', 'wolverine', 'rabbitmq', 'messaging', 'message bus']
count = 0
for u, v, d in G.edges(data=True):
    ul = G.nodes[u].get('label', '').lower()
    vl = G.nodes[v].get('label', '').lower()
    label = ' '.join([ul, vl])
    if any(k in label for k in keywords):
        rel = d.get('relation', '')
        conf = d.get('confidence', '')
        print(f'{G.nodes[u]["label"]} --{rel}--> {G.nodes[v]["label"]} [{conf}]')
        count += 1
        if count >= 50:
            print('... (truncated at 50)')
            break

print()
print('=== COMMUNITY 66 DETAILS ===')
# Community 66 has Identity, MassTransit, Notification, RabbitMQ
for nid, ndata in G.nodes(data=True):
    lbl = ndata.get('label', '').lower()
    if any(k in lbl for k in ['identity service', 'notification service', 'masstransit', 'wolverine', 'clean architecture', 'rabbitmq']):
        print(f'  {ndata["label"]} [type={ndata.get("file_type","?")}] src={ndata.get("source_file","?")}')
for u,v,d in G.edges(data=True):
    ul = G.nodes[u].get('label', '').lower()
    vl = G.nodes[v].get('label', '').lower()
    if ('identity service' in ul or 'identity service' in vl) and ('notification service' in ul or 'notification service' in vl):
        print(f'{G.nodes[u]["label"]} --{d.get("relation","")}--> {G.nodes[v]["label"]} [{d.get("confidence","")}]')
    if ('masstransit' in ul or 'masstransit' in vl) and ('wolverine' in ul or 'wolverine' in vl):
        print(f'{G.nodes[u]["label"]} --{d.get("relation","")}--> {G.nodes[v]["label"]} [{d.get("confidence","")}]')
