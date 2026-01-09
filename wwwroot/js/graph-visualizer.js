// graph-visualizer.js
// Simple Canvas Visualization for Skill Graph

function drawGraph(adjList, canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const width = canvas.width = canvas.offsetWidth;
    const height = canvas.height = 500;

    ctx.clearRect(0, 0, width, height);

    // 1. Convert Dictionary to Nodes and Edges
    const nodes = Object.keys(adjList);
    const n = nodes.length;
    if (n === 0) return;

    // 2. Position Nodes in a Circle
    const centerX = width / 2;
    const centerY = height / 2;
    const radius = Math.min(width, height) / 2 - 50;
    
    const nodePositions = {};
    
    nodes.forEach((node, i) => {
        const angle = (i / n) * 2 * Math.PI;
        nodePositions[node] = {
            x: centerX + radius * Math.cos(angle),
            y: centerY + radius * Math.sin(angle)
        };
    });

    // 3. Draw Edges
    ctx.strokeStyle = "#aaa";
    ctx.lineWidth = 1;

    nodes.forEach(u => {
        const neighbors = adjList[u];
        if (neighbors) {
            Object.keys(neighbors).forEach(v => {
                const uPos = nodePositions[u];
                const vPos = nodePositions[v];
                // Draw line
                ctx.beginPath();
                ctx.moveTo(uPos.x, uPos.y);
                ctx.lineTo(vPos.x, vPos.y);
                ctx.stroke();
            });
        }
    });

    // 4. Draw Nodes
    nodes.forEach(u => {
        const pos = nodePositions[u];
        
        ctx.beginPath();
        ctx.arc(pos.x, pos.y, 20, 0, 2 * Math.PI);
        ctx.fillStyle = "#007bff";
        ctx.fill();
        ctx.strokeStyle = "#fff";
        ctx.stroke();

        ctx.fillStyle = "white"; // Text color inside bubble? No, messy.
        // Label outside
        ctx.fillStyle = "#333";
        ctx.font = "12px Arial";
        ctx.fillText(u, pos.x - 15, pos.y - 25);
    });
}
