using ClassPointAddIn.Api.Service.ClassPointAddIn.Api.Service;
using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Services.ClassService
{
    public class ClassResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool Active { get; set; }
        public string CreatedAt { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string TeacherName { get; set; }
        public int EnrollmentCount { get; set; }
        public int StudentCount { get; set; }
    }

    public class CreateClassRequest
    {
        public int CourseId { get; set; }
    }

    public class ClassApiService : BaseApiClient
    {
        public ClassApiService() : base("/api/classes/")
        {
        }

        public async Task<ClassResponse> CreateClassFromPowerPointAsync(int courseId)
        {
            var request = new CreateClassRequest { CourseId = courseId };
            return await PostAsync<CreateClassRequest, ClassResponse>("create-class/", request);
        }

        public async Task<ClassResponse> GetClassAsync(int classId)
        {
            return await GetAsync<ClassResponse>($"{classId}/");
        }

        public async Task<ClassResponse> EndClassAsync(int classId)
        {
            var request = new { active = false };
            return await PatchAsync<object, ClassResponse>($"{classId}/end/", request);
        }
    }
}
