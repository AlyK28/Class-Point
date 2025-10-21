using ClassPointAddIn.Api.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Service
{
    public class ImageUploadApiClient : IImageUploadApiClient
    {
        private readonly HttpClient _httpClient;
        private string _authToken;

        public ImageUploadApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:8000/api/image-upload/");
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<SessionResponse> CreateSessionAsync(string name, string question = "")
        {
            var payload = new
            {
                name,
                question,
                allow_anonymous = true,
                max_submissions = 100
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("sessions/", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to create session: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SessionResponse>(json);
        }

        public async Task<List<SessionResponse>> GetTeacherSessionsAsync()
        {
            var response = await _httpClient.GetAsync("teacher/sessions/");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get sessions: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SessionResponse>>(json);
        }

        public async Task<SessionResponse> GetSessionAsync(string sessionId)
        {
            var response = await _httpClient.GetAsync($"sessions/{sessionId}/");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get session: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SessionResponse>(json);
        }

        public async Task<SessionResponse> CloseSessionAsync(string sessionId)
        {
            var response = await _httpClient.PostAsync($"sessions/{sessionId}/close/", null);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to close session: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SessionResponse>(json);
        }

        public async Task<SubmissionResponse> UploadImageAsync(string sessionCode, string imagePath, string studentName = "")
        {
            using (var form = new MultipartFormDataContent())
            {
                // Add image file
                var fileContent = new ByteArrayContent(File.ReadAllBytes(imagePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                form.Add(fileContent, "image", Path.GetFileName(imagePath));

                // Add student name if provided
                if (!string.IsNullOrEmpty(studentName))
                {
                    form.Add(new StringContent(studentName), "student_name");
                }

                var response = await _httpClient.PostAsync($"sessions/{sessionCode}/submissions/", form);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to upload image: {response.StatusCode} - {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SubmissionResponse>(json);
            }
        }

        public async Task<List<SubmissionResponse>> GetSessionSubmissionsAsync(string sessionCode)
        {
            var response = await _httpClient.GetAsync($"sessions/{sessionCode}/submissions/");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get submissions: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SubmissionResponse>>(json);
        }

        public async Task<SubmissionResponse> ToggleLikeAsync(string submissionId)
        {
            var response = await _httpClient.PostAsync($"submissions/{submissionId}/like/", null);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to toggle like: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SubmissionResponse>(json);
        }

        public async Task<bool> DeleteSubmissionAsync(string submissionId)
        {
            var response = await _httpClient.PostAsync($"submissions/{submissionId}/delete/", null);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to delete submission: {response.StatusCode}");

            return response.IsSuccessStatusCode;
        }

        public async Task<byte[]> DownloadSubmissionsAsync(string sessionId)
        {
            var payload = new
            {
                include_metadata = true,
                zip_filename = $"session_{sessionId}_submissions.zip"
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"sessions/{sessionId}/download/", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to download submissions: {response.StatusCode}");

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
