import fs from 'node:fs/promises';
import { Presentation, PresentationFile } from 'file:///C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
import { finalizePresentation } from 'file:///C:/Users/JANG/.codex/plugins/cache/openai-primary-runtime/presentations/26.1026.10807/skills/presentations/container_tools/artifact_tool_utils.mjs';
const root='C:/Users/JANG/OneDrive/Documents/GitHub/ProjectJ';
const build=root+'/.tiledata-build';
const skill='C:/Users/JANG/.codex/plugins/cache/openai-primary-runtime/presentations/26.1026.10807/skills/presentations';
await fs.mkdir(root+'/output',{recursive:true});
const p=Presentation.create({slideSize:{width:1600,height:900}});
const s=p.slides.add();
s.background.fill='#FFFFFF';
function box(name,x,y,w,h,fill='none'){
 return s.shapes.add({name,geometry:'rect',position:{left:x,top:y,width:w,height:h},fill,line:{fill:'none',width:0}});
}
function text(name,content,x,y,w,h,size,font='Malgun Gothic',color='#000000',align='left'){
 const t=box(name,x,y,w,h);t.text=content;
 t.text.style={fontSize:size,typeface:font,color,alignment:align,verticalAlignment:'middle',insets:0,wrap:'none',autoFit:'none'};return t;
}
text('제목','코드 TileData',100,65,1400,92,64,'Malgun Gothic','#000000','center');
text('설명','각 타일의 좌표, 플레이어·적·사과의 존재 여부와 이동 가능 여부를 저장하는 클래스',105,160,1390,48,32,'Malgun Gothic','#000000','center');
box('코드 배경',349,222,900,595,'#202331');
const code=`using UnityEngine;

[System.Serializable]
public class TileData
{
    public int x;
    public int y;

    public bool OnPlayer = false;
    public bool OnEnemy = false;
    public bool OnApple = false;

    public bool CanEnemyGo = true;
    public bool CanPlayerGo = true;
    public bool Visitable = true;
    // 현재 실행 중인 맵에서 사용하며 MapData 에셋에는 저장하지 않음
    [System.NonSerialized]
    public Vector3 worldPosition;
}`;
const lines=code.split('\n');
lines.forEach((line,i)=>{
 const t=text('코드 '+(i+1),line,395,239+i*29,820,29,24,'Consolas','#D4D4D4');
 if(line.trim().startsWith('//')){t.text.style={fontSize:21,typeface:'Malgun Gothic',color:'#6A9955',insets:0,wrap:'none'};return;}
 const tokens=line.match(/\b(?:using|public|class|int|bool|false|true|TileData|Vector3|UnityEngine)\b|[{}]/g)||[];
 for(const token of new Set(tokens)){
 const color=['TileData','Vector3'].includes(token)?'#4EC9B0': token==='UnityEngine'?'#D4D4D4': /[{}]/.test(token)?'#FFD700':'#569CD6';
 t.text.get(token).fill=color;
 }
});
box('하단 띠',0,854,1600,46,'#EFCEEE');
text('하단 구분','03 / 프로그래밍',12,856,650,42,36);
await fs.writeFile(build+'/preview.png',new Uint8Array(await (await p.export({slide:s,format:'png',scale:1})).arrayBuffer()));
await (await PresentationFile.exportPptx(p)).save(build+'/candidate.pptx');
const result=await finalizePresentation({workspaceDir:root,candidatePath:build+'/candidate.pptx',finalPath:root+'/output/TileData.pptx',pythonExecutable:'C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe',integrityValidatorPath:skill+'/container_tools/inspect_presentation_package_integrity.py',layoutValidatorPath:skill+'/container_tools/inspect_presentation_layout_geometry.py',layoutArgs:['--expected-slide-size-emu','15240000,8572500','--validate-heading-fit'],explicitTotalSlideCount:1,fontPolicy:{basis:'design',families:['Malgun Gothic','Consolas']},verifyArtifactToolImport:true,receiptPath:build+'/validation.json'});
console.log(JSON.stringify(result));

