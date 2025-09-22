# 🎲 Board Game Buddy Finder (PlayPao) – Development Instructions

## 1. Objective

Build a **web application** where users can:

* Find and create board game activities.
* Specify place, province, participants, and duration.
* Join or leave activities.
* Manage approvals and participants.
* Communicate through an integrated chat.
* Explore extra features (calendar view, social credit, game roles).

---

## 2. Core Requirements

1. **Post an activity** (title, description, place, province, participants, deadline, duration).
2. **Join/Unjoin activity** (with participation status).
3. **Limit participants** (accept/reject by owner).
4. **Close post manually** (e.g., if already found members offline).
5. **Set post expiration** (deadline).
6. **Notify selected participants** when finalized.
7. **Extra feature(s)** – at least one (chat, calendar, rating, etc.).
8. **Responsive design** (portrait + landscape).

---

## 3. Functional Modules

* **Authentication**: login, register, session (Custom Middleware).
* **Activity Management**: create, edit, delete, close, view activities.
* **Participation**: attend/unattend, owner approves participants.
* **Filtering**: by province/place, keyword search.
* **Chat**: messaging inside each activity post.
* **Calendar**: show activities by date.
* **Social Credit**: anonymous rating system for participants.

---

## 4. Database Entities

* **User**: `UserId, Username, Email, PasswordHash, Role, SocialCredit`.
* **ActivityPost**: `PostId, Title, Description, Place, Province, MaxParticipants, Deadline, Duration, Status, OwnerId`.
* **Participation**: `ParticipationId, PostId, UserId, Status (Pending/Accepted/Rejected)`.
* **ChatMessage**: `ChatId, PostId, UserId, Message, SentAt`.

---

## 5. UI Pages

1. **Home/Dashboard** – list all activities, filter by province/place.
2. **Activity Detail** – show description, participants, join button, chat section.
3. **Create Activity** – form to create new activity.
4. **My Activities** – list of activities created and joined.
5. **Login/Register** – authentication forms.
6. **Calendar (Optional)** – visual display of activities by date.

---

## 6. Technology Stack

* **Frontend**: HTML, CSS, JavaScript (no frameworks except icons allowed).
* **Backend**: ASP.NET Core MVC on .NET 9.0.
* **Database**: PostgreSQL (using Npgsql).
* **Authentication**: Custom middleware.
* **Configuration**: `DotNetEnv` for environment variables.
* **AJAX**: at least one asynchronous interaction (e.g., join/unjoin without reload).
* **Deployment**: IIS / Azure Web App.

---

## 7. Development Workflow

1. **Project Setup**

   * Create ASP.NET MVC project.
   * Configure database connection.

2. **Authentication**

   * Implement register/login/logout with custom authentication middleware.

3. **Database Modeling**

   * Define entities (User, ActivityPost, Participation, ChatMessage).
   * Apply EF Core migrations and update database.

4. **Activity Post Management**

   * CRUD for activities.
   * Validate max participants and deadline.

5. **Participation System**

   * Join/unjoin activity.
   * Owner approves/rejects.
   * AJAX used for join/unjoin updates.

6. **Chat Integration (Optional)**

   * Add chat per activity post.
   * Store messages in database.

7. **Calendar Integration (Optional)**

   * Display activities grouped by date.

8. **Social Credit (Optional)**

   * Allow anonymous rating after event.
   * Update user’s social credit score.

9. **Frontend Design**

   * Build responsive pages for desktop/mobile.
   * Use simple CSS grid/flex layouts.

10. **Testing & Deployment**

    * Unit test controllers/models.
    * Deploy on IIS/Azure.

---

## 8. Deliverables

* **Working web application** (ASP.NET Core MVC + SQL Server).
* **Documentation**: ERD, Use Case Diagram, Class Diagram, Sequence Diagram.
* **Presentation**:

  * Requirements & features.
  * UI demo.
  * Database structure.
  * How features were implemented.
