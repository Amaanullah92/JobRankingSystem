# Free Deployment Guide (Render)

This guide shows you how to deploy your Job Ranking System for free using **Render**.

## Prerequisites
1.  **GitHub Account**: You need to push your code to a GitHub repository.
2.  **Render Account**: Sign up at [render.com](https://render.com).

## Step 1: Push Code to GitHub
(Already done! You are looking at it.)

## Step 2: Deploy on Render
1.  **Log in to Render** and click **New +**.
2.  Select **Web Service**.
3.  **Connect your GitHub account** and select your repository.
4.  Configure the service:
    *   **Name**: `job-ranking-system` (or anything you like)
    *   **Region**: Closest to you (e.g., Frankfurt, Oregon)
    *   **Branch**: `main`
    *   **Runtime**: **Docker** (This is important! We added a Dockerfile for you).
5.  **Free Tier**: Scroll down and select **Free**.
6.  Click **Create Web Service**.

## Step 3: Connect Neon DB (PostgreSQL)
Since you are using **Neon DB**, you need to tell Render how to connect to it.

1.  **Get your Connection String** from the Neon Dashboard. It looks like:
    `postgres://user:password@ep-cool-cloud.us-east-2.aws.neon.tech/neondb?sslmode=require`

2.  **Add it to Render**:
    *   Go to your Render Web Service dashboard.
    *   Click **Environment**.
    *   Click **Add Environment Variable**.
    *   **Key**: `ConnectionStrings__DefaultConnection` (Note the double underscore!)
    *   **Value**: Paste your Neon connection string here.
    *   Click **Save Changes**.

3.  **Why the weird Key?**: In .NET, nested JSON keys like `ConnectionStrings: { DefaultConnection: ... }` are overridden by environment variables using double underscores (`__`). So `ConnectionStrings__DefaultConnection` overrides the local `appsettings.json` value.

## Scaling & Limitations
*   **Spin Down**: Free web services verify spin down after 15 minutes of inactivity. The first request might take 30-50s to load.
