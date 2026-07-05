const fs=require('fs');
const g=JSON.parse(fs.readFileSync('D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/knowledge-graph.json','utf8'));

const nodes=g.nodes.slice(0,200).map(n=>({
  id:n.id,label:n.name||n.id,group:n.type||'?',
  title:(n.summary||'').slice(0,150)
}));
const nodeIds=new Set(nodes.map(n=>n.id));
const edges=g.edges.filter(e=>nodeIds.has(e.source)&&nodeIds.has(e.target)).slice(0,500).map(e=>({
  from:e.source,to:e.target,label:e.type
}));

const colors={file:'#4e79a7',class:'#f28e2b',function:'#e15759',document:'#59a14f',config:'#76b7b2'};
const groups=[...new Set(nodes.map(n=>n.group))];

const html='<!DOCTYPE html>\n<html lang="zh">\n<head>\n<meta charset="utf-8">\n<title>GenBall_Impact</title>\n'
+'<script src="https://cdn.bootcdn.net/ajax/libs/vis/4.21.0/vis.min.js"></script>\n'
+'<link href="https://cdn.bootcdn.net/ajax/libs/vis/4.21.0/vis.min.css" rel="stylesheet">\n'
+'<style>\nbody{margin:0;font:14px/1.5 "Microsoft YaHei",sans-serif;overflow:hidden}\n'
+'#network{width:100vw;height:100vh;background:#f5f6fa}\n'
+'#info{position:fixed;top:12px;left:12px;background:#fff;padding:8px 14px;border-radius:6px;box-shadow:0 2px 10px rgba(0,0,0,.1);z-index:99;font-size:13px;max-width:350px}\n'
+'.legend{position:fixed;bottom:16px;right:16px;background:#fff;padding:8px 12px;border-radius:6px;box-shadow:0 2px 10px rgba(0,0,0,.1);z-index:99;font-size:12px;line-height:1.8}\n'
+'.legend i{display:inline-block;width:10px;height:10px;border-radius:2px;margin-right:5px;vertical-align:middle}\n'
+'</style>\n</head>\n<body>\n'
+'<div id="info"><b>GenBall_Impact</b><br>'+nodes.length+' nodes . '+edges.length+' edges</div>\n'
+'<div class="legend">'+groups.map(g=>'<div><i style=background:'+(colors[g]||'#999')+'></i> '+g+'</div>').join('')+'</div>\n'
+'<div id="network"></div>\n<script>\n'
+'window.onload=function(){\n'
+'var nodes=new vis.DataSet('+JSON.stringify(nodes)+');\n'
+'var edges=new vis.DataSet('+JSON.stringify(edges)+');\n'
+'var c=document.getElementById("network");\n'
+'if(!c){alert("Container not found!");return;}\n'
+'var opts={physics:{barnesHut:{gravitationalConstant:-2000,centralGravity:0.2,springLength:200,springConstant:0.04,damping:0.09},stabilization:{iterations:150}},nodes:{color:{background:"#4e79a7",border:"#2c5aa0"},font:{size:11}},edges:{smooth:true,arrows:{to:{enabled:false}}}};\n'
+'var net=new vis.Network(c,{nodes:nodes,edges:edges},opts);\n'
+'net.on("click",function(p){if(p.nodes.length){var n=nodes.get(p.nodes[0]);var el=document.getElementById("info");el.innerHTML="<b>"+n.label+"</b><br><small>"+n.group+"</small><br>"+(n.title||"")}});\n'
+'net.once("stabilizationIterationsDone",function(){net.setOptions({physics:false})});\n'
+'};\n'
+'</script>\n</body>\n</html>';

fs.writeFileSync('D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/architecture.html',html,'utf8');
console.log('Done:',(fs.statSync('D:/Apps/Unity/Unity Project/GenBall_Impact/.understand-anything/architecture.html').size/1024).toFixed(0),'KB');
