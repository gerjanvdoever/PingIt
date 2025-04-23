# PingIt – Incident Reporting Application

**PingIt** is a cross-platform application designed to facilitate the reporting and management of public space incidents. The system consists of a mobile application built with .NET MAUI and a web-based management interface built with Blazor, both connected through a shared ASP.NET Core Web API backend.

## Overview

The mobile application allows individuals to report incidents such as infrastructure issues or public disturbances. Reports can include location data and images. Depending on the user’s preferences, reports can be submitted anonymously or with account-based tracking for status updates.

A separate web interface enables municipal staff to review, categorize, and update reported incidents. This includes assigning priorities and managing statuses throughout the incident resolution process.

## Architecture

- **Frontend (Mobile)**: .NET MAUI application for Android and Windows platforms.
- **Frontend (Web)**: Blazor Server application for administrative use.
- **Backend**: ASP.NET Core Web API responsible for data storage, business logic, and communication between client apps.

## Key Capabilities

- Incident submission with support for media and location data
- Optional user authentication and personalized tracking
- Real-time updates and incident status changes
- Administrative dashboard with overview and detail editing functionality

## Deployment Platforms

- Android (mobile devices)
- Windows (desktop environments)

This system supports responsive communication between residents and municipal services, improving transparency and efficiency in public incident management.
