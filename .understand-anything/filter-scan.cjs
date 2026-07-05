const fs=require('fs');
const path=require('path');
function walk(dir,exts){
  const r=[];
  try{
    for(const e of fs.readdirSync(dir,{withFileTypes:true})){
      if(e.name.startsWith('.')||e.name==='Editor'||e.name==='Generated') continue;
      const f=path.join(dir,e.name);
      if(e.isDirectory()) r.push(...walk(f,exts));
      else if(exts.some(x=>e.name.endsWith(x))) r.push(f);
    }
  }catch(ex){}
  return r;
}
const root='D:/Apps/Unity/Unity Project/GenBall_Impact';
const all=[
  ...walk(root+'/Assets/Scripts/GenBall',['.cs','.md']),
  ...walk(root+'/Assets/Scripts/Yueyn',['.cs','.md'])
];
const rels=all.map(p=>p.replace(/\\/g,'/').replace(root.replace(/\\/g,'/')+'/',''));
console.log('Total files:',rels.length);

const coreDirs=[
  'Assets/Scripts/Yueyn/',
  'Assets/Scripts/GenBall/Framework/',
  'Assets/Scripts/GenBall/BattleSystem/Framework/',
  'Assets/Scripts/GenBall/BattleSystem/Buff/',
  'Assets/Scripts/GenBall/BattleSystem/Command/',
  'Assets/Scripts/GenBall/BattleSystem/Executors/',
  'Assets/Scripts/GenBall/BattleSystem/Weapons/',
  'Assets/Scripts/GenBall/Event/',
  'Assets/Scripts/GenBall/Player/',
  'Assets/Scripts/GenBall/Procedure/',
  'Assets/Scripts/GenBall/Entry/',
];

const files=[];
for(const rel of rels){
  let ok=false;
  for(const d of coreDirs){if(rel.startsWith(d)||rel===d){ok=true;break;}}
  if(!ok) continue;
  if(rel.includes('/Editor/')) continue;
  if(rel.includes('/Generated/')||rel.includes('.Generated.')||rel.includes('.Bind.')) continue;
  try{
    const c=fs.readFileSync(root+'/'+rel,'utf8');
    files.push({path:rel,language:rel.endsWith('.md')?'markdown':'csharp',sizeLines:c.split('\n').length,fileCategory:rel.endsWith('.md')?'docs':'code'});
  }catch(e){}
}
console.log('Core filtered:',files.length);

fs.mkdirSync(root+'/.understand-anything/intermediate',{recursive:true});
fs.writeFileSync(root+'/.understand-anything/intermediate/scan-result.json',JSON.stringify({
  projectName:'GenBall_Impact_Core',projectDescription:'Yueyn+GenBall+BattleEntity+ISystem',languages:['csharp'],frameworks:['Unity'],complexity:'large',files,importMap:{}
},null,2));
console.log('Done');
