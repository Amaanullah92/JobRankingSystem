const API_URL = '/api';

const app = {
    state: {
        candidates: [],
        jobs: [],
        trace: null,
        explainMode: false,
        theme: localStorage.getItem('theme') || 'light',
        notifications: []
    },

    addNotification: (message, type = 'info') => {
        const id = Date.now();
        const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        app.state.notifications.unshift({ id, message, type, time });
        if (app.state.notifications.length > 20) app.state.notifications.pop(); // Keep last 20

        app.renderNotifications();

        // Show badge
        const badge = document.getElementById('notification-badge');
        if (badge) badge.classList.remove('d-none');
    },

    renderNotifications: () => {
        const list = document.getElementById('notification-list');
        if (!list) return;

        if (app.state.notifications.length === 0) {
            list.innerHTML = `
                <li><h6 class="dropdown-header text-uppercase small fw-bold">Recent Activity</h6></li>
                <li><hr class="dropdown-divider"></li>
                <li class="px-3 py-2 text-muted small text-center">No new notifications</li>`;
            return;
        }

        let html = `<li><h6 class="dropdown-header text-uppercase small fw-bold">Recent Activity</h6></li>
                    <li><hr class="dropdown-divider"></li>`;

        app.state.notifications.forEach(n => {
            const iconMap = {
                'success': '<span class="text-success">●</span>',
                'info': '<span class="text-primary">●</span>',
                'error': '<span class="text-danger">●</span>'
            };

            html += `
                <li>
                    <a class="dropdown-item d-flex gap-2 align-items-start py-2" href="#">
                        <div class="mt-1">${iconMap[n.type] || iconMap['info']}</div>
                        <div>
                            <div class="small fw-normal text-wrap">${n.message}</div>
                            <div class="text-muted" style="font-size: 0.7rem">${n.time}</div>
                        </div>
                    </a>
                </li>
            `;
        });

        // Add clear button
        html += `<li><hr class="dropdown-divider"></li>
                 <li><a class="dropdown-item text-center small text-muted" href="#" onclick="app.clearNotifications(event)">Clear all</a></li>`;

        list.innerHTML = html;
    },

    clearNotifications: (e) => {
        if (e) { e.preventDefault(); e.stopPropagation(); }
        app.state.notifications = [];
        app.renderNotifications();
        document.getElementById('notification-badge').classList.add('d-none');
    },

    init: () => {
        // Theme Setup
        document.documentElement.setAttribute('data-theme', app.state.theme);
        app.updateThemeIcon();

        document.getElementById('themeToggle').addEventListener('click', app.toggleTheme);

        document.getElementById('explainModeToggle').addEventListener('change', (e) => {
            app.state.explainMode = e.target.checked;
            const logEl = document.getElementById('live-trace-log');
            logEl.innerHTML = app.state.explainMode
                ? '<div class="text-success">Explain Mode Enabled. Logs will appear here.</div>'
                : '<div class="text-muted">Enable Explain Mode to see live execution...</div>';
        });

        // Load initial data
        app.loadCandidates();
        app.loadJobs();

        // Init Graph Visualizer
        app.graphViz = new GraphVisualizer('skillGraph');
    },

    toggleTheme: () => {
        app.state.theme = app.state.theme === 'light' ? 'dark' : 'light';
        document.documentElement.setAttribute('data-theme', app.state.theme);
        localStorage.setItem('theme', app.state.theme);
        app.updateThemeIcon();
        // Redraw graph if visible because colors change
        if (!document.getElementById('skills-page').classList.contains('d-none')) {
            app.graphViz.draw();
        }
    },

    updateThemeIcon: () => {
        const btn = document.getElementById('themeToggle');
        btn.innerHTML = app.state.theme === 'light'
            ? '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path></svg>'
            : '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"></circle><line x1="12" y1="1" x2="12" y2="3"></line><line x1="12" y1="21" x2="12" y2="23"></line><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line><line x1="1" y1="12" x2="3" y2="12"></line><line x1="21" y1="12" x2="23" y2="12"></line><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line></svg>';
    },

    showPage: (pageId) => {
        document.querySelectorAll('.page-section').forEach(el => el.classList.add('d-none'));
        document.getElementById(`${pageId}-page`).classList.remove('d-none');
        document.querySelectorAll('.nav-link').forEach(el => el.classList.remove('active'));

        // Highlight active sidebar link
        // We find the link that calls this pageId
        const activeLink = document.querySelector(`.nav-link[onclick*="'${pageId}'"]`);
        if (activeLink) activeLink.classList.add('active');

        // Update top text if needed (Optional)
        const titleMap = {
            'dashboard': 'Dashboard',
            'candidates': 'Candidates',
            'jobs': 'Job Matching Engine',
            'skills': 'Skill Network Analysis'
        };
        const titleEl = document.getElementById('page-title');
        if (titleEl && titleMap[pageId]) titleEl.innerText = titleMap[pageId];

        if (pageId === 'skills') {
            app.loadGraph();
            setTimeout(() => app.graphViz.resize(), 100); // Ensure size correct after display
        }
    },

    loadCandidates: async () => {
        try {
            const res = await fetch(`${API_URL}/Candidates`);
            if (!res.ok) throw new Error('Failed to fetch');
            app.state.candidates = await res.json();
            app.renderCandidates(app.state.candidates);
        } catch (e) {
            console.error(e);
        }
    },

    loadJobs: async () => {
        const res = await fetch(`${API_URL}/Jobs`);
        app.state.jobs = await res.json();
        const select = document.getElementById('job-select');
        select.innerHTML = '<option value="">Select a Job to Match...</option>';
        app.state.jobs.forEach(j => {
            select.innerHTML += `<option value="${j.id}">${j.jobTitle}</option>`;
        });
    },

    renderCandidates: (list) => {
        const container = document.getElementById('candidate-list');
        if (!list || list.length === 0) {
            container.innerHTML = '<div class="col-12 text-center text-muted">No candidates found.</div>';
            return;
        }

        container.innerHTML = list.map(c => `
            <div class="col-md-6 col-lg-4">
                <div class="card h-100 border-0 shadow-sm">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                             <h5 class="card-title text-primary fw-bold mb-0">${c.fullName}</h5>
                             <span class="badge bg-secondary bg-opacity-10 text-secondary">${c.experienceYears} Years</span>
                        </div>
                        <p class="card-text small text-muted mb-2">${c.education}</p>
                        <p class="card-text fw-semibold">$${c.expectedSalary.toLocaleString()}</p>
                        <div class="d-flex gap-1 flex-wrap mt-3">
                            ${c.candidateSkills ? c.candidateSkills.map(s => `<span class="badge bg-light text-dark border">${s.skill?.skillName || 'Skill'}</span>`).join('') : ''}
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    },

    addCandidate: async () => {
        const candidate = {
            fullName: document.getElementById('newCandName').value,
            experienceYears: parseInt(document.getElementById('newCandExp').value),
            expectedSalary: parseFloat(document.getElementById('newCandSalary').value),
            education: document.getElementById('newCandEducation').value,
            resumeText: document.getElementById('newCandResume').value,
            candidateSkills: []
        };

        try {
            const res = await fetch(`${API_URL}/Candidates`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(candidate)
            });

            if (res.ok) {
                // Close modal
                const modalEl = document.getElementById('addCandidateModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();

                // Clear form
                document.getElementById('addCandidateForm').reset();

                // Refresh list
                app.loadCandidates();
                app.addNotification('New candidate added successfully', 'success');
                alert('Candidate added successfully!');
            } else {
                alert('Failed to add candidate. Please check inputs.');
            }
        } catch (e) {
            console.error(e);
            alert('Error adding candidate.');
        }
    },

    rankCandidates: async () => {
        const res = await fetch(`${API_URL}/Ranking/rank`);
        const data = await res.json();

        const list = document.getElementById('top-candidates-list');
        list.innerHTML = data.candidates.map((c, i) => `
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <span class="fw-bold me-2">#${i + 1}</span>
                    ${c.fullName}
                </div>
                <span class="badge bg-primary rounded-pill">${c.experienceYears} Yrs</span>
            </li>
        `).join('');

        if (app.state.explainMode && data.trace) {
            visualizer.logTrace(data.trace, 'live-trace-log');
        }
    },

    searchCandidates: async () => {
        const keyword = document.getElementById('candidateSearch').value;
        if (!keyword) {
            app.loadCandidates();
            return;
        }

        try {
            const res = await fetch(`${API_URL}/Candidates/search?keyword=${encodeURIComponent(keyword)}`);
            const data = await res.json();

            // Handle lowercase or PascalCase
            const list = data.candidates || data.Candidates || [];
            const traces = data.traces || data.Traces || [];

            app.renderCandidates(list);

            if (app.state.explainMode && traces.length > 0) {
                // Find the first trace that has a 'found' step for better UX, otherwise first one
                const foundTrace = traces.find(t => t.steps.some(s => s.description.includes('Pattern found'))) || traces[0];
                visualizer.logTrace(foundTrace, 'live-trace-log');
            }
        } catch (e) {
            console.error("Search failed", e);
            document.getElementById('candidate-list').innerHTML = '<div class="text-danger text-center">Search failed. See console.</div>';
        }
    },

    sortCandidates: async (algo) => {
        const res = await fetch(`${API_URL}/Ranking/sort?algorithm=${algo}`);
        const data = await res.json();
        app.renderCandidates(data.candidates);

        if (app.state.explainMode && data.trace) {
            visualizer.logTrace(data.trace, 'live-trace-log');
        }
    },

    greedyShortlist: async () => {
        const budget = document.getElementById('budgetInput').value;
        const res = await fetch(`${API_URL}/Ranking/shortlist?budget=${budget}`);
        const data = await res.json();

        const container = document.getElementById('greedy-results');
        if (data.candidates.length === 0) {
            container.innerHTML = '<div class="col-12 text-muted small">No candidates selected.</div>';
        } else {
            container.innerHTML = data.candidates.map(c => `
                <div class="col-6 col-md-4">
                    <div class="p-2 bg-success bg-opacity-10 border border-success rounded small">
                        <strong>${c.fullName}</strong>
                        <div class="text-muted">$${c.expectedSalary.toLocaleString()}</div>
                    </div>
                </div>
            `).join('');
        }

        if (app.state.explainMode && data.trace) {
            visualizer.logTrace(data.trace, 'live-trace-log');
        }
    },

    matchJob: async () => {
        const jobId = document.getElementById('job-select').value;
        if (!jobId) {
            document.getElementById('match-results').innerHTML = '';
            return;
        }

        const res = await fetch(`${API_URL}/Jobs/${jobId}/match`);
        const results = await res.json();

        const container = document.getElementById('match-results');
        container.innerHTML = results.map(r => `
            <div class="alert ${r.score >= 70 ? 'alert-success' : 'alert-light border'} d-flex justify-content-between align-items-center">
                 <span>
                    <strong>${r.candidate}</strong>
                 </span>
                 <span class="fw-bold">${r.score.toFixed(1)}% Match</span>
            </div>
        `).join('');

        if (app.state.explainMode && results.length > 0 && results[0].trace) {
            // Visualize the first match calculation
            visualizer.logTrace(results[0].trace, 'live-trace-log');
        }
    },

    autocompleteSkill: async () => {
        const prefix = document.getElementById('skillInput').value;

        // Filter graph immediately on input
        app.filterGraph(prefix);

        const list = document.getElementById('autocomplete-list');

        if (prefix.length < 1) {
            list.innerHTML = '';
            return;
        }

        const res = await fetch(`${API_URL}/Skills/autocomplete?prefix=${prefix}`);
        const data = await res.json();

        if (data.results.length === 0) {
            list.innerHTML = '<li class="list-group-item text-muted small">No matches found in Trie.</li>';
            return;
        }

        list.innerHTML = data.results.map(s => `
            <li class="list-group-item list-group-item-action cursor-pointer" onclick="document.getElementById('skillInput').value='${s}'; document.getElementById('autocomplete-list').innerHTML=''; app.filterGraph('${s}');">${s}</li>
        `).join('');

        if (app.state.explainMode && data.trace) {
            visualizer.logTrace(data.trace, 'live-trace-log');
        }
    },

    loadGraph: async () => {
        try {
            const res = await fetch(`${API_URL}/Skills/network`);
            const data = await res.json();
            console.log("Graph API Response:", data);
            app.graphViz.setData(data.graph);
        } catch (e) {
            console.error("Error loading graph:", e);
        }
    },

    filterGraph: (query) => {
        // query is passed directly
        app.graphViz.filterGraph(query);
    }
};

document.addEventListener('DOMContentLoaded', app.init);
