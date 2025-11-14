# Onboarding Controller Documentation

**Route Prefix:** `/api/onboarding`

**⚠️ NOTE:** These endpoints are not yet tested. Use with caution in production environments.

This documentation covers four interconnected controllers for managing onboarding content: **Folders**, **Tasks**, **Steps**, and **Materials**. Together they form a hierarchical structure for organizing and delivering onboarding content.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Folder Controller](#folder-controller)
- [Task Controller](#task-controller)
- [Steps Controller](#steps-controller)
- [Material Controller](#material-controller)
- [Common Workflows](#common-workflows)
- [Error Handling](#error-handling)

---

## Architecture Overview

The onboarding system is organized in a hierarchical structure:

```
OnboardingFolder
├── OnboardingTask
│   ├── OnboardingSteps (ordered by sequence)
│   └── OnboardingMaterials (files/resources)
```

### Key Concepts

- **Folders**: Top-level organizational units that group related tasks
- **Tasks**: Individual onboarding assignments within a folder, containing steps and materials
- **Steps**: Sequential instructions for completing a task (ordered by `SequenceOrder`)
- **Materials**: Files and resources (PDF, images, documents) uploaded to Azure Blob Storage and indexed by AI service

### Authorization

All endpoints require authentication. Creation, update, and deletion operations require **SuperAdmin** or **Admin** role, except for read operations which require any authenticated user.

---

## Folder Controller

**Route:** `/api/onboarding/folders`

Manages top-level organizational folders for grouping onboarding tasks.

### Get All Folders

```http
GET /api/onboarding/folders
```

**Authorization:** Requires authentication

**Returns:** List of all onboarding folders, ordered by title

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Engineering Onboarding",
    "description": "Complete onboarding program for engineering department",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "taskCount": 5
  }
]
```

### Get Folder by ID

```http
GET /api/onboarding/folders/{id}
```

**Authorization:** Requires authentication

**Parameters:**
- `id` (path, required): Folder ID

**Returns:** Specific folder with nested task count

**Response (200 OK):** See response structure above

**Response (404 Not Found):**
```json
{
  "message": "Onboarding folder not found."
}
```

### Create Folder

```http
POST /api/onboarding/folders
Content-Type: application/json
```

**Authorization:** Requires SuperAdmin or Admin role

**Request Body:**
```json
{
  "title": "Engineering Onboarding",
  "description": "Complete onboarding program for engineering department"
}
```

**Validation:**
- `title` (required): 1-255 characters
- `description` (required): 1-1000 characters
- Title must be unique

**Response (201 Created):** Folder object (see Get Folder response)

**Response (400 Bad Request):**
```json
{
  "message": "Title is required and must be between 1 and 255 characters."
}
```

**Response (409 Conflict):**
```json
{
  "message": "A folder with this title already exists."
}
```

### Update Folder

```http
PUT /api/onboarding/folders/{id}
Content-Type: application/json
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Folder ID

**Request Body:**
```json
{
  "title": "Updated Title",
  "description": "Updated description"
}
```

**Validation:** Same as Create Folder

**Response (200 OK):** Updated folder object

**Response (404 Not Found):**
```json
{
  "message": "Onboarding folder not found."
}
```

### Delete Folder

```http
DELETE /api/onboarding/folders/{id}
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Folder ID

**Note:** Cascading delete will remove all associated tasks, steps, and materials. Files in Azure Blob Storage should be cleaned up separately.

**Response (204 No Content):** No response body

**Response (404 Not Found):**
```json
{
  "message": "Onboarding folder not found."
}
```

---

## Task Controller

**Route:** `/api/onboarding/tasks`

Manages individual onboarding tasks within folders. Tasks contain steps and materials.

### Get All Tasks

```http
GET /api/onboarding/tasks
```

**Authorization:** Requires authentication

**Returns:** List of all onboarding tasks with material and step counts, ordered by title

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Company Policies Review",
    "description": "Review essential company policies and procedures",
    "folderId": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "materialCount": 3,
    "stepCount": 4
  }
]
```

### Get Tasks by Folder

```http
GET /api/onboarding/tasks/folder/{folderId}
```

**Authorization:** Requires authentication

**Parameters:**
- `folderId` (path, required): Folder ID

**Returns:** List of tasks in specified folder

**Response (200 OK):** Array of task objects (see Get All Tasks response)

**Response (404 Not Found):**
```json
{
  "message": "Onboarding folder not found."
}
```

### Get Task by ID

```http
GET /api/onboarding/tasks/{id}
```

**Authorization:** Requires authentication

**Parameters:**
- `id` (path, required): Task ID

**Returns:** Task with nested materials and steps

**Response (200 OK):** See Get All Tasks response

**Response (404 Not Found):**
```json
{
  "message": "Onboarding task not found."
}
```

### Create Task

```http
POST /api/onboarding/tasks
Content-Type: application/json
```

**Authorization:** Requires SuperAdmin or Admin role

**Request Body:**
```json
{
  "title": "Company Policies Review",
  "description": "Review essential company policies and procedures",
  "folderId": 1
}
```

**Validation:**
- `title` (required): 1-255 characters
- `description` (required): 1-1000 characters
- `folderId` (required): Must reference an existing folder

**Response (201 Created):** Task object (see Get Task response)

**Response (400 Bad Request):**
```json
{
  "message": "The specified folder does not exist."
}
```

### Update Task

```http
PUT /api/onboarding/tasks/{id}
Content-Type: application/json
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Task ID

**Request Body:**
```json
{
  "title": "Updated Title",
  "description": "Updated description"
}
```

**Note:** Cannot update `folderId` through this endpoint

**Response (200 OK):** Updated task object

**Response (404 Not Found):**
```json
{
  "message": "Onboarding task not found."
}
```

### Delete Task

```http
DELETE /api/onboarding/tasks/{id}
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Task ID

**Note:** Cascading delete will remove all associated steps and materials.

**Response (204 No Content):** No response body

**Response (404 Not Found):**
```json
{
  "message": "Onboarding task not found."
}
```

---

## Steps Controller

**Route:** `/api/onboarding/steps`

Manages sequential steps within tasks. Steps are ordered by `SequenceOrder`.

### Get All Steps

```http
GET /api/onboarding/steps
```

**Authorization:** Requires authentication

**Returns:** List of all steps, ordered by TaskId then SequenceOrder

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "stepDescription": "Read the employee handbook",
    "sequenceOrder": 1,
    "taskId": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
]
```

### Get Steps by Task

```http
GET /api/onboarding/steps/task/{taskId}
```

**Authorization:** Requires authentication

**Parameters:**
- `taskId` (path, required): Task ID

**Returns:** Steps for specified task, ordered by SequenceOrder

**Response (200 OK):** Array of step objects (see Get All Steps response)

**Response (404 Not Found):**
```json
{
  "message": "Onboarding task not found."
}
```

### Get Step by ID

```http
GET /api/onboarding/steps/{id}
```

**Authorization:** Requires authentication

**Parameters:**
- `id` (path, required): Step ID

**Response (200 OK):** Step object (see Get All Steps response)

**Response (404 Not Found):**
```json
{
  "message": "Onboarding step not found."
}
```

### Create Step

```http
POST /api/onboarding/steps
Content-Type: application/json
```

**Authorization:** Requires SuperAdmin or Admin role

**Request Body:**
```json
{
  "stepDescription": "Read the employee handbook",
  "taskId": 1,
  "sequenceOrder": 1
}
```

**Validation:**
- `stepDescription` (required): 1-1000 characters
- `taskId` (required): Must reference an existing task
- `sequenceOrder` (optional): If not provided, automatically assigned as next available number

**Note:** Uses a serializable database transaction to prevent race conditions when auto-assigning sequence numbers.

**Response (201 Created):** Step object (see Get Step response)

**Response (400 Bad Request):**
```json
{
  "message": "The specified task does not exist."
}
```

### Update Step

```http
PUT /api/onboarding/steps/{id}
Content-Type: application/json
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Step ID

**Request Body:**
```json
{
  "stepDescription": "Updated step description",
  "sequenceOrder": 2
}
```

**Validation:**
- `stepDescription` (required): 1-1000 characters
- `sequenceOrder` (optional): If provided, triggers automatic reordering of all steps in the task

**Reordering Behavior:** When `sequenceOrder` is updated, all steps in the task are automatically reordered. The system clamps the sequence order to valid range (1 to total steps).

**Response (200 OK):** Updated step object

**Response (404 Not Found):**
```json
{
  "message": "Onboarding step not found."
}
```

### Delete Step

```http
DELETE /api/onboarding/steps/{id}
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Step ID

**Response (204 No Content):** No response body

**Response (404 Not Found):**
```json
{
  "message": "Onboarding step not found."
}
```

---

## Material Controller

**Route:** `/api/onboarding/materials`

Manages onboarding materials (files) with upload to Azure Blob Storage and AI document indexing.

### Supported File Types

**Allowed Extensions:** `.pdf`, `.txt`, `.md`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.jpg`, `.jpeg`, `.png`, `.gif`

**Allowed Content Types:**
- `application/pdf`
- `text/plain`, `text/markdown`, `text/x-markdown`, `application/x-markdown`
- `application/vnd.openxmlformats-officedocument.wordprocessingml.document` (`.docx`)
- `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` (`.xlsx`)
- `image/jpeg`, `image/png`, `image/gif`

**Maximum File Size:** 50 MB

**AI-Indexed Formats:** PDF, JSON, and Markdown files are automatically uploaded to the AI service for document indexing and semantic search.

### Get All Materials

```http
GET /api/onboarding/materials
```

**Authorization:** Requires authentication

**Returns:** List of all onboarding materials, ordered by filename

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "fileName": "employee_handbook.pdf",
    "fileType": "application/pdf",
    "url": "https://mystorageaccount.blob.core.windows.net/onboarding/file_20240115_abc123.pdf",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "taskId": 1
  }
]
```

### Get Materials by Task

```http
GET /api/onboarding/materials/task/{taskId}
```

**Authorization:** Requires authentication

**Parameters:**
- `taskId` (path, required): Task ID

**Returns:** Materials for specified task

**Response (200 OK):** Array of material objects (see Get All Materials response)

**Response (404 Not Found):**
```json
{
  "message": "Onboarding task not found."
}
```

### Get Material by ID

```http
GET /api/onboarding/materials/{id}
```

**Authorization:** Requires authentication

**Parameters:**
- `id` (path, required): Material ID

**Response (200 OK):** Material object (see Get All Materials response)

**Response (404 Not Found):**
```json
{
  "message": "Onboarding material not found."
}
```

### Create Material (Upload File)

```http
POST /api/onboarding/materials
Content-Type: multipart/form-data
```

**Authorization:** Requires SuperAdmin or Admin role

**Form Data:**
- `file` (required): File to upload
- `taskId` (required): Task ID to associate with material

**Process:**
1. File is validated (size, type, extension)
2. File uploaded to Azure Blob Storage with unique name
3. Material record created in database
4. If supported format, file is indexed by AI service (non-blocking)

**Response (201 Created):**
```json
{
  "id": 1,
  "fileName": "employee_handbook.pdf",
  "fileType": "application/pdf",
  "url": "https://mystorageaccount.blob.core.windows.net/onboarding/file_20240115_abc123.pdf",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null,
  "taskId": 1
}
```

**Response (400 Bad Request):**
```json
{
  "message": "File size cannot exceed 50MB."
}
```

**Response (500 Internal Server Error):**
```json
{
  "message": "File upload failed. Please try again."
}
```

**Error Handling:** If database save fails after blob upload, the blob is automatically deleted to prevent orphaned files.

### Update Material

```http
PUT /api/onboarding/materials/{id}
Content-Type: multipart/form-data
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Material ID

**Form Data:**
- `file` (optional): New file to replace existing file

**Behavior:**
- If `file` is provided: Old file deleted from blob storage, new file uploaded, AI service updated
- If `file` is not provided: Only `UpdatedAt` timestamp is refreshed

**Process:**
1. New file validated
2. New file uploaded to Azure Blob Storage
3. Database updated with new URL
4. AI service updated (if supported format)
5. Old blob deleted

**Response (200 OK):** Updated material object

**Response (404 Not Found):**
```json
{
  "message": "Onboarding material not found."
}
```

**Error Handling:** If old blob deletion fails after successful DB update, error is logged but operation succeeds.

### Delete Material

```http
DELETE /api/onboarding/materials/{id}
```

**Authorization:** Requires SuperAdmin or Admin role

**Parameters:**
- `id` (path, required): Material ID

**Process:**
1. If supported format, delete from AI service (non-blocking)
2. Delete from database
3. Delete blob from Azure Blob Storage (non-blocking)

**Response (204 No Content):** No response body

**Response (404 Not Found):**
```json
{
  "message": "Onboarding material not found."
}
```

**Error Handling:** Failures in AI service or blob storage deletion are logged but don't fail the operation.

---

## Common Workflows

### Workflow 1: Create Complete Onboarding Program

```bash
# 1. Create folder
curl -X POST http://localhost:5164/api/onboarding/folders \
  -H "Content-Type: application/json" \
  -H "Cookie: {auth-cookie}" \
  -d '{
    "title": "Engineering Onboarding",
    "description": "Complete onboarding for engineering team"
  }'
# Response includes: id (e.g., 1)

# 2. Create task in folder
curl -X POST http://localhost:5164/api/onboarding/tasks \
  -H "Content-Type: application/json" \
  -H "Cookie: {auth-cookie}" \
  -d '{
    "title": "Company Policies Review",
    "description": "Review essential company policies",
    "folderId": 1
  }'
# Response includes: id (e.g., 1)

# 3. Create steps for task
curl -X POST http://localhost:5164/api/onboarding/steps \
  -H "Content-Type: application/json" \
  -H "Cookie: {auth-cookie}" \
  -d '{
    "stepDescription": "Read employee handbook",
    "taskId": 1
  }'

curl -X POST http://localhost:5164/api/onboarding/steps \
  -H "Content-Type: application/json" \
  -H "Cookie: {auth-cookie}" \
  -d '{
    "stepDescription": "Review code of conduct",
    "taskId": 1,
    "sequenceOrder": 2
  }'

# 4. Upload materials
curl -X POST http://localhost:5164/api/onboarding/materials \
  -H "Cookie: {auth-cookie}" \
  -F "file=@employee_handbook.pdf" \
  -F "taskId=1"
```

### Workflow 2: Reorder Steps

```bash
# Get current steps
curl -X GET http://localhost:5164/api/onboarding/steps/task/1 \
  -H "Cookie: {auth-cookie}"

# Move step 2 to position 1
curl -X PUT http://localhost:5164/api/onboarding/steps/2 \
  -H "Content-Type: application/json" \
  -H "Cookie: {auth-cookie}" \
  -d '{
    "stepDescription": "Review code of conduct",
    "sequenceOrder": 1
  }'

# All steps in task are automatically reordered
```

### Workflow 3: Update Material (Replace File)

```bash
# Update material with new file
curl -X PUT http://localhost:5164/api/onboarding/materials/1 \
  -H "Cookie: {auth-cookie}" \
  -F "file=@updated_handbook.pdf"

# Old file automatically deleted, new file uploaded, AI service updated
```

---

## Error Handling

### Common Error Scenarios

| Scenario | Status Code | Response |
|----------|------------|----------|
| Not authenticated | 401 | Redirect to login |
| Insufficient role | 403 | `Unauthorized` |
| Resource not found | 404 | `{"message": "... not found."}` |
| Validation failure | 400 | `{"message": "Validation error..."}` |
| Duplicate title (folders) | 409 | `{"message": "A folder with this title already exists."}` |
| File upload failure | 500 | `{"message": "File upload failed. Please try again."}` |
| AI service failure | 200 | Logged warning, operation succeeds |
| Blob deletion failure | 200 | Logged warning, operation succeeds |

### Resilience Features

- **Orphaned blob prevention**: If DB save fails after blob upload, blob is deleted
- **AI service non-blocking**: Failures in AI indexing don't fail the request
- **Blob deletion non-blocking**: Failures deleting old blobs don't fail update/delete requests
- **Transaction support**: Step creation and reordering use serializable transactions to prevent race conditions

---

## Notes

- These endpoints are not yet tested in production
- All timestamps are in UTC format
- File URLs point to Azure Blob Storage and remain accessible after material deletion (blob is deleted separately)
- AI document indexing currently supports: PDF, JSON, Markdown
- Large file uploads (>50MB) will be rejected
