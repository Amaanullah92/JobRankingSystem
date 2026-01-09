// visualizer.js
// Helper to render Algorithm Traces into the UI

function renderTrace(trace, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    if (!trace) {
        container.innerHTML = "";
        return;
    }

    let html = `<h6>${trace.algorithmName} - Execution Trace</h6>`;
    html += `<div class="trace-log">`;
    
    trace.steps.forEach(step => {
        // Format variables
        let vars = "";
        if (step.variables) {
            vars = " | " + Object.entries(step.variables)
                .map(([k, v]) => `<span class="text-info">${k}=${v}</span>`)
                .join(", ");
        }

        html += `
            <div class="step-item">
                <span class="text-warning">[Step ${step.stepId}]</span> 
                ${step.description} 
                ${vars}
            </div>
        `;
    });

    html += `</div>`;
    container.innerHTML = html;
}
