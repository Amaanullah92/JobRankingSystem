# Job Ranking System (DSA Project)

A high-performance **Job Ranking & Candidate Matching System** built with **.NET 10.0** and **PostgreSQL**.

🌐 **Live Demo:** [https://jobrankingsystem.onrender.com/](https://jobrankingsystem.onrender.com/)

## 🎯 Purpose & Real-World Impact
Manual resume screening is the biggest bottleneck in modern recruitment. This project demonstrates how **Data Structures and Algorithms (DSA)** can automate this process with mathematical precision. By moving beyond simple keyword filters to computational matching, this system helps **Recruiters** and **Hiring Managers** short-list the best talent objectively and efficiently.

## 🚀 Features & Algorithms

This application solves key hiring challenges using optimized algorithms:

| Feature | Algorithm / Data Structure | Time Complexity | Description |
| :--- | :--- | :--- | :--- |
| **Candidate Search** | **KMP Algorithm** | `O(N + M)` | Efficiently searches resume text for specific keywords without backtracking. |
| **Skill Autocomplete** | **Trie (Prefix Tree)** | `O(L)` | Provides instant skill suggestions as you type. |
| **Job Matching** | **Dynamic Programming (LCS)** | `O(M * N)` | Calculates a "Fit Score" by finding the Longest Common Subsequence between job requirements and candidate skills. |
| **Ranking** | **Max Heap** | `O(N log N)` | Efficiently retrieves the top K candidates sorted by experience or score. |
| **Sorting** | **Merge Sort / Quick Sort** | `O(N log N)` | Sorts large lists of candidates by salary or experience. |
| **Shortlisting** | **Greedy Algorithm** | `O(N log N)` | Selects the best set of candidates that fit within a hiring budget. |
| **Skill Network** | **Graph (Adjacency List)** | `O(V + E)` | Visualizes relationships between skills (co-occurrence in candidate profiles). |
| **Fast Lookup** | **Hash Table (Custom)** | `O(1) avg` | Indexes candidates by their primary skills for quick retrieval. |

## 🛠 Tech Stack

- **Backend**: .NET 10.0 (C#), ASP.NET Core Web API
- **Database**: PostgreSQL (Entity Framework Core)
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5
- **Visualization**: HTML5 Canvas (for Graph), Custom Trace Visualizer
- **Containerization**: Docker
- **Deployment**: Render (Cloud Hosting)

## 📦 Getting Started

### Prerequisites
- .NET 10.0 SDK
- Docker (Optional, for containerized run)
- PostgreSQL Database (Local or Neon Cloud)

### Local Setup
1.  **Clone the repository**
    ```bash
    git clone https://github.com/Amaanullah92/JobRankingSystem.git
    cd JobRankingSystem
    ```

2.  **Configure Database**
    - Update `appsettings.json` with your PostgreSQL connection string.
    - Or set the `ConnectionStrings__DefaultConnection` environment variable.

3.  **Run the Application**
    ```bash
    dotnet run
    ```
    The app will proceed to `http://localhost:5000` (or the port shown in terminal).

### 🐳 Run with Docker

```bash
docker build -t job-ranking-system .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="YOUR_DB_CONNECTION_STRING" job-ranking-system
```

## 🌐 Deployment

This project is configured for easy deployment on **Render** (Free Tier).

👉 **[Read the Deployment Guide](DEPLOYMENT.md)** for step-by-step instructions.

## 📂 Project Structure

```
JobRankingSystem/
├── Controllers/       # API Endpoints (Candidates, Jobs, Skills, Ranking)
├── Models/            # Data Models (Candidate, Job, Skill, AlgorithmTrace)
├── Services/          # DSA Implementations (AVL, Graph, Heap, KMP, etc.)
├── Data/              # EF Core DbContext
├── wwwroot/           # Frontend Assets (HTML, CSS, JS)
├── Dockerfile         # Container Configuration
└── Program.cs         # App Configuration & DI
```

## 🔍 Algorithm Visualization
The application includes a built-in **Visualizer** that prints step-by-step execution traces of the algorithms to the UI, helping users understand exactly how the ranking and matching logic works under the hood.

---
*Created by Muhammad Amaanullah for DSA Semester Project.*
