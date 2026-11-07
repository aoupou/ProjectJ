import fs from 'node:fs/promises';
import {PresentationFile,FileBlob} from 'file:///C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const base='C:/Users/JANG/OneDrive/Documents/GitHub/ProjectJ';
const dir=base+'/output/map_presentation';
const p=await PresentationFile.importPptx(await FileBlob.load(dir+'/final/9주차_개발일지_맵과_프리뷰.pptx'));
const tile=await fs.readFile(base+'/Assets/Scripts/Map/TileData.cs','utf8');
const map=await fs.readFile(base+'/Assets/Scripts/Map/Map.cs','utf8');
const start=map.indexOf('        // MapData 에셋의 타일 데이터를 런타임 배열로 복사');
const end=map.indexOf('        // 플레이어가 배치될 타일',start);
const mapCode=map.slice(start,end).trimEnd().split('\n').map(l=>l.startsWith('        ')?l.slice(8):l).join('\n');
for(const [index,source] of [[7,tile],[11,mapCode]]){
 const s=p.slides.items[index];
 for(const shape of s.shapes.items){const y=shape.position.top;if(y>=170&&y<670){shape.text='';shape.fill='none';shape.line={fill:'none',width:0};}}
 const x=index===7?230:270,w=index===7?820:740,y=180,h=478;
 s.shapes.add({geometry:'rect',position:{left:x,top:y,width:w,height:h},fill:'#1E1E1E',line:{fill:'none',width:0}});
 const lines=source.replace(/\r/g,'').split('\n');
 const step=Math.min(index===7?24:15,(h-24)/lines.length),size=index===7?20:13;
 for(let i=0;i<lines.length;i++){
 const line=lines[i];
 const t=s.shapes.add({geometry:'textbox',name:'원본 코드 '+(i+1),position:{left:x+22,top:y+12+i*step,width:w-40,height:step+2},fill:'none',line:{fill:'none',width:0}});
 t.text=line;t.text.style={fontSize:size,typeface:'Consolas',color:'#D4D4D4',wrap:false,insets:0};t.text.insets=0;
 if(line.trim().startsWith('//')){t.text.style={fontSize:size-1,typeface:'Malgun Gothic',color:'#6A9955',wrap:false,insets:0};continue;}
 for(const token of new Set(line.match(/\b(?:using|public|class|int|bool|false|true|new|if|foreach|in|continue|null|TileData|Vector3|Debug)\b|[{}]/g)||[]))t.text.get(token).fill=['TileData','Vector3','Debug'].includes(token)?'#4EC9B0':/[{}]/.test(token)?'#FFD700':'#569CD6';
 }
 s.speakerNotes.text=String(s.speakerNotes.text??'').replace('발표용 발췌: x와 y 선언을 한 줄로 정리','원본 코드 전체');
 await fs.writeFile(dir+`/.build/revised-${index+1}.png`,new Uint8Array(await (await p.export({slide:s,format:'png',scale:1.5})).arrayBuffer()));
}
await (await PresentationFile.exportPptx(p)).save(dir+'/.build/revised-candidate.pptx');
console.log('saved');
