using ClassPointAddIn.Api.Service.ClassPointAddIn.Api.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Services.CourseService
{
    public class CourseApiService : BaseApiClient
    {
        public CourseApiService() : base("/api/courses/") { }

        public Task<CourseResponse> CreateCourseAsync(string name)
        {
            var payload = new { name };
            return PostAsync<object, CourseResponse>("", payload);
        }

        public Task<List<CourseResponse>> GetAllCoursesAsync()
        {
            return GetAsync<List<CourseResponse>>("");
        }

        public Task<CourseResponse> GetCourseByIdAsync(int id)
        {
            return GetAsync<CourseResponse>($"{id}/");
        }
    }
}
