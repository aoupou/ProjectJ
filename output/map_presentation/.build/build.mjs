import fs from 'node:fs/promises';
import { PresentationFile,FileBlob } from 'file:///C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const root='C:/Users/JANG/OneDrive/Documents/GitHub/ProjectJ/output/map_presentation';
const p=await PresentationFile.importPptx(await FileBlob.load('C:/Users/JANG/Downloads/9주차 개발일지 앞부분.pptx'));
p.slides.keep([0,1,2,3,4,5]);
const agenda=await p.inspect({kind:'textbox',search:'03 / 일정',maxChars:5000});
for(const r of agenda.records??[])if(r.kind==='textbox'&&r.slideIndex===1)p.resolve(r.id).text='03 / 후속 작업';
const titles=['9주차 개발일지','목차','UI 제작','적 스프라이트 시트 제작','아이템 / 재화 아이콘 제작','만화 구성'];
const C={blue:'#4E95D9',pink:'#DEA0DF',pale:'#F6E3F5',ink:'#171921',muted:'#545966',code:'#222534',green:'#C8EAD9',gray:'#E9ECF1',red:'#F2CCCC'};
const font='맑은 고딕';
function txt(s,text,x,y,w,h,size=28,opts={}){const a=s.shapes.add({geometry:'textbox',name:opts.name??text.slice(0,30),position:{left:x,top:y,width:w,height:h},fill:opts.fill??'none',line:{fill:'none',width:0}});a.text=text;a.text.style={typeface:opts.code?'Consolas':font,fontSize:size,color:opts.color??C.ink,bold:opts.bold??false,alignment:opts.align??'left',verticalAlignment:'top',wrap:true};a.text.insets=opts.padding??0;return a;}
function slide(title,sub=''){titles.push(title);const s=p.slides.add({layout:'빈 화면'});s.background.fill='#FFFFFF';txt(s,title,54,36,1172,74,46,{align:'center'});if(sub)txt(s,sub,70,118,1140,44,24,{align:'center',color:C.muted});txt(s,String(p.slides.items.length),1176,674,48,24,16,{align:'right',color:'#7A7D85'});return s;}
function table(s,values,x,y,w,h,widths=null,size=25,header=true){const t=s.tables.add({rows:values.length,columns:values[0].length,left:x,top:y,width:w,height:h,values,...(widths?{columnWidths:widths}:{})});t.borders.assign({fill:'#FFFFFF',width:2,style:'solid'});t.cells.block({row:0,column:0,rowCount:values.length,columnCount:values[0].length}).assign({fill:C.pale,textStyle:{typeface:font,fontSize:size,color:C.ink},margins:{left:14,right:12,top:10,bottom:8},anchor:'center'});for(let r=0;r<values.length;r++)t.rows[r].height=h/values.length;if(header){t.cells.block({row:0,column:0,rowCount:1,columnCount:values[0].length}).assign({fill:C.pink,textStyle:{bold:true}});}return t;}
function code(s,lines,x=64,y=192,w=730,h=418,size=25){return txt(s,lines,x,y,w,h,size,{code:true,color:'#EEF0F8',fill:C.code,padding:24});}
function steps(s,items,x=835,y=205,w=380,gap=112){for(let i=0;i<items.length;i++){txt(s,String(i+1),x,y+i*gap,36,42,32,{bold:true,color:C.blue});txt(s,items[i][0],x+47,y+i*gap,w-47,40,27,{bold:true});txt(s,items[i][1],x+47,y+i*gap+44,w-47,gap-42,23,{color:C.muted});}}
function caption(s,text){txt(s,text,70,624,1140,43,23,{align:'center',color:C.muted});}
function grid(s,{x=90,y=195,size=390,n=5,values={},fills={},coords=false}={}){const data=[];for(let yy=n-1;yy>=0;yy--){const row=[];for(let xx=0;xx<n;xx++)row.push(values[`${xx},${yy}`]??(coords?`(${xx}, ${yy})`:''));data.push(row);}const t=table(s,data,x,y,size,size,null,26,false);t.cells.block({row:0,column:0,rowCount:n,columnCount:n}).assign({margins:{left:4,right:4,top:4,bottom:4}});for(let yy=0;yy<n;yy++)for(let xx=0;xx<n;xx++){const cell=t.getCell(n-1-yy,xx);cell.fill=fills[`${xx},${yy}`]??((xx+yy)%2===0?'#F0F1F4':'#FFFFFF');cell.text.style={typeface:font,fontSize:coords?20:26,color:C.ink,alignment:'center',bold:!!values[`${xx},${yy}`]};}t.borders.assign({fill:'#AFB5C0',width:1,style:'solid'});txt(s,'x',x+size+18,y+size-22,28,30,24,{color:C.muted});txt(s,'y',x-26,y-30,28,30,24,{color:C.muted});return t;}
// 7
{
const s=slide('맵이 작동하기 위해 필요한 데이터','맵 전체의 설정과 타일 한 칸의 상태를 나누어 관리');
table(s,[['MapData','맵 전체에 대한 정보'],['width / height','가로와 세로 칸 수'],['tiles','TileData 목록']],62,206,552,285,[210,342],26);
table(s,[['TileData','타일 한 칸에 대한 정보'],['x / y','맵 안에서의 좌표'],['OnPlayer / OnEnemy','캐릭터 점유 상태'],['Visitable / CanPlayerGo\nCanEnemyGo','지형과 대상별 이동 허용'],['worldPosition','실행 중 계산한 월드 좌표']],650,206,568,375,[285,283],24);
caption(s,'Map.cs는 이 데이터를 실행용 2차원 배열로 관리한다');
}
// 8
{
const s=slide('코드 TileData','좌표, 점유 상태, 이동 허용 여부를 한 칸에 모아 저장');
code(s,'[System.Serializable]\npublic class TileData\n{\n    public int x, y;\n    public bool OnPlayer = false;\n    public bool OnEnemy = false;\n    public bool OnApple = false;\n    public bool CanEnemyGo = true;\n    public bool CanPlayerGo = true;\n    public bool Visitable = true;\n    [System.NonSerialized]\n    public Vector3 worldPosition;\n}',72,185,726,432,23);
steps(s,[['점유 상태','플레이어·적이 있는지 기록'],['이동 허용','지형과 이동 대상의 조건'],['저장 제외','worldPosition은 에셋에 저장하지 않음']],840,205,370,120);
caption(s,'발표용 발췌: x와 y 선언을 한 줄로 정리');
}
// 9
{
const s=slide('좌표 평면으로 표현되는 맵','격자 좌표로 타일을 찾고, 월드 좌표로 화면에 배치');
grid(s,{x:125,y:195,size:400,coords:true,values:{'2,2':'P\n(2, 2)'},fills:{'2,2':C.blue}});
txt(s,'격자 좌표',650,205,500,45,34,{bold:true});txt(s,'mapData[2, 2]\n플레이어가 있는 타일의 정보',650,266,510,100,28);
txt(s,'월드 좌표',650,408,500,45,34,{bold:true});txt(s,'타일 중심의 실제 위치\n캐릭터를 화면에 배치할 때 사용',650,468,520,105,28);
caption(s,'설명용 5 × 5 예시. 좌표는 왼쪽 아래 (0, 0)부터 시작');
}
// 10
{
const s=slide('맵 크기에 맞춰 타일 데이터 생성','MapData.GenerateTiles()');
code(s,'for (int y = 0; y < height; y++)\n{\n    for (int x = 0; x < width; x++)\n    {\n        tiles.Add(new TileData\n        {\n            x = x,\n            y = y\n        });\n    }\n}',64,190,740,405,25);
steps(s,[['기존 목록 정리','목록이 없으면 생성하고, 있으면 비움'],['좌표 반복','가로와 세로 범위를 차례로 순회'],['데이터 추가','각 (x, y)에 해당하는 TileData 생성']],840,200,370,120);
caption(s,'MapDataEditor의 “타일 자동 생성” 버튼에서 호출');
}
// 11
{
const s=slide('게임 실행 시 맵을 준비하는 순서','Awake()에서 데이터를 준비하고 Start()에서 화면을 생성');
table(s,[['Awake()','Start()'],['맵 데이터와 크기 검사','타일 프리팹 확인'],['한 칸 크기와 시작점 계산','BattlePreview 참조 찾기'],['실행용 배열에 타일 데이터 복사','CreateTiles()로 타일 생성'],['플레이어·적 시작 좌표 선택','CreateFrame()으로 외곽선 생성']],90,204,1100,360,[550,550],27);
caption(s,'[DefaultExecutionOrder(-100)]으로 Map의 초기화를 앞당김');
}
// 12
{
const s=slide('저장한 데이터와 실행 중 상태 분리','MapData의 목록을 TileData[,] 배열로 복사');
code(s,'mapData = new TileData[width, height];\n\nmapData[source.x, source.y] = new TileData\n{\n    x = source.x,\n    y = source.y,\n    OnPlayer = false,\n    Visitable = source.Visitable,\n    CanEnemyGo = source.CanEnemyGo,\n    CanPlayerGo = source.CanPlayerGo,\n    OnApple = false,\n    OnEnemy = false\n};',58,190,790,425,23);
steps(s,[['좌표로 바로 조회','목록의 정보를 mapData[x, y]에 배치'],['새 객체 생성','플레이 중 바뀌는 상태를 별도로 관리'],['예외 검사','null과 맵 밖 좌표는 복사하지 않음']],878,207,330,122);
}
// 13
{
const s=slide('타일을 생성하는 과정','CreateTiles()가 각 좌표의 CreateTile()을 호출');
code(s,'private void CreateTile(int x, int y)\n{\n    GameObject tile = Instantiate(TilePrefab);\n\n    SetTileTransform(tile.transform, x, y);\n    RegisterTileCoordinate(tile, x, y);\n    SetupTileVisuals(tile, x, y);\n}',62,214,748,335,25);
steps(s,[['생성','타일 프리팹을 복제'],['배치','크기와 월드 위치 설정'],['연결','콜라이더와 타일 데이터 연결'],['표시','기본 색상 설정과 프리뷰 등록']],840,188,365,104);
caption(s,'CreateTiles()는 2차원 배열에서 데이터가 존재하는 칸만 생성');
}
// 14
{
const s=slide('격자 좌표를 타일 중심 위치로 변환','SetTileTransform()');
code(s,'Vector3 worldPosition = new Vector3(\n    MapStartX + TileWidthSize * (x + 0.5f),\n    MapStartY + TileHeightSize * (y + 0.5f),\n    1f\n);\n\ntileTransform.position = worldPosition;\nmapData[x, y].worldPosition = worldPosition;',62,225,780,330,24);
steps(s,[['한 칸 크기','MapSize의 크기 ÷ 가로·세로 칸 수'],['시작점','맵 중심에서 전체 크기의 절반을 뺌'],['중심으로 보정','좌표에 0.5를 더해 칸 중앙에 배치']],872,205,335,124);
caption(s,'예: 시작점 0, 한 칸 너비 2, x = 1이면 중심 x는 3');
}
// 15
{
const s=slide('클릭한 콜라이더로 타일 좌표 찾기','RegisterTileCoordinate()와 TryGetTileCoordinate()');
code(s,'// 타일 생성 시 연결\ntileDataByCollider[tileCollider] = mapData[x, y];\n\n// 클릭한 콜라이더로 조회\nif (!tileDataByCollider.TryGetValue(\n        tileCollider, out TileData tile))\n    return false;\n\nx = tile.x;\ny = tile.y;\nreturn true;',62,198,790,407,24);
steps(s,[['입력','레이캐스트가 타일 콜라이더를 찾음'],['조회','딕셔너리에서 연결된 TileData 검색'],['결과','찾은 데이터의 x, y를 반환']],884,216,325,120);
caption(s,'Dictionary<Collider2D, TileData>가 콜라이더와 데이터를 연결');
}
// 16
{
const s=slide('체스판 색상과 프리뷰 연결','SetupTileVisuals()에서 표시할 타일을 등록');
code(s,'fill.color = (x + y) % 2 == 0\n    ? CheckerColor\n    : Color.clear;\n\nif (tileColor != null)\n    tileColor.AddTile(x, y, fill, damageText);\n\n// BattlePreview.AddTile()\ntileFill[x, y] = fill;\ntileText[x, y] = tileDamage;\noriginalTileColors[x, y] = fill.color;',62,195,780,410,24);
steps(s,[['기본 색상','좌표 합의 홀짝에 따라 번갈아 표시'],['프리뷰 등록','좌표별 렌더러와 텍스트 참조 저장'],['원래 색 보관','이동·공격 표시를 지울 때 복원']],875,210,332,125);
caption(s,'짝수 칸은 CheckerColor, 홀수 칸은 투명색. 외곽선은 CreateFrame()에서 생성');
}
// 17
{
const s=slide('시작 위치 선택과 점유 상태 관리','타일의 지형 설정과 캐릭터가 있는 상태를 함께 관리');
table(s,[['구분','현재 코드의 처리'],['플레이어 시작 위치','데이터가 존재하는 타일 중 무작위 선택'],['적 시작 위치','Visitable과 CanEnemyGo 검사\n플레이어 주변 3 × 3 영역 제외'],['이전 플레이어 칸','OnPlayer 해제, 기존 CanEnemyGo 복원'],['새 플레이어 칸','OnPlayer 설정, CanEnemyGo를 false로 변경']],76,195,1128,390,[300,828],26);
caption(s,'적 점유는 OnEnemy로 검사하며, 적 배치 시 지형의 이동 설정을 유지');
}
// 18
{
const s=slide('다른 스크립트에서 맵 정보 사용','TryGet 함수는 조회 성공 여부와 결과를 함께 전달');
table(s,[['함수','가져오는 정보'],['TryGetTile(x, y, out tile)','타일 데이터와 이동 조건'],['TryGetTileCoordinate(collider, out x, out y)','클릭한 타일의 격자 좌표'],['TryGetTileWorldPosition(x, y, out position)','지정한 타일 중심의 월드 좌표'],['TryGetPlayerWorldPosition(...)','현재 플레이어 타일의 월드 좌표'],['TryGetEnemyWorldPosition(...)','현재 적 타일의 월드 좌표']],62,196,1156,384,[720,436],24);
caption(s,'BattlePreview는 맵의 좌표와 상태를 사용해 이동 범위를 계산');
}
// 19
{
const s=slide('이동 범위 미리보기의 실행 흐름','이동 선택 후 갈 수 있는 목적지를 색상으로 표시');
table(s,[['단계','함수','역할'],['1 이동 선택','Battle.Choose_Move()','SelectDirection 단계로 변경'],['2 범위 계산','CalculateMoveRange()','이동력 안에서 BFS 탐색'],['3 목적지 확인','CanMoveTo(x, y)','도달 여부와 타일 상태 검사'],['4 표시','ShowMovePreview()','이동 가능한 칸만 색칠']],62,204,1156,360,[215,460,481],25);
caption(s,'PlayerMovePreview()가 범위 계산과 화면 표시를 연결');
}
// 20
{
const s=slide('BFS로 가까운 칸부터 탐색','이동력 2, 장애물이 없는 5 × 5 맵의 예시');
const values={},fills={};for(let y=0;y<5;y++)for(let x=0;x<5;x++){const d=Math.abs(x-2)+Math.abs(y-2);if(d<=2){values[`${x},${y}`]=d===0?'P':String(d);fills[`${x},${y}`]=d===0?C.blue:d===1?'#9EC9EF':'#DAEAF7';}}
grid(s,{x:125,y:195,size:400,values,fills});
steps(s,[['시작 칸','거리 0, 방문 표시 후 큐에 추가'],['거리 1','시작 칸의 상하좌우를 확인'],['거리 2','다음 칸의 이웃으로 탐색 확장'],['종료','이동력을 다 쓴 칸에서는 확장 중단']],658,193,530,106);
caption(s,'칸의 숫자는 시작점부터 필요한 이동 횟수. 대각선으로 직접 이동하지 않음');
}
// 21
{
const s=slide('적이 있는 칸과 막힌 지형의 차이','탐색에 포함하는 칸과 이동 목적지를 구분');
grid(s,{x:120,y:200,size:390,values:{'2,2':'P','3,2':'적\n1','4,2':'2','2,3':'막힘'},fills:{'2,2':C.blue,'3,2':C.red,'4,2':C.green,'2,3':'#A8AFBB'}});
txt(s,'적이 있는 칸',635,213,550,45,33,{bold:true});txt(s,'BFS 탐색은 통과할 수 있다\n목적지 선택과 색칠에서는 제외한다',635,278,555,100,28);
txt(s,'이동 불가 지형',635,418,550,45,33,{bold:true});txt(s,'Visitable 또는 CanPlayerGo가 false면\n그 칸을 통해 탐색을 이어가지 않는다',635,482,565,105,27);
caption(s,'예시에서 적 너머의 칸은 거리 2로 탐색 가능. 실제 경로 애니메이션은 아직 없음');
}
// 22
{
const s=slide('코드 CalculateMoveRange','큐에서 꺼낸 칸의 이웃을 조건에 맞춰 탐색');
code(s,'if (currentDistance >= PlayerMovePower)\n    continue;\n\nif (!map.TryGetTile(nx, ny, out TileData tile))\n    continue;\nif (isVisited[nx, ny])\n    continue;\nif (!tile.Visitable || !tile.CanPlayerGo)\n    continue;\n\nisVisited[nx, ny] = true;\ndistance[nx, ny] = currentDistance + 1;\nqueue.Enqueue(neighborTile);',58,189,790,434,23);
steps(s,[['탐색 제한','이동력 초과, 없는 칸, 방문한 칸 제외'],['지형 검사','Visitable과 CanPlayerGo 확인'],['다음 탐색 준비','방문·거리 기록 후 큐에 추가']],877,207,332,124);
caption(s,'발표용 발췌: nx, ny는 neighborTile.x, neighborTile.y를 줄여 표기');
}
// 23
{
const s=slide('이동 목적지 검사와 색상 표시','BFS 결과에 현재 타일 상태를 한 번 더 확인');
code(s,'// CanMoveTo()의 핵심 조건\nreachableTiles[x, y]\n&& map.TryGetTile(x, y, out TileData tile)\n&& tile.Visitable\n&& tile.CanPlayerGo\n&& !tile.OnEnemy\n\n// ShowMovePreview()\nif (moveRange[x, y] && CanMoveTo(x, y))\n    Paint(x, y, movePreviewColor);',58,205,790,397,24);
steps(s,[['탐색 결과 사용','도달 가능한 칸인지 확인'],['목적지 검사','적이 점유한 칸은 선택 불가'],['미리보기 제거','ClearMovePreview()로 범위와 색 초기화']],877,214,330,124);
caption(s,'실제 CanMoveTo()는 배열 null, 좌표 범위, map 참조도 검사');
}
// 24
{
const s=slide('클릭한 타일로 플레이어 이동','BattlePreview의 입력을 Battle.Move_Player()에 전달');
table(s,[['입력과 검증','위치와 상태 반영'],['SelectDirection 단계에서 클릭 확인','목적지 월드 좌표 조회'],['레이캐스트로 타일 콜라이더 확인','SetPlayerTile()로 점유 상태 갱신'],['딕셔너리에서 타일 좌표 조회','미리보기 색상 제거'],['이동 범위 재계산 후 CanMoveTo() 검사','캐릭터 위치를 목적지에 직접 설정']],70,201,1140,360,[570,570],26);
caption(s,'현재는 이동 후 Finish_Anim()을 즉시 호출하며 행동 선택 단계로 돌아감');
}
// 25
{
const s=slide('마우스 위치로 공격 방향 결정','GetMouseDirection()');
code(s,'int dx = lastTileX - playerTile.x;\nint dy = lastTileY - playerTile.y;\n\nif (Mathf.Abs(dy) > Mathf.Abs(dx))\n{\n    // dy > 0이면 위, 그 외에는 아래\n}\nelse\n{\n    // dx > 0이면 오른쪽, 그 외에는 왼쪽\n}',62,205,760,402,25);
steps(s,[['좌표 차이 계산','플레이어 칸에서 마우스 칸까지의 차이'],['큰 축 선택','세로 차이가 더 크면 위·아래'],['가로 우선','두 차이가 같으면 왼쪽·오른쪽']],856,214,350,122);
caption(s,'마우스가 맵 밖으로 나가면 마지막으로 확인한 타일 좌표를 유지');
}
// 26
{
const s=slide('무기 공격 패턴을 4방향으로 회전','위쪽 기준 상대 좌표를 회전한 뒤 플레이어 좌표에 더함');
table(s,[['방향','회전식','상대 좌표 (1, 2)의 결과'],['위','(x, y)','(1, 2)'],['아래','(-x, -y)','(-1, -2)'],['오른쪽','(y, -x)','(2, -1)'],['왼쪽','(-y, x)','(-2, 1)']],105,199,1070,330,[215,300,555],28);
txt(s,'최종 공격 칸 = 플레이어 좌표 + 회전한 상대 좌표',100,563,1080,50,31,{align:'center',bold:true,color:C.blue});
caption(s,'예: 플레이어 (2, 2) + 오른쪽으로 회전한 (2, -1) = 공격 칸 (4, 1)');
}
// 27
{
const s=slide('코드 PlayerAttackPreview','검·창·총은 무기 범위를 플레이어 위치와 방향에 맞춰 표시');
code(s,'foreach (AttackCell attackCell in weapon.weaponRD)\n{\n    Vector2Int targetTile = attackCell.cell;\n    Vector2Int getMouseDirection =\n        playerTile + GetMouseDirection(targetTile);\n\n    Paint(getMouseDirection.x,\n          getMouseDirection.y,\n          attackPreviewColor);\n}',58,207,800,383,24);
steps(s,[['무기 범위 읽기','weaponRD의 상대 좌표를 순회'],['회전과 위치 이동','방향을 적용하고 플레이어 좌표 추가'],['타일 색칠','맵 범위와 렌더러 존재를 확인']],888,210,318,124);
caption(s,'Paint()는 범위를 표시하며, 실제 적 명중이나 피해를 판정하지 않음');
}
// 28
{
const s=slide('공격 방향이 바뀌면 프리뷰 갱신','RefreshAttackPreview()와 ResetPreviewColors()');
code(s,'if (battle.CurrentStep !=\n    Battle.Battle_Step.SelectAttack)\n    return;\nif (currentWeapon == null)\n    return;\n\nVector2Int direction = GetMouseDirection(Vector2Int.up);\nif (direction == lastDirection)\n    return;\n\nlastDirection = direction;\nPlayerAttackPreview(currentWeapon);',58,191,810,427,23);
steps(s,[['상태 확인','공격 단계이며 무기를 골랐는지 확인'],['방향 비교','이전과 같은 방향이면 다시 칠하지 않음'],['표시 갱신','기존 색 복원 후 새 공격 범위 표시']],898,211,310,124);
}
// 29
{
const s=slide('현재 구현한 범위','맵 데이터와 입력을 연결해 이동·공격 범위를 표시');
table(s,[['구현한 기능','추가 연결이 필요한 기능'],['좌표 기반 타일 생성과 데이터 조회','플레이어 시작 위치의 이동 조건 검사'],['BFS 이동 범위와 목적지 검증','이동 경로 애니메이션'],['클릭 이동과 점유 상태 갱신','이동 후 적 행동 진행'],['검·창·총의 방향별 공격 범위 표시','마법 범위 규칙, 공격 확정과 피해 적용']],70,205,1140,365,[570,570],26);
caption(s,'현재 Magic 분기는 무기 셀 좌표를 그대로 맵에 표시하는 임시 처리');
}
// 30
{
const s=slide('후속 작업 제안','구현 상태를 바탕으로 정리한 다음 작업');
table(s,[['순서','작업','확인할 결과'],['1','시작 위치와 이동 예외 보완','이동 불가 칸에서 시작하지 않는지 확인'],['2','마법 공격 범위 규칙 적용','의도한 기준 위치에 범위가 표시되는지 확인'],['3','공격 확정과 피해 처리 연결','프리뷰와 실제 공격 결과가 일치하는지 확인'],['4','적 행동과 턴 진행 연결','플레이어 행동 뒤 적 턴으로 이어지는지 확인']],62,203,1156,370,[110,475,571],25);
caption(s,'확정 일정이 아닌 작업 제안. 팀 일정에 맞춰 우선순위 조정');
}
const notes=JSON.parse(await fs.readFile(root+'/.build/notes.json','utf8'));
for(let i=0;i<p.slides.items.length;i++){const n=notes[i];if(!n||n.number!==i+1)throw Error('notes mismatch '+i);n.title=titles[i];p.slides.items[i].speakerNotes.text=n.script+'\n\n[화면 설명] '+n.cue+'\n\n[근거] '+n.source;}
const script='9주차 개발일지 발표 대본\r\n\r\n슬라이드 1~30 순서에 맞춘 대본입니다. 각 장의 화면 안내는 발표 중 가리킬 부분입니다.\r\n\r\n'+notes.map(n=>`${String(n.number).padStart(2,'0')}. ${n.title}\r\n\r\n${n.script}\r\n\r\n[화면 안내] ${n.cue}`).join('\r\n\r\n────────────────────────────────────────\r\n\r\n');
await fs.writeFile(root+'/final/9주차_개발일지_발표대본.txt','\uFEFF'+script,'utf8');
await fs.mkdir(root+'/.build/render',{recursive:true});
await (await PresentationFile.exportPptx(p)).save(root+'/.build/candidate.pptx');
await fs.writeFile(root+'/.build/final-inspect.ndjson',(await p.inspect({kind:'slide,textbox,table,notes',maxChars:300000})).ndjson);
for(let i=0;i<p.slides.items.length;i++){const b=await p.export({slide:p.slides.items[i],format:'png',scale:1});await fs.writeFile(`${root}/.build/render/slide-${String(i+1).padStart(2,'0')}.png`,new Uint8Array(await b.arrayBuffer()));console.log('Rendered',i+1);}
console.log('Done',p.slides.items.length);
