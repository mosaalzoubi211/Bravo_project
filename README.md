# 🛠️ Bravo - Craftsmen & Freelance Services Marketplace

**Bravo** is a comprehensive, multi-role marketplace platform built with **ASP.NET Core MVC**. It is designed to bridge the gap between clients seeking reliable maintenance/craftsmanship services and skilled local artisans looking for a professional environment to showcase their expertise, receive requests, and grow their income.

## 🌟 Overview

The platform digitizes the manual labor and maintenance sector by providing a transparent, secure, and highly interactive ecosystem. It features a dual-request system, allowing clients to either post a general task for competitive bidding or send a direct request to a specific professional based on their profile and ratings. 

## ✨ Core Features

*   **Dual-Request Workflow:** 
    *   **Open Bidding:** Clients post tasks to the "Job Market" where craftsmen can submit price offers.
    *   **Direct Requests:** Clients can search for specific artisans by name or category and send them private task requests.
*   **Role-Based Dashboards:** Isolated, highly customized workspaces (Areas) for Clients, Workers, and Admins, featuring real-time statistics (Completed, Active, and Cancelled tasks).
*   **Integrated Chat System:** A secure, task-specific messaging system allowing clients and workers to discuss details and negotiate before finalizing the agreement.
*   **Dynamic Notifications:** Real-time unread message alerts built using ASP.NET Core `ViewComponents` for a seamless user experience.
*   **Rating & Review Engine:** A post-task evaluation system that builds a trustworthy public profile for craftsmen based on actual client feedback.
*   **Modern UI/UX:** A sleek, fully responsive **Dark & Lime** theme built with Bootstrap 5, featuring native RTL (Right-to-Left) support for Arabic users.

## 🏗️ Architecture & Tech Stack

This project follows a clean, scalable architecture utilizing modern Microsoft web technologies:

*   **Framework:** ASP.NET Core MVC (.NET)
*   **Architecture Pattern:** MVC with **Areas** (Admin, Auth, Client, Worker) for modular separation of concerns.
*   **Database & ORM:** Entity Framework Core (Code-First Approach) with SQL Server.
*   **Authentication & Security:** ASP.NET Core Identity for robust authentication, password hashing, and Role-Based Access Control (RBAC).
*   **Frontend:** Bootstrap 5 (RTL), HTML5, CSS3 (Custom `bravo-dark.css`), and JavaScript.
*   **Dynamic UI:** Heavy utilization of `ViewComponents` and `TagHelpers` for reusable UI widgets (like dropdown notifications and dynamic navigation bars).

## 🚀 Getting Started

### Prerequisites
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (or your specific version)
*   SQL Server / LocalDB
*   Visual Studio 2022 or Visual Studio Code

### Installation

1. Clone the repository:
   ```bash
   git clone [https://github.com/YourUsername/Bravo.git](https://github.com/YourUsername/Bravo.git)
