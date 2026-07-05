const fs=require("fs");
const g=JSON.parse(fs.readFileSync("D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/knowledge-graph.json","utf8"));

// Group files by module (5 levels deep)
const tree={};
for(const n of g.nodes){
  if(n.type!=="file"||!n.filePath) continue;
  const parts=n.filePath.split("/");
  let cur=tree;
  for(let i=0;i<Math.min(parts.length,5);i++){
    if(!cur[parts[i]]) cur[parts[i]]={_files:[]};
    if(i===Math.min(parts.length,4)) cur[parts[i]]._files.push(n);
    cur=cur[parts[i]];
  }
}

function renderTree(node,indent){
  let out="";
  const keys=Object.keys(node).filter(k=>k!=="_files"&&!k.endsWith(".cs")&&!k.endsWith(".md")).sort();
  for(const k of keys){
    const files=node[k]._files||[];
    out+=indent+"- **"+k+"/** ("+files.length+" files)\n";
    for(const f of files.slice(0,5)){
      const s=(f.summary||"").slice(0,80);
      out+=indent+"  - `"+f.name+"`"+(s?" — "+s:"")+"\n";
    }
    if(files.length>5) out+=indent+"  - ... +"+(files.length-5)+" more\n";
    out+=renderTree(node[k],indent+"  ");
  }
  return out;
}

let out="# GenBall_Impact 代码索引\n\n";
out+="> 从 "+g.nodes.length+" 个节点自动生成 | commit: "+g.project.gitCommitHash+"\n\n";

// Module tree
out+="## 模块结构\n\n";
out+=renderTree(tree,"");

// ISystem registrations (find in FrameworkDefault)
const ff=g.nodes.find(n=>n.name==="FrameworkDefault");
out+="\n## 架构要点 (来自图谱分析)\n\n";

// Interfaces and implementations
const impls=g.edges.filter(e=>e.type==="implements");
const ifaceMap={};
for(const e of impls){
  const impl=g.nodes.find(n=>n.id===e.source);
  const iface=g.nodes.find(n=>n.id===e.target);
  if(impl&&iface){
    if(!ifaceMap[iface.name]) ifaceMap[iface.name]=[];
    ifaceMap[iface.name].push(impl.name);
  }
}

out+="### 接口 → 实现\n\n";
out+="| 接口 | 实现 |\n|---|---|\n";
for(const [iface,impls] of Object.entries(ifaceMap).sort()){
  out+="| **"+iface+"** | "+impls.join(", ")+" |\n";
}

// Dependencies
out+="\n### 关键依赖关系\n\n";
const deps=g.edges.filter(e=>e.type==="depends_on").slice(0,30);
for(const e of deps){
  const s=g.nodes.find(n=>n.id===e.source);
  const t=g.nodes.find(n=>n.id===e.target);
  if(s&&t) out+="- `"+s.name+"` → `"+t.name+"`\n";
}

fs.writeFileSync("D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/code-index.md",out,"utf8");
console.log("Written code-index.md ("+(fs.statSync("D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/code-index.md").size/1024).toFixed(0)+" KB)");
