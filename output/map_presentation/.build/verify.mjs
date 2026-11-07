import fs from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {PresentationFile,FileBlob} from 'file:///C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const root='C:/Users/JANG/OneDrive/Documents/GitHub/ProjectJ/output/map_presentation';
const p=await PresentationFile.importPptx(await FileBlob.load(root+'/final/9주차_개발일지_맵과_프리뷰.pptx'));
await fs.mkdir(root+'/.build/final-render',{recursive:true});
const changed=[];
for(let i=0;i<p.slides.items.length;i++){
 const name=`slide-${String(i+1).padStart(2,'0')}.png`;
 const b=Buffer.from(await (await p.export({slide:p.slides.items[i],format:'png',scale:1})).arrayBuffer());
 await fs.writeFile(root+'/.build/final-render/'+name,b);
 const a=await fs.readFile(root+'/.build/render/'+name);
 if(!a.equals(b))changed.push(i+1);
 if(p.slides.items[i].speakerNotes.text.trim().length<70)throw Error('Missing notes '+(i+1));
}
console.log(JSON.stringify({slides:p.slides.items.length,notes:p.slides.items.length,changedRenders:changed}));
