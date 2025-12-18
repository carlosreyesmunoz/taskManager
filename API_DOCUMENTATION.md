# API Documentation - Request/Response DTOs

## Overview
All create and update operations now use DTOs (Data Transfer Objects) with ID references instead of full entity objects.

---

## Organizations API

### Create Organization
**Endpoint:** `POST /api/organizations`

**Request Body:**
```json
{
  "name": "My Organization",
  "description": "Optional description",
  "ownerId": "user-id-here"
}
```

**Response:** Full Organization object with owner details populated

### Update Organization
**Endpoint:** `PUT /api/organizations/{id}`

**Request Body:**
```json
{
  "name": "Updated Name",
  "description": "Updated description",
  "ownerId": "new-owner-id"
}
```
*All fields are optional - only include fields you want to update*

---

## Users API

### Create User
**Endpoint:** `POST /api/users`

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "role": "user",
  "organizationId": "org-id-here",
  "isActive": true
}
```

**Response:** Full User object with organization details populated

### Update User
**Endpoint:** `PUT /api/users/{id}`

**Request Body:**
```json
{
  "name": "Updated Name",
  "email": "newemail@example.com",
  "role": "admin",
  "organizationId": "new-org-id",
  "points": 100,
  "isActive": true
}
```
*All fields are optional - only include fields you want to update*

**Validations:**
- Email uniqueness is enforced
- Organization must exist

---

## Tasks API

### Create Task
**Endpoint:** `POST /api/tasks`

**Request Body:**
```json
{
  "title": "Task Title",
  "description": "Optional description",
  "dueDate": "2024-12-31T23:59:59Z",
  "organizationId": "org-id-here",
  "createdById": "creator-user-id",
  "assignedToId": "assignee-user-id"
}
```

**Notes:**
- `assignedToId` is optional - if omitted, task goes to the pool
- Organization, creator, and assignee (if provided) are validated to exist

**Response:** Full Task object with all navigation properties populated

### Update Task
**Endpoint:** `PUT /api/tasks/{id}`

**Request Body:**
```json
{
  "title": "Updated Title",
  "description": "Updated description",
  "dueDate": "2024-12-31T23:59:59Z",
  "assignedToId": "new-assignee-id"
}
```
*All fields are optional - only include fields you want to update*

### Assign Task
**Endpoint:** `POST /api/tasks/{id}/assign/{userId}`

Assigns a task to a specific user.

### Pick Task
**Endpoint:** `POST /api/tasks/{id}/pick/{userId}`

Allows a user to pick an unassigned task from the pool.

### Complete Task
**Endpoint:** `POST /api/tasks/{id}/complete/{userId}`

Marks a task as completed by the assigned user.

### Finalize Task
**Endpoint:** `POST /api/tasks/{id}/finalize/{userId}`

Finalizes a completed task and awards points to the user.

---

## User Invitations API

### Create Invitation
**Endpoint:** `POST /api/invitations`

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "role": "user",
  "organizationId": "org-id-here",
  "invitedById": "inviter-user-id",
  "expiresAt": "2024-12-31T23:59:59Z"
}
```

**Notes:**
- `expiresAt` is optional - defaults to 7 days from now
- Organization and inviter are validated to exist
- If an active invitation already exists for this email in the organization, it will be updated

**Response:** Full UserInvitation object with token

### Accept Invitation
**Endpoint:** `POST /api/invitations/accept/{token}`

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "must-match-invitation@example.com",
  "role": "user",
  "organizationId": "will-be-overridden",
  "isActive": true
}
```

**Notes:**
- Email, role, and organizationId will be taken from the invitation
- Only provide the user's name in the request
- Creates a new user and marks the invitation as accepted

---

## Validation & Error Handling

### Automatic Validations

All create/update operations now include:

1. **Foreign Key Validation**
   - All referenced IDs (ownerId, organizationId, userId, etc.) are verified to exist
   - Returns `400 Bad Request` with error message if not found

2. **Email Uniqueness**
   - User emails must be unique across the system
   - Returns `400 Bad Request` if email already exists

3. **Business Rule Validation**
   - Task assignment rules (uncompleted status, not already assigned)
   - Invitation expiration and acceptance rules

### Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "": [
      "User with ID user-123 not found"
    ]
  }
}
```

---

## Navigation Properties

When you GET an entity, all relevant navigation properties are populated:

- **Organization**: Includes `Owner` (User object)
- **User**: Includes `Organization` object
- **Task**: Includes `Creator`, `Assignee`, and `Organization` objects
- **UserInvitation**: Includes `Organization` and `Inviter` objects
- **TaskHistory**: Includes `Task`, `User`, `PreviousAssignee`, and `NewAssignee` objects

---

## Example Workflow

### 1. Create an Organization
```bash
POST /api/organizations
{
  "name": "Acme Corp",
  "description": "My company",
  "ownerId": "existing-user-id"
}
```

### 2. Create a User in that Organization
```bash
POST /api/users
{
  "name": "Jane Smith",
  "email": "jane@acme.com",
  "role": "user",
  "organizationId": "org-id-from-step-1",
  "isActive": true
}
```

### 3. Create a Task
```bash
POST /api/tasks
{
  "title": "Setup project",
  "description": "Initialize the project repository",
  "organizationId": "org-id-from-step-1",
  "createdById": "creator-user-id",
  "assignedToId": "jane-user-id-from-step-2"
}
```

### 4. Send an Invitation
```bash
POST /api/invitations
{
  "email": "newmember@example.com",
  "role": "user",
  "organizationId": "org-id-from-step-1",
  "invitedById": "existing-user-id"
}
```

---

## Benefits of ID-Based APIs

✅ **Cleaner requests** - Only send IDs instead of full objects
✅ **Validation** - All references are verified to exist
✅ **Consistency** - Prevents data inconsistencies
✅ **Performance** - Reduces payload size
✅ **Security** - Can't accidentally override related object properties

---

*Last Updated: December 18, 2024*
