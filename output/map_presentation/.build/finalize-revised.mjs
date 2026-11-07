import path from 'node:path';
const base='C:/Users/JANG/OneDrive/Documents/GitHub/ProjectJ/output/map_presentation';
const skill='C:/Users/JANG/.codex/plugins/cache/openai-primary-runtime/presentations/26.1026.10807/skills/presentations';
process.env.RUNTIME_NODE_MODULES='C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules';
const {finalizePresentation}=await import('file:///'+skill+'/container_tools/artifact_tool_utils.mjs');
const result=await finalizePresentation({workspaceDir:base,candidatePath:base+'/.build/revised-candidate.pptx',finalPath:base+'/final/9주차_개발일지_코드수정본.pptx',pythonExecutable:'C:/Users/JANG/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe',integrityValidatorPath:skill+'/container_tools/inspect_presentation_package_integrity.py',layoutValidatorPath:skill+'/container_tools/inspect_presentation_layout_geometry.py',layoutArgs:['--expected-slide-size-emu','12192000,6858000','--validate-bullet-geometry','--validate-heading-fit',...[7,9,11,17,18,19,20,21,24,26,29,30].flatMap(n=>['--require-native-table-slide',String(n)])],explicitTotalSlideCount:30,requiredNativeTableOwnerSlides:[7,9,11,17,18,19,20,21,24,26,29,30],requiredNativeChartOwnerSlides:[],verifyArtifactToolImport:true,receiptPath:base+'/.build/revised-validation.json'});
console.log(JSON.stringify(result));


