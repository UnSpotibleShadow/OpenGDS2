// One-time, manually dispatched publication of the already tested ea.2 artifacts.
// Never builds a game, replaces an asset, moves a tag or publishes another version.
import {readFile, writeFile, rename, mkdir} from 'node:fs/promises';
import {execFile} from 'node:child_process';
import {createHash} from 'node:crypto';
import {join} from 'node:path';
import {createConnectorGuard} from './connector_guard.mjs';
const repo='UnSpotibleShadow/OpenGDS2', tag='v0.1.0-ea.2';
const source='fb4d8f2969b6288349f4f1db0b68f71438003e82', run=39251557441, releaseId=417249192;
const directory=process.argv[2], publish=process.argv.includes('--publish');
if(!directory)throw Error('Provide the directory containing the verified release artifacts.');
if(publish && (process.env.GITHUB_ACTIONS!=='true'||process.env.GITHUB_REPOSITORY!==repo||process.env.GITHUB_EVENT_NAME!=='workflow_dispatch'))
    throw Error('Publication is restricted to an explicit workflow_dispatch in this repository.');
const audit=join(process.env.RUNNER_TEMP??directory,'ea2-publication');await mkdir(audit,{recursive:true});
const exec=(args)=>new Promise((resolve,reject)=>execFile('gh',args,{timeout:40000,maxBuffer:2*1024*1024},(e,out,err)=>e?reject(Error((err||e.message).replace(/https?:\S+/g,'[URL omitted]'))):resolve(out)));
async function get(path,missing=false){try{return JSON.parse(await exec(['api',`repos/${repo}/${path}`]));}catch(e){if(missing&&String(e).includes('HTTP 404'))return null;throw e;}}
const sha=data=>createHash('sha256').update(data).digest('hex');
const files=[];
for(const [arch,digest] of [['x86_32','9bc13388152dd28cad48d2b5bd37452d3687522a5a77bf13cd80ecb3ea317dcd'],['x86_64','005eb0fc5fc7f29465dd02d7060cd45300c0134d7c5d827d7b1be2bbbf0e035e']]){
    const name=`OpenGDS_v0.1.0-ea.2_windows_${arch}.zip`, bytes=await readFile(join(directory,name)), sidecar=await readFile(join(directory,name+'.sha256'));
    if(sha(bytes)!==digest || sidecar.toString().trim()!==`${digest}  ${name}`)throw Error('Unverified artifact: '+name);
    files.push({name,size:bytes.length,digest},{name:name+'.sha256',size:sidecar.length,digest:sha(sidecar)});
}
const build=await get(`actions/runs/${run}`);
if(build.conclusion!=='success'||build.path!=='.github/workflows/main-build.yml')throw Error('Required build did not pass.');
const promotion=await get('pulls/162');
if(!promotion.merged||promotion.merge_commit_sha!==source)throw Error('Unexpected tested source.');
let release=await get(`releases/${releaseId}`);
if(release.target_commitish!==source)throw Error('Unexpected draft target.');
if(!release.draft && (release.tag_name!==tag||!release.prerelease))throw Error('Existing public release differs.');
console.log('Verified exact source, successful build, immutable ZIP hashes and checksum files.');
if(!publish){console.log('Read-only preflight complete.');process.exit(0);}
let pending=null;try{const p=JSON.parse(await readFile(join(audit,'checkpoint.json'),'utf8'));if(p.mutation&&['started','unknown'].includes(p.status))pending=p;}catch{}
const guard=createConnectorGuard({timeoutMs:45000,pendingWrite:pending,checkpoint:async state=>{await writeFile(join(audit,'checkpoint.tmp'),JSON.stringify(state,null,2)+'\n');await rename(join(audit,'checkpoint.tmp'),join(audit,'checkpoint.json'));}});
async function mutate(label,args,expected,verify){const result=await guard.run({label,mutation:true,expected,call:()=>exec(args),verify});if(result.status!=='verified')throw Error(label+': '+result.status+'; reconcile remote state before retrying.');}
async function asset(file){return (await get(`releases/${releaseId}/assets`)).find(a=>a.name===file.name);}
function matches(a,f){return a?.state==='uploaded'&&a.size===f.size&&(!a.digest||a.digest===`sha256:${f.digest}`);}
for(const file of files){
    const existing=await asset(file);
    if(existing){if(!matches(existing,file)||!existing.digest)throw Error('Existing asset is not proven identical: '+file.name);continue;}
    if(!release.draft)throw Error('Never add missing assets to an already published release.');
    await mutate('upload '+file.name,['api','--method','POST',`https://uploads.github.com/repos/${repo}/releases/${releaseId}/assets?name=${file.name}`,'-H','Content-Type: application/octet-stream','--input',join(directory,file.name)],{releaseId,name:file.name,size:file.size,sha256:file.digest},async()=>matches(await asset(file),file));
}
let ref=await get(`git/ref/tags/${tag}`,true);
if(ref && ref.object.sha!==source)throw Error('Never move an existing release tag.');
if(!ref)await mutate('create tested ea2 tag',['api','--method','POST',`repos/${repo}/git/refs`,'-f',`ref=refs/tags/${tag}`,'-f',`sha=${source}`],{tag,source},async()=>(await get(`git/ref/tags/${tag}`)).object.sha===source);
const notes=(await readFile('docs/release-0.1.0-ea.2.md','utf8'))+`\n## Verified build\n\nSource: \`${source}\`. [Build and packaged Windows checks](https://github.com/${repo}/actions/runs/${run}) passed for both release architectures. Exact matching Linux-export original-LZH import, new company, office rendering and save/reload also passed. These are automated results, not a new manual Windows playthrough.\n`;
release=await get(`releases/${releaseId}`);
if(release.draft){
    const body={tag_name:tag,target_commitish:source,name:'Open Game Dev Story II v0.1.0-ea.2 — Early Access',body:notes,draft:false,prerelease:true,make_latest:'false'};
    const path=join(audit,'publish.json');await writeFile(path,JSON.stringify(body));
    await mutate('publish verified ea2 prerelease',['api','--method','PATCH',`repos/${repo}/releases/${releaseId}`,'--input',path],{releaseId,tag,source},async()=>{const r=await get(`releases/${releaseId}`);return !r.draft&&r.prerelease&&r.tag_name===tag&&r.target_commitish===source;});
}
const verified=[];
for(const file of files){
    const url=`https://github.com/${repo}/releases/download/${tag}/${file.name}`;
    const response=await fetch(url,{signal:AbortSignal.timeout(120000)});
    if(!response.ok)throw Error('Anonymous download failed: '+file.name+' '+response.status);
    const digest=createHash('sha256');let size=0;
    for await(const chunk of response.body){digest.update(chunk);size+=chunk.length;}
    if(size!==file.size||digest.digest('hex')!==file.digest)throw Error('Public download differs: '+file.name);
    verified.push({...file,url});
}
await writeFile(join(audit,'verified-publication.json'),JSON.stringify({tag,source,buildRun:run,releaseId,anonymousDownloadsVerified:true,assets:verified},null,2)+'\n');
console.log(`Published ${tag}; all four anonymous downloads and SHA-256 digests verified.`);
