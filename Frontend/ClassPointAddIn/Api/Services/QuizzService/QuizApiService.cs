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

    public class QuizApiService : BaseApiClient
    {
        public QuizApiService() : base("/api/quizzes/")
        {
        }

        public async Task<QuizResponse> CreateMultipleChoiceQuizAsync(CreateMultipleChoiceQuizRequest request)
        {
            return await PostAsync<CreateMultipleChoiceQuizRequest, QuizResponse>("create/multiple-choice/", request);
        }

        public async Task<List<QuizResponse>> GetQuizzesForCourseAsync(int courseId)
        {
            return await GetAsync<List<QuizResponse>>($"course/{courseId}/");
        }

        public async Task<QuizResponse> GetQuizAsync(int quizId)
        {
            return await GetAsync<QuizResponse>($"{quizId}/");
        }
    }
}