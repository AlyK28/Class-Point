# Quiz Deletion Feature

## Overview
This document describes the quiz deletion functionality that allows teachers to remove quizzes from both PowerPoint slides and the backend database.

## Implementation Details

### Backend (Django)
- **DELETE Endpoint**: The `QuizViewSet` (ModelViewSet) automatically provides a DELETE endpoint at `/api/quizzes/<id>/`
- **Authorization**: Only the quiz creator (teacher) can delete their own quizzes
- **Location**: `Backend/quizzes/views.py`

### Frontend (C# PowerPoint Add-in)

#### 1. BaseApiClient Enhancement
**File**: `Frontend/ClassPointAddIn/Api/BaseApiClient.cs`

Added `DeleteAsync` method to support HTTP DELETE requests:
```csharp
protected async Task DeleteAsync(string endpoint)
{
    ApplyAuthorizationHeader();
    var response = await _httpClient.DeleteAsync(endpoint);

    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && await TryRefreshTokenAsync())
    {
        ApplyAuthorizationHeader();
        response = await _httpClient.DeleteAsync(endpoint); // retry once
    }

    if (!response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        throw new Exception($"Delete request failed: {response.StatusCode} - {json}");
    }
}
```

#### 2. QuizApiService
**File**: `Frontend/ClassPointAddIn/Api/Services/QuizzService/QuizApiService.cs`

Added methods:
- `DeleteQuizAsync(int quizId)`: Deletes a single quiz
- `GetQuizzesForCourseAsync(int courseId)`: Retrieves all quizzes for a course

#### 3. Cleanup Functionality
**File**: `Frontend/ClassPointAddIn/ThisAddIn.cs`

Added `CleanupDeletedQuizButtonsAsync()` method that:
1. Scans all slides in the presentation for quiz buttons
2. Extracts quiz IDs from shape tags
3. Fetches all quizzes for the current course from the API
4. Identifies orphaned quizzes (quizzes without corresponding buttons)
5. Asks user for confirmation
6. Deletes orphaned quizzes from the backend

#### 4. Ribbon Integration
**Files**: 
- `Frontend/ClassPointAddIn/Views/DynamicRibbon.xml`
- `Frontend/ClassPointAddIn/Views/DynamicRibbon.cs`

Added a new ribbon button:
- **Label**: "Cleanup Deleted Quizzes"
- **Location**: Teacher tab → Quizzes group
- **Icon**: DeleteTableRows
- **Handler**: `OnCleanupQuizzesClick()`

## Usage

### For Teachers

1. **Delete a Quiz Button**:
   - Simply delete the quiz button shape from your PowerPoint slide
   - The quiz still exists in the database at this point

2. **Cleanup Orphaned Quizzes**:
   - Click the **"Cleanup Deleted Quizzes"** button in the Teacher ribbon
   - The system will scan all slides and identify quizzes without buttons
   - Review the list of orphaned quizzes in the confirmation dialog
   - Click "Yes" to permanently delete these quizzes from the database
   - Click "No" to cancel without deleting

### Why Manual Cleanup?

PowerPoint doesn't provide a direct event when shapes are deleted, so we can't automatically detect button deletion. Instead, we provide a manual cleanup tool that:
- Gives teachers control over when cleanup happens
- Shows exactly which quizzes will be deleted before confirmation
- Prevents accidental deletions
- Can be run anytime to maintain database hygiene

## Technical Notes

### Shape Tag Detection
Quiz buttons are identified by checking shape tags (case-insensitive):
- `QuizButton` = `MultiChoiceQuiz`
- `QuizId` = `<quiz_id_number>`

PowerPoint converts tag names to UPPERCASE, so all comparisons use `StringComparison.OrdinalIgnoreCase`.

### API Authorization
The DELETE endpoint requires:
- Valid JWT token in Authorization header
- Quiz must be owned by the requesting user
- Returns 404 if quiz doesn't exist or user doesn't have permission

### Error Handling
The cleanup process:
- Continues on individual deletion failures
- Reports success/failure counts to the user
- Logs detailed error information for debugging
- Validates that teacher is logged in and has an active presentation

## Future Enhancements

Potential improvements:
1. **Automatic cleanup on presentation close**: Scan and cleanup when teacher closes the presentation
2. **Periodic background cleanup**: Run cleanup automatically every N minutes
3. **Undo functionality**: Allow recovery of recently deleted quizzes
4. **Batch operations**: Multi-select quizzes for manual deletion from a list view
5. **Context menu integration**: Right-click on quiz button → "Delete Quiz Permanently"

## Related Files

- `Backend/quizzes/views.py` - Quiz ViewSet with DELETE support
- `Backend/quizzes/urls.py` - URL routing for quiz endpoints
- `Frontend/ClassPointAddIn/Api/BaseApiClient.cs` - HTTP client base class
- `Frontend/ClassPointAddIn/Api/Services/QuizzService/QuizApiService.cs` - Quiz API service
- `Frontend/ClassPointAddIn/ThisAddIn.cs` - Main add-in logic
- `Frontend/ClassPointAddIn/Views/DynamicRibbon.cs` - Ribbon event handlers
- `Frontend/ClassPointAddIn/Views/DynamicRibbon.xml` - Ribbon UI definition
