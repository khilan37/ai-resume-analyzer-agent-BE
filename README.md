# Smart Resume Analyzer Agent Backend

ASP.NET Core Web API (.NET 8) with Clean Architecture, PostgreSQL, Entity Framework Core, JWT authentication, and Semantic Kernel based Gemini integration.

## Folder Structure

```text
src/
  SmartResumeAnalyzerAgent.Api/
  SmartResumeAnalyzerAgent.Application/
  SmartResumeAnalyzerAgent.Domain/
  SmartResumeAnalyzerAgent.Infrastructure/
docs/
  postgresql-setup.sql
```

## Backend Setup

1. Update `src/SmartResumeAnalyzerAgent.Api/appsettings.json` with your PostgreSQL and Gemini credentials.
2. Restore packages:
   `dotnet restore`
3. Create the first migration:
   `dotnet ef migrations add InitialCreate --project src/SmartResumeAnalyzerAgent.Infrastructure --startup-project src/SmartResumeAnalyzerAgent.Api`
4. Apply the database:
   `dotnet ef database update --project src/SmartResumeAnalyzerAgent.Infrastructure --startup-project src/SmartResumeAnalyzerAgent.Api`
5. Run the API:
   `dotnet run --project src/SmartResumeAnalyzerAgent.Api`

## API Endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/resumes/analyze`
- `GET /api/resumes/{resumeId}`
- `GET /api/resumes/history/{userId}`

## Semantic Kernel + Gemini Notes

The infrastructure service uses Semantic Kernel prompt invocation and expects the installed Semantic Kernel package version to expose `AddGoogleAIGeminiChatCompletion`. If your installed connector version names the extension differently, adjust that single registration line in `src/SmartResumeAnalyzerAgent.Infrastructure/AI/GeminiSemanticKernelResumeAnalyzerService.cs`.

## Sample Analyze Request

Use `multipart/form-data`:

- `file`: `resume.pdf`
- `userId`: `2e421edd-ae3d-4206-bf6e-193d1872bf8f`
- `jobRole`: `.NET Developer`

## Sample Analyze Response

```json
{
  "resumeId": "8b7e5661-dc9f-45dd-9d44-5e40d6d9478b",
  "fileName": "john-doe-resume.pdf",
  "jobRole": ".NET Developer",
  "score": 84,
  "summary": "Strong backend resume with solid .NET and SQL experience.",
  "overallFeedback": "Good technical depth but the resume needs stronger achievement statements.",
  "skills": ["ASP.NET Core", "C#", "Docker", "Entity Framework Core", "PostgreSQL"],
  "missingSkills": ["Azure DevOps", "Kubernetes"],
  "suggestions": ["Add quantified project outcomes", "Mention CI/CD exposure"],
  "matchedJobRoles": [".NET Developer", "Backend Engineer", "Software Engineer"],
  "createdAtUtc": "2026-03-19T12:00:00Z"
}
```
