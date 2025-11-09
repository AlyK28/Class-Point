using ClassPointAddIn.Api.Service.ClassPointAddIn.Api.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Services.QuizService
{
    public class QuizChoice
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuizProperties
    {
        public string QuestionText { get; set; }
        public bool AllowMultipleChoices { get; set; }
        public List<QuizChoice> Choices { get; set; }
        public int NumberOfChoices { get; set; }
        public bool HasCorrectAnswer { get; set; }
        public bool CompetitionMode { get; set; }
        public bool RandomizeChoiceOrder { get; set; }
        public int PointsPerCorrect { get; set; }
        public int PenaltyPerWrong { get; set; }

        public QuizProperties()
        {
            Choices = new List<QuizChoice>();
            AllowMultipleChoices = false;
            HasCorrectAnswer = true;
            CompetitionMode = false;
            RandomizeChoiceOrder = false;
            PointsPerCorrect = 1;
            PenaltyPerWrong = 0;
        }
    }

    public class CreateMultipleChoiceQuizRequest
    {
        public int Course { get; set; }
        public string Title { get; set; }
        public QuizProperties Properties { get; set; }

        public CreateMultipleChoiceQuizRequest()
        {
            Properties = new QuizProperties();
        }
    }

    public class ShortAnswerQuizProperties
    {
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string ExpectedKeywords { get; set; }
        public bool CaseSensitive { get; set; }
        public int? MaxLength { get; set; }
        public bool UseRegex { get; set; }

        public ShortAnswerQuizProperties()
        {
            CaseSensitive = false;
            UseRegex = false;
        }
    }

    public class CreateShortAnswerQuizRequest
    {
        public int Course { get; set; }
        public string Title { get; set; }
        public ShortAnswerQuizProperties Properties { get; set; }

        public CreateShortAnswerQuizRequest()
        {
            Properties = new ShortAnswerQuizProperties();
        }
    }

    public class QuizResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Course { get; set; }
        public string QuizType { get; set; }
        public QuizProperties Properties { get; set; }
        public string CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChoiceStats
    {
        public int Index { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
        public int Percentage { get; set; }
        public bool IsCorrect { get; set; }
        public List<string> Students { get; set; }  // Student names who selected this choice
    }

    public class QuizSubmissionStatsResponse
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int TotalSubmissions { get; set; }
        public int EnrolledStudents { get; set; }
        public List<ChoiceStats> ChoiceStats { get; set; }
    }

    public class ShortAnswerStatsResponse
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string QuestionText { get; set; }
        public int TotalSubmissions { get; set; }
        public int EnrolledStudents { get; set; }
        public List<StudentSubmission> Submissions { get; set; }
    }

    public class StudentSubmission
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string Answer { get; set; }
        public string SubmittedAt { get; set; }
        public bool IsLiked { get; set; }
    }

    public class QuizApiService : BaseApiClient
    {
        public QuizApiService() : base("/api/quizzes/")
        {
        }

        public async Task<QuizResponse> CreateMultipleChoiceQuizAsync(CreateMultipleChoiceQuizRequest request)
        {
            return await PostAsync<CreateMultipleChoiceQuizRequest, QuizResponse>("create/multiple-choice/", request);
        }

        public async Task<QuizResponse> CreateShortAnswerQuizAsync(CreateShortAnswerQuizRequest request)
        {
            return await PostAsync<CreateShortAnswerQuizRequest, QuizResponse>("create/short-answer/", request);
        }

        public async Task<List<QuizResponse>> GetQuizzesForCourseAsync(int courseId)
        {
            // Get all quizzes and filter by course ID on client side
            // The backend already filters by logged-in user
            var allQuizzes = await GetAsync<List<QuizResponse>>("");
            return allQuizzes.FindAll(q => q.Course == courseId);
        }

        public async Task<QuizResponse> GetQuizAsync(int quizId)
        {
            var result = await GetAsync<QuizResponse>($"{quizId}/");
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"[QuizApiService] GetQuizAsync returned:");
            System.Diagnostics.Debug.WriteLine($"  ID: {result?.Id}");
            System.Diagnostics.Debug.WriteLine($"  Title: {result?.Title}");
            System.Diagnostics.Debug.WriteLine($"  QuizType: '{result?.QuizType}'");
            
            return result;
        }

        public async Task<QuizSubmissionStatsResponse> GetQuizSubmissionStatsAsync(int quizId)
        {
            return await GetAsync<QuizSubmissionStatsResponse>($"{quizId}/stats/");
        }

        public async Task<ShortAnswerStatsResponse> GetShortAnswerStatsAsync(int quizId)
        {
            return await GetAsync<ShortAnswerStatsResponse>($"{quizId}/short-answer-stats/");
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
        {
            try
            {
                await DeleteAsync($"{quizId}/");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task CloseQuizSubmissionsAsync(int quizId)
        {
            // PATCH request to mark quiz as inactive (close submissions)
            var request = new { is_active = false };
            await PatchAsync<object, QuizResponse>($"{quizId}/", request);
        }
    }
}