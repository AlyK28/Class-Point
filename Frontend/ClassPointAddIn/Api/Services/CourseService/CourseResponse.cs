namespace ClassPointAddIn.Api.Services.CourseService
{
    public class CourseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TeacherDto Teacher { get; set; }
        public string CreatedAt { get; set; }
    }
}