"""Standalone, searchable view of authored workflows and original graph metadata."""
import json


def render_view(inventory, catalog):
    payload = {key: inventory[key] for key in ("objects", "controls", "events", "procedures", "menus", "timers", "edges")}
    payload["catalog"] = catalog
    payload["counts"] = {key: len(inventory[key]) for key in ("objects", "controls", "events", "procedures", "menus", "instances", "pictures", "timers", "unresolved_dispatch")}
    # Inline data makes file:// use work without a local server or network.
    data = json.dumps(payload, ensure_ascii=True, separators=(",", ":")).replace("<", "\\u003c")
    return TEMPLATE.replace("__DATA__", data)


TEMPLATE = r'''<!doctype html>
<html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Original-game map · OpenGDS2</title>
<style>
*{box-sizing:border-box}body{margin:0;background:#101720;color:#e3eaf2;font:15px/1.5 system-ui,sans-serif}header{padding:24px 32px;border-bottom:1px solid #334355;background:#182432}h1{margin:0;font-size:26px}p{max-width:100ch}small,.muted{color:#b1c2d3}a{color:#80c5ff}input,select,button{font:inherit;color:inherit;background:#24374b;border:1px solid #5a7189;border-radius:5px;padding:8px}input{width:min(650px,100%)}label{display:inline-block;margin:8px 12px 0 0}main{display:grid;grid-template-columns:340px 1fr;gap:20px;padding:24px 32px}nav button{display:block;text-align:left;width:100%;margin:0 0 7px;background:#182432}nav button[aria-pressed=true]{border-color:#80c5ff;background:#28435d}.tag{display:inline-block;border:1px solid #5a7189;padding:2px 8px;border-radius:4px;margin:4px 6px 4px 0}.missing{color:#ffb3aa}.notice{color:#ffd893}section{min-width:0}h2{margin-top:0}h3{margin-bottom:7px}pre{white-space:pre-wrap;overflow-wrap:anywhere}table{width:100%;border-collapse:collapse;font-size:13px}td,th{text-align:left;padding:6px 9px;border-bottom:1px solid #334355;overflow-wrap:anywhere}th{color:#b1c2d3}details{margin:14px 0}summary{cursor:pointer;font-weight:600}ul{padding-left:22px}.counts{display:flex;flex-wrap:wrap;gap:10px;margin-top:15px}.counts span{background:#24374b;padding:5px 10px;border-radius:4px}.links a{display:block;margin:5px 0}code{font-size:12px}@media(max-width:800px){main{grid-template-columns:1fr;padding:16px}header{padding:16px}nav{max-height:300px;overflow:auto}}
</style>
<header><h1>Original-game map</h1><p>Explore the original executable's inventory, then follow the separate OpenGDS2 comparison links. Counts describe discovered items. <strong class="notice">Full behavior and native runtime parity remain unverified.</strong></p><small id="baseline"></small><div class="counts" id="counts"></div><label>Search workflows, forms or menus<br><input id="search" type="search" placeholder="Try backup, fGMDevW1, awards, W30…"></label><label>Adaptation disposition<br><select id="status"><option value="">All dispositions</option><option>missing</option><option>partial</option><option>comparison-present</option><option>intentional-difference</option><option>unresolved</option></select></label><p id="result-count" role="status"></p></header>
<main><nav id="workflows" aria-label="Workflows"></nav><section id="detail" aria-live="polite"></section></main>
<script type="application/json" id="data">__DATA__</script>
<script>
'use strict';
const d=JSON.parse(document.getElementById('data').textContent), byId=new Map(d.procedures.map(p=>[p.id,p]));
const el=(tag,text,parent,cls)=>{const n=document.createElement(tag);if(text!==undefined)n.textContent=text;if(cls)n.className=cls;if(parent)parent.append(n);return n;};
const link=(path,parent)=>{const a=el('a',path,parent);a.href='../'+path;};
const list=(items,parent)=>{const ul=el('ul',undefined,parent);items.forEach(t=>el('li',t,ul));};
const table=(headers,rows,parent)=>{const t=el('table',undefined,parent),thead=el('thead',undefined,t),tr=el('tr',undefined,thead);headers.forEach(h=>el('th',h,tr));const tbody=el('tbody',undefined,t);rows.forEach(row=>{const r=el('tr',undefined,tbody);row.forEach(v=>el('td',String(v??'unknown/default'),r));});};
document.getElementById('baseline').textContent='Original SHA-256 '+d.catalog.source_sha256+' · 0 / '+d.catalog.workflows.length+' workflows accepted on original runtime';
Object.entries(d.counts).forEach(([key,value])=>el('span',value.toLocaleString()+' '+key.replaceAll('_',' '),document.getElementById('counts')));
let selected=null;
function show(w){selected=w.id;document.querySelectorAll('nav button').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.id===w.id)));const p=document.getElementById('detail');p.replaceChildren();el('h2',w.id+' · '+w.title,p);el('span',w.adaptation,p,'tag '+(w.adaptation==='missing'?'missing':''));el('span','understanding: '+w.understanding,p,'tag');el('span','original runtime pending',p,'tag notice');el('h3','Original entry and flow',p);el('p',w.entry,p);el('p',w.sequence,p);el('h3','Original objects',p);list(w.objects,p);el('h3','Open questions',p);list(w.open_questions,p);el('h3','Evidence and comparison sources',p);const links=el('div',undefined,p,'links');[...w.evidence,...w.adaptation_files,...w.tests].forEach(path=>link(path,links));
const menus=d.catalog.menu_dispositions.filter(m=>m.workflows.includes(w.id));if(menus.length){const det=el('details',undefined,p);el('summary','Original menu dispositions ('+menus.length+')',det);table(['Original ID','Label','Disposition'],menus.map(m=>[m.id,m.label,m.disposition]),det);}
const objects=new Set(w.objects),ps=new Set(w.source_procedures||d.procedures.filter(x=>objects.has(x.object)).map(x=>x.id));
const eventRows=d.events.filter(e=>ps.has(e.procedure));const ev=el('details',undefined,p);el('summary','Bound event slots ('+eventRows.length+')',ev);table(['Original control','Slot','Procedure','Candidate role'],eventRows.map(e=>[e.control,e.slot,e.procedure,e.candidate_role]),ev);
const procs=d.procedures.filter(x=>ps.has(x.id)),pr=el('details',undefined,p);el('summary','Original procedures ('+procs.length+')',pr);table(['ID','Object','Bytes','Instructions','Status'],procs.map(x=>[x.id,x.object,x.size,x.instructions,x.status]),pr);
const edges=d.edges.filter(e=>ps.has(e.source)||ps.has(e.target)||objects.has(e.target)),gr=el('details',undefined,p);el('summary','Direct calls and object references ('+edges.length+')',gr);el('p','Object reference does not prove Show or modality. Dynamic dispatch targets are unresolved.',gr,'notice');table(['Source','At','Target','Kind'],edges.map(e=>[e.source,'+0x'+e.at.toString(16),e.target,e.kind]),gr);
const timers=d.timers.filter(t=>objects.has(t.object)),tm=el('details',undefined,p);el('summary','Serialized timers ('+timers.length+')',tm);table(['Control','Interval ms','Explicit Enabled','Callbacks'],timers.map(t=>[t.control,t.serialized.Interval,t.serialized.Enabled,t.bound_procedures.join(', ')]),tm);
const sc=el('details',undefined,p);el('summary','Pending original/runtime scenarios ('+w.scenarios.length+')',sc);w.scenarios.forEach(c=>{el('h3',c.id+' · '+c.path,sc);el('p',c.observe,sc);el('p','Status: '+c.status,sc,'notice');});
el('p','Original assets, executable bytes, captions, saves and full traces are excluded. Open original-game-map.md for reproduction commands and the prioritized gap queue.',p,'muted');}
function filter(){const q=document.getElementById('search').value.toLowerCase().trim(),status=document.getElementById('status').value;const rows=d.catalog.workflows.filter(w=>(!status||w.adaptation===status)&&(!q||JSON.stringify(w).toLowerCase().includes(q)||d.catalog.menu_dispositions.some(m=>m.workflows.includes(w.id)&&JSON.stringify(m).toLowerCase().includes(q))));const nav=document.getElementById('workflows');nav.replaceChildren();rows.forEach(w=>{const b=el('button',w.id+' · '+w.title,nav);b.dataset.id=w.id;b.type='button';b.setAttribute('aria-pressed',String(selected===w.id));el('small',' · '+w.adaptation,b);b.onclick=()=>show(w);});document.getElementById('result-count').textContent=rows.length+' of '+d.catalog.workflows.length+' workflows';if(rows.length&&!rows.some(w=>w.id===selected))show(rows[0]);if(!rows.length){document.getElementById('detail').replaceChildren();selected=null;}}
document.getElementById('search').addEventListener('input',filter);document.getElementById('status').addEventListener('change',filter);filter();
</script></html>
'''
