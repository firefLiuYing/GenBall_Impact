const fs=require("fs");
const g=JSON.parse(fs.readFileSync("D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/knowledge-graph.json","utf8"));

// Group files by module
const modules={};
for(const n of g.nodes){
  if(n.type!=="file"||!n.filePath) continue;
  const parts=n.filePath.split("/");
  let mod="other";
  if(parts[0]==="Assets"&&parts[1]==="Scripts"){
    if(parts[2]==="Yueyn") mod="Yueyn/"+ (parts[3]||"");
    else if(parts[2]==="GenBall") mod="GenBall/"+ (parts[3]||"");
  }
  if(!modules[mod]) modules[mod]={files:[],classes:[]};
  modules[mod].files.push(n);
}
for(const n of g.nodes){
  if(n.type!=="class") continue;
  const fp=n.filePath||n.id.replace(/^class:/,"").replace(/:.*/,"");
  const parts=fp.split("/");
  let mod="other";
  if(parts[0]==="Assets"&&parts[1]==="Scripts"){
    if(parts[2]==="Yueyn") mod="Yueyn/"+ (parts[3]||"");
    else if(parts[2]==="GenBall") mod="GenBall/"+ (parts[3]||"");
  }
  if(!modules[mod]) modules[mod]={files:[],classes:[]};
  modules[mod].classes.push(n);
}

// File-to-module mapping
const fileToMod={};
for(const n of g.nodes){
  if(n.type==="file"&&n.filePath){
    const p=n.filePath.split("/");
    if(p[0]==="Assets"&&p[1]==="Scripts") fileToMod["file:"+n.filePath]=p[2]+"/"+(p[3]||"");
  }
}

// Inter-module edges
const modEdges={};
for(const e of g.edges){
  if(e.type==="depends_on"||e.type==="calls"||e.type==="implements"){
    const sm=fileToMod[e.source]||"";
    const tm=fileToMod[e.target]||"";
    if(sm&&tm&&sm!==tm){const k=sm+"|"+tm;modEdges[k]=(modEdges[k]||0)+1;}
  }
}

const modNames=Object.keys(modules).sort();
const ids={};
modNames.forEach((name,i)=>{ids[name]="M"+i});

// Mermaid module graph
let md="```mermaid\ngraph TD\n";
for(const name of modNames){
  const count=modules[name].files.length;
  const cls=modules[name].classes.length;
  md+="  "+ids[name]+"[\""+name+"<br/>"+count+" files, "+cls+" classes\"]\n";
}
const topEdges=[...Object.entries(modEdges)].sort((a,b)=>b[1]-a[1]).slice(0,20);
for(const [k,w] of topEdges){
  const [s,t]=k.split("|");
  if(ids[s]&&ids[t]) md+="  "+ids[s]+" -->|\""+w+"\"| "+ids[t]+"\n";
}
md+="```\n";

// Module detail tables
for(const name of modNames.slice(0,15)){
  md+="\n### "+name+"\n";
  md+="| Class | Summary |\n|---|---|\n";
  const cls=modules[name].classes.slice(0,20);
  for(const c of cls){
    const summary=(c.summary||"-").slice(0,100).replace(/\|/g,"/").replace(/\n/g," ");
    md+="| **"+c.name+"** | "+summary+" |\n";
  }
  if(modules[name].classes.length>20) md+="| ... | +"+(modules[name].classes.length-20)+" more classes |\n";
}

fs.writeFileSync("D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/architecture.md",md,"utf8");
console.log("Modules:",modNames.length,"Cross-module edges:",Object.keys(modEdges).length);
console.log("Written architecture.md");
