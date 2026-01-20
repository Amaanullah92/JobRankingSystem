const API_URL = '/api';

const app = {
    state: {
        candidates: [],
        jobs: [],
        trace: null,
        explainMode: false,
        theme: localStorage.getItem('theme') || 'light',
        notifications: [],
        isSidebarOpen: false,
        user: JSON.parse(localStorage.getItem('user')) || null,
        token: localStorage.getItem('token') || null,
        currentPage: 1,
        pageSize: 5
    },

    // Helper Methods
    togglePassword: (id) => {
        const input = document.getElementById(id);
        if (input) {
            input.type = input.type === "password" ? "text" : "password";
        }
    },

    // Auth Methods
    login: async () => {
        const username = document.getElementById('loginUser').value;
        const password = document.getElementById('loginPass').value;

        try {
            const res = await fetch(`${API_URL}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            if (res.ok) {
                const data = await res.json();
                app.handleLoginSuccess(data);
            } else {
                try {
                    const err = await res.json();
                    alert('Login failed: ' + (err.message || 'Check credentials'));
                } catch {
                    alert('Login failed. Check credentials.');
                }
            }
        } catch (e) {
            console.error(e);
            alert('Login error.');
        }
    },

    guestLogin: async () => {
        const username = document.getElementById('guestUser').value;
        if (!username) {
            alert("Please enter a username for the session.");
            return;
        }

        try {
            const res = await fetch(`${API_URL}/auth/login/guest`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username })
            });

            if (res.ok) {
                const data = await res.json();
                app.handleLoginSuccess(data);
            } else {
                alert('Guest login failed.');
            }
        } catch (e) {
            console.error(e);
            alert('Login error.');
        }
    },

    handleLoginSuccess: (data) => {
        app.state.token = data.token;
        // Decode token or use response data. 
        // Backend now returns { token, expiration, username, roles }
        app.state.user = {
            username: data.username || "User",
            roles: data.roles || ["User"]
        };

        localStorage.setItem('token', app.state.token);
        localStorage.setItem('user', JSON.stringify(app.state.user));

        app.updateAuthUI();
        app.addNotification(`Welcome back, ${app.state.user.username}!`, 'success');

        const loginModal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
        if (loginModal) loginModal.hide();

        app.loadCandidates(); // Reload to refresh buttons if needed
    },




    logout: () => {
        app.state.token = null;
        app.state.user = null;
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        app.updateAuthUI();
        app.addNotification('Logged out', 'info');
        app.loadCandidates(); // Reload to refresh buttons
    },

    updateAuthUI: () => {
        const loginBtn = document.getElementById('loginBtn');
        const logoutBtn = document.getElementById('logoutBtn');
        const userDisplay = document.getElementById('user-display');
        const addCandBtn = document.querySelector('[data-bs-target="#addCandidateModal"]');

        if (app.state.token) {
            if (loginBtn) loginBtn.classList.add('d-none');
            if (logoutBtn) logoutBtn.classList.remove('d-none');
            if (userDisplay) {
                userDisplay.classList.remove('d-none');
                userDisplay.innerText = `Hi, ${app.state.user.username}`;
                if (app.state.user.roles.includes("Admin")) {
                    userDisplay.innerText += " (Admin)";
                }
            }

            // Only Admin can add candidates
            if (addCandBtn) {
                if (app.state.user.roles && app.state.user.roles.includes("Admin")) {
                    addCandBtn.classList.remove('d-none');
                } else {
                    addCandBtn.classList.add('d-none');
                }
                addCandBtn.disabled = false;
            }
        } else {
            if (loginBtn) loginBtn.classList.remove('d-none');
            if (logoutBtn) logoutBtn.classList.add('d-none');
            if (userDisplay) userDisplay.classList.add('d-none');
            // Hide Add Button if not logged in
            if (addCandBtn) addCandBtn.classList.add('d-none');
        }
    },

    toggleSidebar: () => {
        app.state.isSidebarOpen = !app.state.isSidebarOpen;
        const sidebar = document.querySelector('.sidebar');
        const overlay = document.getElementById('sidebar-overlay');

        if (app.state.isSidebarOpen) {
            sidebar.classList.add('active');
            overlay.classList.add('active');
        } else {
            sidebar.classList.remove('active');
            overlay.classList.remove('active');
        }
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
        console.log("App initializing...");
        try {
            // Theme Setup
            if (app.state.theme) {
                document.documentElement.setAttribute('data-theme', app.state.theme);
                app.updateThemeIcon();
            }

            const themeBtn = document.getElementById('themeToggle');
            if (themeBtn) themeBtn.addEventListener('click', app.toggleTheme);

            const explainToggle = document.getElementById('explainModeToggle');
            if (explainToggle) {
                explainToggle.addEventListener('change', (e) => {
                    app.state.explainMode = e.target.checked;
                    const logEl = document.getElementById('live-trace-log');
                    if (logEl) {
                        logEl.innerHTML = app.state.explainMode
                            ? '<div class="text-success">Explain Mode Enabled. Logs will appear here.</div>'
                            : '<div class="text-muted">Enable Explain Mode to see live execution...</div>';
                    }
                });
            }

            // Init Auth UI
            app.updateAuthUI();

            // Load initial data
            app.loadCandidates();
            app.loadJobs();

            // Init Graph Visualizer
            if (typeof GraphVisualizer !== 'undefined') {
                app.graphViz = new GraphVisualizer('skillGraph');
            } else {
                console.warn('GraphVisualizer class not found. Skill graph will not load.');
            }
        } catch (err) {
            console.error("Critical Error during App Init:", err);
        }
    },

    toggleTheme: () => {
        app.state.theme = app.state.theme === 'light' ? 'dark' : 'light';
        document.documentElement.setAttribute('data-theme', app.state.theme);
        localStorage.setItem('theme', app.state.theme);
        app.updateThemeIcon();
        // Redraw graph if visible because colors change
        if (typeof app.graphViz !== 'undefined' && !document.getElementById('skills-page').classList.contains('d-none')) {
            app.graphViz.draw();
        }
    },

    updateThemeIcon: () => {
        const btn = document.getElementById('themeToggle');
        if (!btn) return;
        btn.innerHTML = app.state.theme === 'light'
            ? '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path></svg>'
            : '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"></circle><line x1="12" y1="1" x2="12" y2="3"></line><line x1="12" y1="21" x2="12" y2="23"></line><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line><line x1="1" y1="12" x2="3" y2="12"></line><line x1="21" y1="12" x2="23" y2="12"></line><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line></svg>';
    },

    showPage: (pageId) => {
        // Close sidebar on mobile if open
        if (window.innerWidth <= 768 && app.state.isSidebarOpen) {
            app.toggleSidebar();
        }

        document.querySelectorAll('.page-section').forEach(el => el.classList.add('d-none'));
        const page = document.getElementById(`${pageId}-page`);
        if (page) page.classList.remove('d-none');
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

        if (pageId === 'skills' && typeof app.graphViz !== 'undefined') {
            app.loadGraph();
            setTimeout(() => app.graphViz.resize(), 100); // Ensure size correct after display
        }
    },

    loadCandidates: async () => {
        try {
            console.log("Loading candidates...");
            const res = await fetch(`${API_URL}/Candidates?page=${app.state.currentPage}&pageSize=${app.state.pageSize}`);
            if (!res.ok) throw new Error('Failed to fetch');
            app.state.candidates = await res.json();
            app.renderCandidates(app.state.candidates);
            app.renderPagination();
        } catch (e) {
            console.error("Error loading candidates:", e);
            const container = document.getElementById('candidate-list');
            if (container) container.innerHTML = '<div class="col-12 text-center text-danger">Failed to load candidates. API might be down.</div>';
            app.addNotification('Failed to load candidates', 'danger');
        }
    },

    changePage: (delta) => {
        const newPage = app.state.currentPage + delta;
        if (newPage > 0) {
            app.state.currentPage = newPage;
            app.loadCandidates();
        }
    },

    renderPagination: () => {
        const prevBtn = document.getElementById('prevPageBtn');
        const nextBtn = document.getElementById('nextPageBtn');
        const indicator = document.getElementById('pageIndicator');

        if (indicator) indicator.innerText = `Page ${app.state.currentPage}`;
        if (prevBtn) prevBtn.disabled = app.state.currentPage <= 1;
        // Next button logic: we assume if we get fewer items than pageSize, it's the last page.
        if (nextBtn) {
            nextBtn.disabled = app.state.candidates.length < app.state.pageSize;
        }
    },

    loadJobs: async () => {
        try {
            const res = await fetch(`${API_URL}/Jobs`);
            if (!res.ok) throw new Error('Failed to fetch Jobs');
            app.state.jobs = await res.json();
            const select = document.getElementById('job-select');
            if (select) {
                select.innerHTML = '<option value="">Select a Job to Match...</option>';
                app.state.jobs.forEach(j => {
                    select.innerHTML += `<option value="${j.id}">${j.jobTitle}</option>`;
                });
            }
        } catch (e) {
            console.warn("Error loading jobs:", e);
        }
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
                             ${(app.state.user && app.state.user.roles && app.state.user.roles.includes("Admin")) ? `
                             <div class="d-flex gap-2">
                                <button class="btn btn-sm btn-outline-secondary" onclick="app.editCandidate(${c.id})">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                                </button>
                                <button class="btn btn-sm btn-outline-danger" onclick="app.deleteCandidate(${c.id})">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                                </button>
                             </div>` : ''}
                        </div>
                        <span class="badge bg-secondary bg-opacity-10 text-secondary mb-2">${c.experienceYears} Years Experience</span>
                        <p class="card-text small text-muted mb-2">${c.education}</p>
                        <p class="card-text fw-semibold">$${c.expectedSalary.toLocaleString()}</p>
                        <div class="d-flex gap-1 flex-wrap mt-3">
                            ${(c.skills && c.skills.length > 0) ? c.skills.map(s => `<span class="badge bg-light text-dark border">${s}</span>`).join('') : ''}
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    },

    editCandidate: (id) => {
        const candidate = app.state.candidates.find(c => c.id === id);
        if (!candidate) return;

        document.getElementById('editCandId').value = candidate.id;
        document.getElementById('editCandName').value = candidate.fullName;
        document.getElementById('editCandExp').value = candidate.experienceYears;
        document.getElementById('editCandSalary').value = candidate.expectedSalary;
        document.getElementById('editCandEducation').value = candidate.education;
        document.getElementById('editCandResume').value = candidate.resumeText || '';

        new bootstrap.Modal(document.getElementById('editCandidateModal')).show();
    },

    updateCandidate: async () => {
        const id = document.getElementById('editCandId').value;
        const candidate = {
            id: parseInt(id),
            fullName: document.getElementById('editCandName').value,
            experienceYears: parseInt(document.getElementById('editCandExp').value),
            expectedSalary: parseFloat(document.getElementById('editCandSalary').value),
            education: document.getElementById('editCandEducation').value,
            resumeText: document.getElementById('editCandResume').value
            // Backend will handle skills update if resume text changes
        };

        try {
            const res = await fetch(`${API_URL}/Candidates/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${app.state.token}`
                },
                body: JSON.stringify(candidate)
            });

            if (res.ok) {
                app.addNotification('Candidate updated!', 'success');
                const modal = bootstrap.Modal.getInstance(document.getElementById('editCandidateModal'));
                if (modal) modal.hide();
                app.loadCandidates();
            } else {
                throw new Error('Failed to update');
            }
        } catch (e) {
            console.error(e);
            app.addNotification('Error updating candidate', 'danger');
        }
    },

    deleteCandidate: async (id) => {
        if (!confirm('Are you sure you want to delete this candidate?')) return;

        try {
            const res = await fetch(`${API_URL}/Candidates/${id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${app.state.token}`
                }
            });

            if (res.ok) {
                app.addNotification('Candidate deleted!', 'success');
                app.loadCandidates();
            } else {
                throw new Error('Failed to delete');
            }
        } catch (e) {
            console.error(e);
            app.addNotification('Error deleting candidate', 'danger');
        }
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
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${app.state.token}`
                },
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
        try {
            const res = await fetch(`${API_URL}/Ranking/rank`);

            if (!res.ok) {
                throw new Error(`HTTP ${res.status}`);
            }

            const data = await res.json();

            // Handle PascalCase vs camelCase
            const candidates = data.candidates || data.Candidates || [];
            const trace = data.trace || data.Trace;

            const list = document.getElementById('top-candidates-list');

            if (!candidates || candidates.length === 0) {
                list.innerHTML = `<li class="list-group-item text-muted text-center py-5">No candidates found to rank.</li>`;
                return;
            }

            list.innerHTML = candidates.map((c, i) => `
                <li class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <span class="fw-bold me-2">#${i + 1}</span>
                        ${c.fullName}
                    </div>
                    <span class="badge bg-primary rounded-pill">${c.experienceYears} Yrs</span>
                </li>
            `).join('');

            if (app.state.explainMode && trace) {
                visualizer.logTrace(trace, 'live-trace-log');
            }
        } catch (e) {
            console.error(e);
            alert("Error running ranking: " + e.message);
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
