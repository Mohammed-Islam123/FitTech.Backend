import json
from pathlib import Path
from collections import defaultdict

detect = json.loads(Path('graphify-out/.graphify_detect.json').read_text())
doc_files = detect.get('files', {}).get('document', [])

# Group by directory for related files
dir_groups = defaultdict(list)
for f in doc_files:
    parent = str(Path(f).parent)
    dir_groups[parent].append(f)

chunks = []
current_chunk = []
for parent, files in dir_groups.items():
    for f in files:
        if len(current_chunk) >= 11:
            chunks.append(current_chunk)
            current_chunk = []
        current_chunk.append(f)
if current_chunk:
    chunks.append(current_chunk)

# Write out each chunk's file list
for i, chunk in enumerate(chunks):
    Path(f'graphify-out/.chunk_{i+1:02d}_files.txt').write_text('\n'.join(chunk))
    
print(f'Written {len(chunks)} chunk file lists')
for i, chunk in enumerate(chunks):
    dirs = set(str(Path(f).parent) for f in chunk)
    print(f'  Chunk {i+1}: {len(chunk)} files from {len(dirs)} directories')
