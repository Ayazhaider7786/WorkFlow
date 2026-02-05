# WORK FLOW (Work Management Tool)

A multi-tenant SaaS work management and ticketing application built with .NET 9 Web API and React 18.

## All Technologies

### Backend
- .NET 9 Web API
- Entity Framework Core with SQL Server
- JWT Authentication
- BCrypt password hashing

### Frontend
- React 18 with TypeScript
- Tailwind CSS
- React Router v6
- Axios for API calls
- Headless UI components
- React Hot Toast notifications

## How to Run

### Prerequisites
- .NET 9 SDK
- Node.js 18+
- SQL Server (LocalDB or full instance)

### Backend Setup

1. Navigate to backend folder:
```bash
cd backend/WorkManagement.API
```

2. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkManagement;Trusted_Connection=True;"
  }
}
```

3. Database Migrations
To run migration:
```powershell
add-migration <migrationName>
update-Database
```

4. Run the API:
```bash
dotnet run
```

The API will run on **https://localhost:5001**

Swagger UI: https://localhost:5001/swagger

### Frontend Setup

1. Navigate to frontend folder:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start development server:
```bash
npm run dev
```

The frontend will run on **http://localhost:3000** (or 5173)

## All features of application

- **Role-Based Access Control**
  - SuperAdmin: Company owner, can transfer status, create all user types
  - Admin: Can create Managers, Members, QA users
  - Manager: Manages projects and teams, can view all tickets
  - QA: Can create/delete tickets, comment on tickets
  - Member: Can view assigned tickets and tickets they created

- **Project Management**
  - Projects require at least one Manager
  - Multiple Managers can work on the same project
  - Members/QA inherit project visibility through their Manager

- **Ticket System**
  - Large, user-friendly ticket modal with tabs (Details, Comments, Activity)
  - Rich text editor with bold and bullet point support
  - File attachments (Images up to 10MB, Videos up to 70MB)
  - Full activity logging

- **Kanban Board** with drag-and-drop
- **Sprint Management** with planning, active, and completed states
- **File Ticket Tracking** for physical/digital documents

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Super Admin | superadmin@techcorp.com | SuperAdmin@123 |
| Admin | admin@techcorp.com | Admin@123 |
| Manager 1 | manager@techcorp.com | Manager@123 |
| Manager 2 | manager2@techcorp.com | Manager@123 |
| QA | qa@techcorp.com | QA@123 |
| Member 1 | member1@techcorp.com | Member@123 |
| Member 2 | member2@techcorp.com | Member@123 |
| Member 3 | member3@techcorp.com | Member@123 |

## Role Hierarchy

```
SuperAdmin (1 per company)
├── Admin (multiple)
│   └── Manager (multiple)
│       ├── QA (reports to manager)
│       └── Member (reports to manager)
```

### Permissions

| Action | SuperAdmin | Admin | Manager | QA | Member |
|--------|------------|-------|---------|-----|--------|
| Create Admin | ✓ | ✓ | ✗ | ✗ | ✗ |
| Create Manager | ✓ | ✓ | ✗ | ✗ | ✗ |
| Create QA | ✓ | ✓ | ✓ | ✗ | ✗ |
| Create Member | ✓ | ✓ | ✓ | ✗ | ✗ |
| Create Project | ✓ | ✓ | ✗ | ✗ | ✗ |
| Create Ticket | ✓ | ✓ | ✓ | ✓ | ✗ |
| Delete Ticket | ✓ | ✓ | ✓ | Own only | ✗ |
| View All Tickets | ✓ | ✓ | In project | Assigned | Assigned |
| Transfer SuperAdmin | ✓ | ✗ | ✗ | ✗ | ✗ |

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register company

### Users
- `GET /api/users` - List users
- `GET /api/users/me` - Current user
- `GET /api/users/team` - Get team members (for managers)
- `GET /api/users/managers` - Get all managers
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/transfer-super-admin` - Transfer SuperAdmin status

### Projects
- `GET /api/projects` - List projects
- `POST /api/projects` - Create project (requires managerId)
- `GET /api/projects/{id}/members` - List project members
- `POST /api/projects/{id}/members` - Add member

### Work Items (Tickets)
- `GET /api/projects/{projectId}/workitems` - List tickets
- `POST /api/projects/{projectId}/workitems` - Create ticket
- `PUT /api/projects/{projectId}/workitems/{id}` - Update ticket
- `DELETE /api/projects/{projectId}/workitems/{id}` - Delete ticket

### Comments
- `GET /api/projects/{projectId}/workitems/{workItemId}/comments`
- `POST /api/projects/{projectId}/workitems/{workItemId}/comments`
- `DELETE /api/projects/{projectId}/workitems/{workItemId}/comments/{id}`

### Attachments
- `GET /api/projects/{projectId}/workitems/{workItemId}/attachments`
- `POST /api/projects/{projectId}/workitems/{workItemId}/attachments` (multipart/form-data)
- `DELETE /api/projects/{projectId}/workitems/{workItemId}/attachments/{id}`
- `GET /api/projects/{projectId}/workitems/{workItemId}/attachments/{id}/download`

## Project Structure

```
WorkManagementTool/
├── backend/
│   └── WorkManagement.API/
│       ├── Controllers/
│       ├── Models/
│       ├── Data/
│       ├── Services/
│       ├── DTOs/
│       └── Program.cs
├── frontend/
│   └── src/
│       ├── components/
│       ├── pages/
│       ├── context/
│       ├── services/
│       └── types/
└── README.md
```

## License

MIT

Please help us improve by contribution/sponsorship , email me ayazhaider7786@gmail.com
