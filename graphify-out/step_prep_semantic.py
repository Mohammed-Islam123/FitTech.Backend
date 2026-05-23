import json
from pathlib import Path
from collections import defaultdict

detect = json.loads(Path('graphify-out/.graphify_detect.json').read_text())

# Only non-code files need semantic extraction (code handled by AST)
doc_files = detect.get('files', {}).get('document', [])
paper_files = detect.get('files', {}).get('paper', [])
image_files = detect.get('files', {}).get('image', [])
all_uncached = doc_files + paper_files + image_files

print(f'Semantic extraction: {len(all_uncached)} files to process')
print(f'  docs: {len(doc_files)}, papers: {len(paper_files)}, images: {len(image_files)}')

# Split into chunks of 10-12 files (smaller chunks due to large corpus >200 files)
# Group by directory for related files
dir_groups = defaultdict(list)
for f in all_uncached:
    parent = str(Path(f).parent)
    dir_groups[parent].append(f)

# Build chunks respecting directory grouping
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

print(f'Chunks: {len(chunks)} groups of ~11 files each')
for i, chunk in enumerate(chunks):
    print(f'  Chunk {i+1}: {len(chunk)} files')

# Save chunks
Path('graphify-out/.graphify_chunk_manifest.json').write_text(json.dumps(chunks, indent=2))
