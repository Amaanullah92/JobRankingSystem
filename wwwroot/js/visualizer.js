const visualizer = {
    renderTrace: (trace, containerId) => {
        const container = document.getElementById(containerId);
        container.innerHTML = ''; // Clear

        // Simple linear visualization of the FINAL step state or animation loop
        // For a true animation, we'd use setInterval. For this MVP, let's show the final array state
        // and allow user to click steps in the log.
    },

    logTrace: (trace, containerId) => {
        const container = document.getElementById(containerId);
        container.innerHTML = `<h6>${trace.algorithmName} Trace</h6>`;

        trace.steps.forEach(step => {
            const div = document.createElement('div');
            div.className = 'trace-step';

            let vars = '';
            if (step.variables) {
                vars = Object.entries(step.variables).map(([k, v]) => `<span class="badge bg-secondary me-1">${k}: ${v}</span>`).join('');
            }

            div.innerHTML = `
                <div><strong>Step ${step.stepId}:</strong> ${step.description}</div>
                <div class="mt-1">${vars}</div>
            `;

            // Hover to visualize state?
            // In a full implementation, clicking this would re-render the visualizer box with 'step.stateSnapshot'

            container.appendChild(div);
        });
    }
};
