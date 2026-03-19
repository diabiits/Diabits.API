# Diabits API

Backend for collecting, processing, and serving health data in the Diabits system.

## Purpose

Centralizes health data (glucose, heart rate, sleep, etc.) and prepares it for analysis and visualization.

- Combines physiological + contextual data
- Enables correlation insights
- Returns chart-ready data to clients

## Tech Stack
- ASP.NET Core Web API
- Entity Framework Core
- JWT Authentication

## Responsibilities
- Validate and store incoming data
- Prevent duplicates
- Aggregate and transform data
- Provide visualization-ready endpoints

## Key Concepts

### Health Data Model
All data is:
- Timestamped
- User-scoped
- Typed (e.g. Glucose, Sleep, Workout)

### Bucketed Time Series
- Fixed time intervals (e.g. 10 min)
- Aligned datasets for consistent charting

### Read Models
- API returns pre-shaped DTOs
- Minimal frontend processing required

## Testing
- Unit tests for logic
- Integration tests for:
  - Auth
  - Data persistence
  - Aggregation

## Notes
- Backend handles most data processing
- Designed for single-user (current scope) via invite-only access
- Focus on reducing manual input
