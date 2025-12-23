<!-- Purpose: Project overview and setup instructions. -->

# DigitalLogbook

A PDF document logbook system for capturing form data, drawings, and generating flattened PDFs.

## Architecture

- **Backend**: .NET 8 minimal API with EF Core + SQLite
- **PDF Processing**: iText7 for form filling, overlays, and flattening
- **Storage**: Filesystem (templates, drafts, images, exports)

## Features

### Templates
- Upload PDF templates
- Store metadata (title, college name, original filename)
- List all templates

### Drafts
- Create drafts from templates
- Store form data as JSON
- Capture drawing as PNG overlay
- Multiple versions per user/template
- Ownership enforcement via user context

### Exports
- Generate flattened PDFs from drafts
- Fill AcroForm fields with draft data
- Add form data text overlay (fallback for non-form PDFs)
- Overlay drawing PNG if present
- Stream exported PDFs

## PDF Export Behavior

### Form Filling
- If template has AcroForm fields, they are populated from `draft.FormDataJson`
- Boolean values map to "Yes" (true) or "Off" (false)
- Unknown keys in form data are silently ignored

### Overlays
1. **Form Data Text**: Always added to page 1, top-left. Lists key/value pairs so export visibly reflects data even if template has no form fields.
2. **Drawing**: If `draft.DrawingImagePath` exists, PNG is overlaid at top-right (150x150 px).

### Output
- Flattened PDF (read-only)
- Saved to `storage/exports/{userId}/{draftId}.pdf`

## Database

### Migrations
```bash
# Add migration after schema changes
dotnet ef migrations add {MigrationName} --project packages/backend.data --startup-project apps/api

# Apply migrations
dotnet ef database update --project packages/backend.data --startup-project apps/api
```

### Schema
- **PdfTemplates**: Id, CollegeName, Title, OriginalFileName, StoredPath, CreatedAtUtc
- **PdfDrafts**: Id, TemplateId (FK), UserId, Version, FormDataJson, DrawingImagePath, Status, CreatedAtUtc, UpdatedAtUtc

## Storage Structure

```
storage/
├── app.db              # SQLite database
├── templates/          # Uploaded PDF templates
│   └── {templateId}.pdf
├── images/             # Drawing overlays
│   └── {userId}/
│       └── {draftId}.png
└── exports/            # Generated flattened PDFs
    └── {userId}/
        └── {draftId}.pdf
```

## Dependencies

### iText7
- **Package**: itext7 (v9.4.0+)
- **License**: AGPL + commercial license available
- **Note**: For non-AGPL use, purchase a commercial license from iText

## Endpoints

### Templates
- `POST /api/templates` - Upload template
- `GET /api/templates` - List all templates
- `GET /api/templates/{id}/file` - Stream template PDF

### Drafts
- `POST /api/drafts` - Create draft
- `GET /api/drafts?templateId={id}` - List drafts for template
- `GET /api/drafts/{id}` - Get draft details
- `GET /api/drafts/{id}/drawing` - Stream drawing PNG
- `POST /api/drafts/{id}/export` - Trigger PDF export
- `GET /api/drafts/{id}/export/file` - Stream exported PDF

### Utility
- `GET /health` - API health check

## Running

```bash
dotnet build
dotnet run --project apps/api
```

API starts on `http://localhost:5263` (HTTP) or `https://localhost:5001` (HTTPS with dev cert).

## Testing

Example workflow:
1. Upload template: `POST /api/templates`
2. Create draft: `POST /api/drafts`
3. Export PDF: `POST /api/drafts/{id}/export`
4. Download export: `GET /api/drafts/{id}/export/file`
