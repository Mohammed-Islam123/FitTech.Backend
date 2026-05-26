import json
from graphify.cache import check_semantic_cache
from pathlib import Path

inc = json.loads(Path('graphify-out/.graphify_incremental.json').read_text())
all_new = []
for ext, files in inc.get('new_files', {}).items():
    if ext != 'code':
        all_new.extend(files)

# Also add deleted file to handle
deleted = inc.get('deleted_files', [])

if not all_new:
    print('No doc/paper/image files changed - skipping semantic extraction')
    Path('graphify-out/.graphify_semantic.json').write_text(json.dumps({'nodes':[],'edges':[],'hyperedges':[],'input_tokens':0,'output_tokens':0}))
else:
    cached_nodes, cached_edges, cached_hyperedges, uncached = check_semantic_cache(all_new)
    if cached_nodes or cached_edges or cached_hyperedges:
        Path('graphify-out/.graphify_cached.json').write_text(json.dumps({'nodes': cached_nodes, 'edges': cached_edges, 'hyperedges': cached_hyperedges}))
    Path('graphify-out/.graphify_uncached.txt').write_text('\n'.join(uncached))
    print(f'Cache: {len(all_new)-len(uncached)} files hit, {len(uncached)} files need extraction')
