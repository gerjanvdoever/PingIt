# PingIt – Cross-Platform Public Incident Management

> This project was developed as final graduation project for my Computing Science Associate Degree.
> A video showcasing its features can be found [here](https://www.youtube.com/watch?v=gaFJUJ0kOhU).

**PingIt** is a modern, cross-platform application designed to streamline the reporting and handling of public incidents. The platform enables citizens to submit reports — optionally anonymously — while providing municipalities with a structured dashboard for follow-up and resolution.

PingIt consists of three core applications:

- A **mobile client** built with [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/), targeting Android and Windows.
- A **web-based dashboard** developed in [Blazor WebAssembly](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor).
- A shared **REST API backend** using [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) and [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) with PostgreSQL.

---

## Architecture Overview

PingIt is structured around a **clean, API-centric architecture**:

- **Backend (ASP.NET Core)**  
  - Exposes RESTful endpoints to all clients.  
  - Built around a layered MVC structure with controllers, services, and models.  
  - Uses Entity Framework Core for database access and schema evolution (migrations).  
  - Authenticates users using JWT tokens.
  - Email functionality for sending incident updates

- **Web Client (Blazor WebAssembly)**  
  - Acts as the administrative interface for handling incoming reports.  
  - Built using a feature-based structure with Razor pages and components.
  - Provides secure access for authenticated municipal staff only.

- **Mobile App (.NET MAUI)**  
  - Enables citizens to submit incident reports, view details, and track progress.
  - Enables field workers to see assigned incidents and manage them accordingly.  
  - Follows the MVVM pattern with strong separation of UI and business logic.  
  - Adapted for multiple platforms using platform-aware styling and navigation.

- **Shared Project**  
  - Contains all shared definitions (DTOs, enums), enabling strong typing across all layers.  
  - Promotes consistency and decoupling via the Dependency Inversion Principle.

---

## Key Features

- Report submission with support for media, location, and metadata.  
- User registration with secure authentication.
- Role-based access control for certain app functionalities and dashboard login.  
- Clean UI separation with platform-specific adaptations (e.g., mobile vs desktop layouts).  
- Integrated testing support for both unit and UI tests.

---

## Supported Platforms

- Android (via .NET MAUI)  
- Windows (via .NET MAUI)  
- Any modern browser (via Blazor WebAssembly)

---

## Project Goal

PingIt aims to improve communication between citizens and municipal services through a responsive, reliable, and secure digital reporting system — increasing transparency and efficiency in handling incidents in public space.
