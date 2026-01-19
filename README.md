# Portfolio Calculator

Portfolio Calculator is project for calculating the value of an investor’s portfolio at a given reference date.

The project is built using **Clean Architecture**, **CQRS**, **ASP.NET Core**, **MongoDB**, and **Angular**, and supports multiple user interfaces:
- a console application (CLI)
- a Web API with an Angular-based UI for visualization

---

## Features

- Import investment data from CSV files into MongoDB
- Calculate total portfolio value for an investor at a specific date
- Support for multiple investment types:
  - **Stocks** (priced via market quotes)
  - **Real Estate** (based on transactions)
  - **Funds** (recursive composition of underlying investments)
- Transactions and quotes are evaluated **only up to the reference date**
- Protection against cyclic fund dependencies
- Portfolio composition visualization (Bar / Pie charts)

---

## Architecture

The project follows **Clean Architecture** and **SOLID** principles.

### Layers

- **Domain**
  - Pure domain types and enums
- **Application**
  - CQRS with MediatR
  - Portfolio valuation business logic
  - Repository abstractions
- **Infrastructure**
  - MongoDB integration
  - CSV import
  - Repository implementations
- **Presentation**
  - Console Application (CLI)
  - ASP.NET Core Web API
- **UI**
  - Angular + Angular Material

All UI layers reuse the **same Application business logic**.

---

## Requirements

- .NET 10 SDK
- Docker & Docker Compose
- Node.js (optional, only for UI)
- Angular CLI (optional, only for UI)

---

## PowerShell Helper Scripts

The project includes several PowerShell (`.ps1`) helper scripts located in the `docker/PSScripts` folder.  
They are intended to simplify local development, builds, and common workflows.

> All scripts are designed to be run in the `docker/PSScripts` folder.

---

## Running the Console Application

The Console Application (CLI) is used for importing data and calculating portfolio values.  
To run it correctly, MongoDB must be started first and the CLI must be launched in **interactive mode**.

---

### Step 1: Start MongoDB

MongoDB is required for storing imported data and for all portfolio calculations.

Start MongoDB using the provided PowerShell script:

```powershell
.\mongo-up.ps1
```

### Step 2: Run the Console Application in Interactive Mode

Start the CLI in interactive mode using the helper script:

```powershell
.\cli-run-regular-interactive.ps1
```

After startup, the CLI will wait for user commands.

### Available CLI Commands

The console application supports two main commands:

### import

Imports CSV data into MongoDB.

```text
import
```

- Reads CSV files from the specified folder
- Stores investments, transactions, quotes, and ownership data in MongoDB
- This command should be executed before running any portfolio calculations

### value <InvestorId> <yyyy-MM-dd>

Calculates the total portfolio value for a given investor at a specific reference date.

```text
value Investor0 2016-12-31
```

- Considers only transactions and quotes up to the given date

Returns:

- total portfolio value
- breakdown by investment type

### Example CLI Session

```text
> import ./data
Import completed:
  Investments rows:  10
  Transactions rows: 42
  Quotes rows:       15

> value Investor0 2019-12-31
Total portfolio value: 12345.67
Breakdown:
  RealEstate: 10000.00
  Stock:      2345.67
```

## BONUS:

## Web API

In addition to the console application, the project also provides an **ASP.NET Core Web API** that exposes the same portfolio calculation functionality.

The Web API uses the **same Application layer and CQRS queries** as the CLI, ensuring consistent business logic across all entry points.

### API Functionality

The Web API exposes an endpoint for calculating the portfolio value with **two parameters**:

- `investorId`
- `date` (reference date)

### Running the Web API

Start the Web API using the provided PowerShell script:

```powershell
.\docker\PSScripts\webapi-up.ps1
```

The API is available locally at:

```text
https://localhost:44340/
```

Portfolio Valuation Endpoint:

```text
GET https://localhost:44340/api/portfolio/value?investorId=Investor0&date=2019-12-31
```

## Angular UI

In addition to the CLI and Web API, the project also provides a **browser-based Angular UI** for interactive portfolio analysis and visualization.

The Angular UI communicates exclusively with the **ASP.NET Core Web API** and does not contain any business logic.  
All calculations are performed on the backend using the shared Application layer.

---

### Functionality

The Angular UI allows you to:

- Enter an **Investor ID** and a **reference date**
- Calculate the portfolio value using the Web API
- View:
  - total portfolio value
  - breakdown by investment type
- Switch between **Bar Chart** and **Pie Chart** visualizations
- Handle loading and error states gracefully

---

### Running the Angular UI

Start the Angular UI using the provided PowerShell script:

```powershell
.\docker\PSScripts\angularui-up.ps1
```

After startup, the UI is available at:

```powershell
http://localhost:4200
```

#### Prerequisites

- The Web API must be running and reachable (default: https://localhost:53820)
- MongoDB must be running
- Data should be imported at least once using the CLI

### Notes

- MongoDB must be running before starting the CLI
- The import command needs to be executed at least once

The same business logic is shared between:

- CLI
- Web API
- Angular UI

---

### Scripts Overview

#### `mongo-up.ps1`
Starts MongoDB using Docker Compose.

- Brings up the MongoDB container in detached mode
- Should be executed before running CLI, Web API, or Angular UI

```powershell
.\mongo-up.ps1
```

#### `compose-down.ps1`
Stops and removes all Docker containers defined in docker-compose.yml.

- Useful for cleanup or resetting the local environment

```powershell
.\compose-down.ps1
```

#### `cli-build.ps1`
Builds the Console Application (CLI).

- Compiles the CLI project

```powershell
.\cli-build.ps1
```

#### `cli-run-direct-import.ps1`
Runs the Console Application and directly executes a CSV import.

- Intended for quick data initialization
- Skips interactive mode

```powershell
.\cli-run-direct-import.ps1
```

#### `cli-run-regular-interactive.ps1`
Runs the Console Application in interactive mode.

- Allows executing commands such as `import` and `value` manually

```powershell
.\cli-run-regular-interactive.ps1
```

#### `cli-stop-and-rm.ps1`
Stops and removes any running CLI-related containers or processes.

- Useful when restarting the CLI workflow

```powershell
.\cli-stop-and-rm.ps1
```

#### `webapi-build.ps1`
Builds the ASP.NET Core Web API project.

- Ensures the Web API compiles successfully

```powershell
.\webapi-build.ps1
```

#### `webapi-up.ps1`
Runs the ASP.NET Core Web API.

- Starts the Web API locally

```powershell
.\webapi-up.ps1
```

#### `angularui-up.ps1`
Starts the Angular UI.

```powershell
.\angularui-up.ps1
```
