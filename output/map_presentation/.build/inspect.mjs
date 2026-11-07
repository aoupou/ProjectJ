import fs from 'node:fs/promises';
import { PresentationFile,FileBlob } from 'file:///C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const dir='C:/Users/JANG/OneDrive/Documents/GitHub/ProjectJ/output/map_presentation/.build';
const p=await PresentationFile.importPptx(await FileBlob.load('C:/Users/JANG/Downloads/9주차 개발일지 앞부분.pptx'));
await fs.writeFile(dir+'/source-inspect.ndjson',(await p.inspect({kind:'slide,textbox,image,layout',maxChars:100000})).ndjson);
await fs.writeFile(dir+'/source-proto.json',JSON.stringify(p.toProto()));
for(let i=0;i<p.slides.items.length;i++) {const b=await p.export({slide:p.slides.items[i],format:'png',scale:1});await fs.writeFile(`${dir}/source-${i+1}.png`,new Uint8Array(await b.arrayBuffer()));}
console.log('Rendered',p.slides.items.length);

