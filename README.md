# JournalMate

JournalMate is a premium, secure, and private journaling application built using **.NET MAUI Blazor Hybrid**. It focuses on local-first data storage and an elegant user experience.

---

## 📋 Table of Contents
1. [Core Features](#-core-features)
2. [Project Structure](#-project-structure)
3. [Technology Stack](#-technology-stack)
4. [Getting Started](#-getting-started)
5. [Development Guide](#-development-guide)
6. [Security Architecture](#-security-architecture)

---

## ✨ Core Features

### 🔒 Security & Privacy
- **PIN-Based Authentication**: Secure access with a custom 4-digit PIN.
- **Intelligent Lockout**: Automatically blocks access for 5 minutes after 5 failed attempts.
- **Local-First**: All data is stored locally on your device in an encrypted/private SQLite database.
- **Factory Reset**: A "Reset App" feature to wipe all entries, settings, and media for a fresh start.

### ✍️ Journaling Experience
- **Interactive Diary**: Write and manage your thoughts with a sleek, minimalist editor.
- **Mood Tracking**: Log your daily mood and visualize your internal state.
- **Calendar View**: Navigate through your memories using a unified calendar interface.
- **Media Support**: Set and persist your profile picture to personalize your space.

### 🎨 Premium UI/UX
- **Dynamic Backgrounds**: Interactive, mouse-reactive background shapes for a modern feel.
- **State-of-the-Art Design**: Glassmorphism, smooth gradients, and curated color palettes.
- **Responsive Layout**: Optimized for both desktop and mobile window sizes.
- **Theme Support**: Seamless switching between light and dark modes.

---

## 📁 Project Structure

```text
JournalMate/
├── Components/
│   ├── Layout/                # Shared layout components (NavMenu, MainLayout)
│   ├── Pages/                 # Main application screens (Razor components)
│   │   ├── Login.razor        # Dual-mode (Setup/Unlock) authentication page
│   │   ├── Diary.razor        # Main journaling view
│   │   ├── Calendar.razor     # Chronological memory navigator
│   │   ├── Settings.razor     # User preferences, Name/PIN change, Reset
│   │   └── Profile.razor      # User profile management
│   ├── Routes.razor           # Application routing configuration
│   └── _Imports.razor         # Global using directives for Razor components
├── Services/
│   ├── AppCurrentState.cs     # Global state management & persistence
│   ├── AuthService.cs        # PIN verification and security logic
│   ├── JournalDatabase.cs     # SQLite data access layer
│   ├── FileSaverService.cs    # Local file system interactions
│   └── ToggleTheme.cs         # Theme switching logic
├── Models/                    # C# data entities (User, Entry, etc.)
├── Resources/                 # Fonts, Images, and Styles
├── wwwroot/                   # Static web assets (CSS, JS, Icons)
├── MauiProgram.cs             # Dependency Injection & App Startup
└── JournalMate.csproj         # Project configuration & dependencies
```

---

## 🛠 Technology Stack
- **Framework**: .NET 10.0 (MAUI Blazor Hybrid)
- **UI Architecture**: HTML5, Vanilla CSS3 (Custom Design System)
- **Database**: SQLite (local storage)
- **Programming Language**: C# 13
- **Styling**: Modern CSS (Flexbox, Grid, Glassmorphism, Animations)

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0/10.0 SDK](https://dotnet.microsoft.com/download)
- MAUI Workload (`dotnet workload install maui`)
- IDE: Visual Studio 2022 or VS Code with .NET MAUI extension

### Installation & Run
1. Clone the repository.
2. Open terminal in the project root:
   ```powershell
   # Restore dependencies
   dotnet restore

   # Run for Windows
   dotnet run --project JournalMate.csproj -f net10.0-windows10.0.19041.0
   ```

---

## 🏗 How to Make This Project (Technical Flow)

### 1. Unified State Management
The project uses `AppCurrentState.cs` as a singleton to sync data across Razor components. It leverages `Microsoft.Maui.Storage.Preferences` for simple settings (Name, Theme) and `SQLite` for heavy data (Entries).

### 2. The Authentication Loop
The `Login.razor` acts as the gateway:
- **Mode Switching**: It checks `AuthService.IsPinSetupAsync()` on load. If false, it shows **Setup Mode**; otherwise, it shows **Unlock Mode**.
- **Security Check**: Each login attempt hits the `AuthService` which manages the `FailedAttempts` counter in the database.

### 3. Database Layer
`JournalDatabase.cs` initializes the SQLite connection at startup. It ensures tables are created if they don't exist and provides async methods for CRUD operations on journal entries.

---

## 🛡 Security Architecture
- **Salted Hashing**: PINs are never stored in plain text. They are salted and hashed using SHA256.
- **Session Isolation**: User authentication state is managed in-memory and reset upon application restart or manual logout.
- **Data Protection**: All database operations are performed locally, ensuring no cloud leakage.
