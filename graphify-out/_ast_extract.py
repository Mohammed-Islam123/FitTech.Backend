import sys, json
from graphify.extract import collect_files, extract
from pathlib import Path

inc = json.loads(Path('graphify-out/.graphify_incremental.json').read_text())
changed_code = inc.get('new_files', {}).get('code', [])
all_code = []
for f in changed_code:
    all_code.extend(collect_files(Path(f)) if Path(f).is_dir() else [Path(f)])

if all_code:
    result = extract(all_code)
    Path('graphify-out/.graphify_ast.json').write_text(json.dumps(result, indent=2))
    print(f'AST: {len(result["nodes"])} nodes, {len(result["edges"])} edges')
else:
    Path('graphify-out/.graphify_ast.json').write_text(json.dumps({'nodes':[],'edges':[],'input_tokens':0,'output_tokens':0}))
    print('No changed code files - skipping AST extraction')
