// app.js - Main Application Logic

const API_URL = "/api";

document.addEventListener("DOMContentLoaded", () => {
    loadDashboard();
});

function showSection(id) {
    document.querySelectorAll(".section").forEach(s => s.classList.add("d-none"));
    document.getElementById(id).classList.remove("d-none");
    
    if (id === 'candidates') loadCandidates();
    if (id === 'jobs') loadJobs();
    if (id === 'skills') loadSkillGraph();
}

async function loadDashboard() {
    // Simple mock stats or fetch from API
    let cRes = await fetch(`${API_URL}/candidates`);
    let cData = await cRes.json();
    document.getElementById("total-candidates").innerText = cData.length;

    let jRes = await fetch(`${API_URL}/jobs`);
    let jData = await jRes.json();
    document.getElementById("total-jobs").innerText = jData.length;
}

// --- Candidates ---

async function loadCandidates(data = null) {
    if (!data) {
        let res = await fetch(`${API_URL}/candidates`);
        data = await res.json();
    }
    
    const list = document.getElementById("candidates-list");
    list.innerHTML = "";

    data.forEach(c => {
        let skills = c.candidateSkills ? c.candidateSkills.map(s => `<span class="badge bg-secondary me-1">${s.skill.skillName}</span>`).join("") : "";
        list.innerHTML += `
            <div class="col-md-4 mb-3">
                <div class="card p-3">
                    <h5>${c.fullName}</h5>
                    <p class="text-muted">${c.education} | ${c.experienceYears} Years Exp</p>
                    <p>Salary: $${c.expectedSalary}</p>
                    <div class="mb-2">${skills}</div>
                    <small><i>Resume: ${c.resumeText.substring(0, 50)}...</i></small>
                </div>
            </div>
        `;
    });
}

async function addCandidate() {
    const candidate = {
        fullName: document.getElementById("c-name").value,
        experienceYears: parseInt(document.getElementById("c-exp").value),
        education: document.getElementById("c-edu").value,
        expectedSalary: parseFloat(document.getElementById("c-salary").value),
        resumeText: document.getElementById("c-resume").value
    };

    await fetch(`${API_URL}/candidates`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(candidate)
    });

    const modal = bootstrap.Modal.getInstance(document.getElementById('addCandidateModal'));
    modal.hide();
    loadCandidates();
    loadDashboard(); 
}

async function searchCandidates() {
    const keyword = document.getElementById("search-keyword").value;
    const res = await fetch(`${API_URL}/candidates/search?keyword=${keyword}`);
    const result = await res.json();
    
    // Result contains { candidates: [], traces: [] }
    loadCandidates(result.candidates);
    
    if (result.traces && result.traces.length > 0) {
         renderTrace(result.traces[0], "candidate-trace");
    }
}

async function sortCandidates(type) {
    const res = await fetch(`${API_URL}/ranking/sort?algorithm=${type}`);
    const result = await res.json();
    loadCandidates(result.candidates);
    renderTrace(result.trace, "candidate-trace");
}

async function rankCandidates() {
    const res = await fetch(`${API_URL}/ranking/rank`); // MaxHeap
    const result = await res.json();
    loadCandidates(result.candidates);
    renderTrace(result.trace, "candidate-trace");
}

async function shortlistCandidates() {
    const budget = prompt("Enter Hiring Budget:", "200000");
    if (!budget) return;
    
    const res = await fetch(`${API_URL}/ranking/shortlist?budget=${budget}`); // Greedy
    const result = await res.json();
    loadCandidates(result.candidates);
    renderTrace(result.trace, "candidate-trace");
}

// --- Jobs ---

async function loadJobs() {
    let res = await fetch(`${API_URL}/jobs`);
    let data = await res.json();
    
    const list = document.getElementById("jobs-list");
    list.innerHTML = "";

    data.forEach(j => {
        list.innerHTML += `
            <div class="card p-3 mb-2">
                <div class="d-flex justify-content-between">
                    <h4>${j.jobTitle}</h4>
                    <button class="btn btn-primary btn-sm" onclick="matchCandidates(${j.id})">Run Match (DP)</button>
                </div>
                <p>Req: ${j.requiredSkills}</p>
                <div id="match-area-${j.id}" class="mt-2 text-muted"></div>
            </div>
        `;
    });
}

async function addJob() {
    const job = {
        jobTitle: document.getElementById("j-title").value,
        requiredSkills: document.getElementById("j-skills").value,
        minExperience: parseInt(document.getElementById("j-exp").value),
        maxSalary: parseFloat(document.getElementById("j-salary").value)
    };

    await fetch(`${API_URL}/jobs`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(job)
    });

    const modal = bootstrap.Modal.getInstance(document.getElementById('addJobModal'));
    modal.hide();
    loadJobs();
}

async function matchCandidates(jobId) {
    const res = await fetch(`${API_URL}/jobs/${jobId}/match`);
    const results = await res.json();
    
    const area = document.getElementById(`match-area-${jobId}`);
    area.innerHTML = "<h6>Top Matches:</h6>";
    
    // Only show top 3
    results.slice(0, 3).forEach(r => {
        area.innerHTML += `
            <div class="alert alert-info py-1">
                <strong>${r.candidate}</strong> - Fit Score: ${r.score.toFixed(1)}%
            </div>
        `;
    });

    // Visualize the first trace in the main result area
    if (results.length > 0) {
        renderTrace(results[0].trace, "job-match-results");
    }
}

// --- Skills ---

async function loadSkillGraph() {
    const res = await fetch(`${API_URL}/skills/network`);
    const data = await res.json();
    
    // data.graph is Dictionary<string, Dictionary<string, int>>
    drawGraph(data.graph, "graphCanvas");
}

async function autoCompleteSkill() {
    const prefix = document.getElementById("skill-search").value;
    if (prefix.length < 1) {
        document.getElementById("autocomplete-list").innerHTML = "";
        return;
    }

    const res = await fetch(`${API_URL}/skills/autocomplete?prefix=${prefix}`);
    const data = await res.json();
    
    const list = document.getElementById("autocomplete-list");
    list.innerHTML = "";
    
    data.results.forEach(s => {
        list.innerHTML += `<li class="list-group-item">${s}</li>`;
    });
    
    renderTrace(data.trace, "skill-trace");
}
