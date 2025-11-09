# Quiz Submission Fix

## Problem
When students clicked to submit answers, the submission was failing.

## Root Cause
The frontend was sending data in a format that didn't match the backend's expectations:

1. **Multiple Choice**: Frontend sent `selected_choices` in `answer_data`, but backend expected `selected_choice_indices` as a direct FormData field
2. **Word Cloud**: Frontend sent `words` as an array, but backend expected `answer_text` as a comma-separated string
3. **API Service**: Was sending all data as `answer_data` JSON, but backend expects specific field names for each quiz type

## Changes Made

### 1. Fixed API Service (`student-portal/src/services/api.ts`)
- Updated `submitAnswer` to send data in the correct format for each quiz type
- Multiple choice: sends `selected_choice_indices` as JSON array
- Short answer & Word cloud: sends `answer_text` as string
- Added better error logging with detailed console messages

### 2. Fixed MultipleChoiceQuiz Component
- Simplified to only send `selected_choices` array
- Removed unused `choice_texts` field

### 3. Fixed WordCloudQuiz Component
- Changed to send `answer_text` as comma-separated string instead of array
- Backend will parse this and create the word array

### 4. Updated CORS Settings
- Added port 5174 to allowed origins (Vite started on alternate port)

## Backend Expectations

Each quiz type expects specific data format:

### Multiple Choice
```
FormData fields:
- quiz_id: integer
- selected_choice_indices: JSON array [0, 1, 2]
```

### Short Answer
```
FormData fields:
- quiz_id: integer
- answer_text: string
```

### Word Cloud
```
FormData fields:
- quiz_id: integer
- answer_text: comma-separated string "word1, word2, word3"
```

### Drawing / Image Upload
```
FormData fields:
- quiz_id: integer
- uploaded_file: File object
- answer_data: JSON with metadata (optional)
```

## Testing

### Student Portal is now running on: **http://localhost:5174**

Test with:
- Class code: `5364`
- Any student name

Try submitting each quiz type:
1. ✅ Multiple Choice: "What is 2 + 2?"
2. ✅ Short Answer: "Explain Photosynthesis"
3. ✅ Word Cloud: "Words Related to Science"
4. ✅ Drawing: "Draw a Circle"
5. ✅ Image Upload: "Upload Your Notes"

## Verification

Check submissions in Django admin:
- URL: http://localhost:8000/admin
- Username: `testteacher`
- Password: `password123`
- Navigate to: Student Answers section
