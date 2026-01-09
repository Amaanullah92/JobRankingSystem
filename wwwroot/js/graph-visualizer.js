class GraphVisualizer {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) {
            console.error(`Canvas element #${canvasId} not found`);
            return;
        }
        this.ctx = this.canvas.getContext('2d');
        this.nodes = [];
        this.edges = [];
        this.hoveredNode = null;
        this.highlightedNode = null;
        this.neighborNodes = new Set();

        // Configuration
        this.config = {
            nodeRadius: 20,
            nodeColor: '#3b82f6',
            nodeColorHover: '#ef4444',
            nodeColorHighlight: '#ef4444',   // Red for selected
            nodeColorNeighbor: '#93c5fd',    // Light Blue for connected
            edgeColor: '#94a3b8',
            textColor: '#ffffff',
            textColorOutside: '#1e293b',
            fontSize: '12px Inter'
        };

        // Event Listeners
        this.canvas.addEventListener('mousemove', (e) => this.handleMouseMove(e));

        // Robust Resizing: Use ResizeObserver to detect when the container (page) is shown
        this.observer = new ResizeObserver(() => this.resize());
        if (this.canvas.parentElement) {
            this.observer.observe(this.canvas.parentElement);
        }

        // Initial resize attempt
        setTimeout(() => this.resize(), 500);
    }

    resize() {
        if (!this.canvas) return;
        const parent = this.canvas.parentElement;
        const rect = parent.getBoundingClientRect();

        if (rect.width === 0 || rect.height === 0) return; // Hidden

        this.canvas.width = rect.width;
        this.canvas.height = rect.height;
        this.draw();
    }

    // New Method: Highlight a specific node and its neighbors
    highlightNode(skillName) {
        if (!skillName) {
            this.highlightedNode = null;
            this.neighborNodes.clear();
            this.draw();
            return;
        }

        const node = this.nodes.find(n => n.id.toLowerCase() === skillName.toLowerCase());

        if (node) {
            this.highlightedNode = node;
            this.neighborNodes.clear();

            // Find neighbors through edges
            this.edges.forEach(edge => {
                if (edge.source === node) this.neighborNodes.add(edge.target);
                if (edge.target === node) this.neighborNodes.add(edge.source);
            });
        } else {
            this.highlightedNode = null;
            this.neighborNodes.clear();
        }
        this.draw();
    }

    setData(graphData) {
        console.log("GraphVisualizer received data:", graphData);
        this.fullGraphData = graphData; // Store full data for filtering
        this.renderGraph(graphData);
    }

    renderGraph(graphData) {
        this.nodes = [];
        this.edges = [];

        // graphData is { "SkillA": { "SkillB": weight }, ... }
        const skills = Object.keys(graphData);
        if (skills.length === 0) {
            // console.warn("No skills found in graph data.");
            this.draw(); // Clear
            return;
        }

        // Simple Circle Layout
        const centerX = this.canvas.width / 2;
        const centerY = this.canvas.height / 2;
        const radius = Math.min(centerX, centerY) - 80; // Padding
        const angleStep = (2 * Math.PI) / skills.length;

        skills.forEach((skill, index) => {
            this.nodes.push({
                id: skill,
                x: centerX + radius * Math.cos(index * angleStep),
                y: centerY + radius * Math.sin(index * angleStep),
                radius: this.config.nodeRadius,
                label: skill
            });
        });

        // Create Edges
        skills.forEach(source => {
            const targets = graphData[source];
            const sourceNode = this.nodes.find(n => n.id === source);

            for (const [target, weight] of Object.entries(targets)) {
                const targetNode = this.nodes.find(n => n.id === target);
                if (sourceNode && targetNode) {
                    // Check if edge already exists (undirected visual)
                    const exists = this.edges.some(e =>
                        (e.source === sourceNode && e.target === targetNode) ||
                        (e.source === targetNode && e.target === sourceNode)
                    );

                    if (!exists) {
                        this.edges.push({
                            source: sourceNode,
                            target: targetNode,
                            weight: weight
                        });
                    }
                }
            }
        });

        console.log(`Parsed ${this.nodes.length} nodes and ${this.edges.length} edges.`);
        this.draw();
    }

    filterGraph(query) {
        if (!query || query.trim() === "") {
            this.renderGraph(this.fullGraphData);
            return;
        }

        const lowerQuery = query.toLowerCase();
        const filteredData = {};

        // Find the main matching skill(s)
        const matchingSkills = Object.keys(this.fullGraphData).filter(s => s.toLowerCase().includes(lowerQuery));

        matchingSkills.forEach(mainSkill => {
            if (!filteredData[mainSkill]) filteredData[mainSkill] = {};

            // Add all its neighbors
            const neighbors = this.fullGraphData[mainSkill];
            for (const [neighbor, weight] of Object.entries(neighbors)) {
                filteredData[mainSkill][neighbor] = weight;

                // Ensure neighbor exists in filteredData (even if it has no outgoing edges added yet)
                // We need to verify if the neighbor itself has edges in fullGraphData pointing back
                // For visualization, we just need the node to exist. 
                // But our renderGraph iterates keys of passed object.
                if (!filteredData[neighbor]) {
                    filteredData[neighbor] = {};
                }

                // Also add the back-link from neighbor to mainSkill if it exists in full data
                // (Assuming undirected or bidirectional, but following data structure)
                if (this.fullGraphData[neighbor] && this.fullGraphData[neighbor][mainSkill]) {
                    filteredData[neighbor][mainSkill] = this.fullGraphData[neighbor][mainSkill];
                }
            }
        });

        this.renderGraph(filteredData);
    }

    draw() {
        if (!this.ctx) return;

        // Clear
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        // Show placeholder if empty
        if (this.nodes.length === 0) {
            this.ctx.font = '16px Inter';
            this.ctx.textAlign = 'center';
            this.ctx.textBaseline = 'middle';

            // Check dark mode
            const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
            this.ctx.fillStyle = isDark ? '#94a3b8' : '#64748b';

            this.ctx.fillText("Type a skill above to explore connections...", this.canvas.width / 2, this.canvas.height / 2);
            return;
        }

        // Draw Edges
        this.ctx.lineWidth = 1;
        this.edges.forEach(edge => {
            this.ctx.beginPath();

            // Highlight edge if connected to highlighted node
            let isHighlightedEdge = false;
            if (this.highlightedNode && (edge.source === this.highlightedNode || edge.target === this.highlightedNode)) {
                isHighlightedEdge = true;
            }

            this.ctx.strokeStyle = isHighlightedEdge ? '#64748b' : this.config.edgeColor;
            this.ctx.lineWidth = isHighlightedEdge ? 2 : 1;

            this.ctx.moveTo(edge.source.x, edge.source.y);
            this.ctx.lineTo(edge.target.x, edge.target.y);
            this.ctx.stroke();
        });

        // Draw Nodes
        this.nodes.forEach(node => {
            this.ctx.beginPath();
            this.ctx.arc(node.x, node.y, node.radius, 0, 2 * Math.PI);

            // Determine Color
            let color = this.config.nodeColor;
            if (node === this.highlightedNode) {
                color = this.config.nodeColorHighlight;
            } else if (this.neighborNodes.has(node)) {
                color = this.config.nodeColorNeighbor;
            } else if (node === this.hoveredNode) {
                color = this.config.nodeColorHover;
            }

            this.ctx.fillStyle = color;
            this.ctx.fill();
            this.ctx.strokeStyle = '#fff';
            this.ctx.stroke();

            // Label logic
            this.ctx.fillStyle = this.config.textColor;
            this.ctx.font = this.config.fontSize;
            this.ctx.textAlign = 'center';
            this.ctx.textBaseline = 'middle';

            // Check if dark mode is active for outside text
            const isDark = document.documentElement.getAttribute('data-theme') === 'dark';

            if (node.label.length > 5 && !node.label.startsWith("C#")) { // Short labels inside
                // Draw underneath
                this.ctx.fillStyle = isDark ? '#f8fafc' : this.config.textColorOutside;
                this.ctx.fillText(node.label, node.x, node.y + node.radius + 15);
            } else {
                this.ctx.fillStyle = '#ffffff';
                this.ctx.fillText(node.label, node.x, node.y);
            }
        });
    }

    handleMouseMove(e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        let found = null;
        for (const node of this.nodes) {
            const dist = Math.sqrt((x - node.x) ** 2 + (y - node.y) ** 2);
            if (dist < node.radius) {
                found = node;
                break;
            }
        }

        if (this.hoveredNode !== found) {
            this.hoveredNode = found;
            this.canvas.style.cursor = found ? 'pointer' : 'default';
            this.draw();
        }
    }
}
