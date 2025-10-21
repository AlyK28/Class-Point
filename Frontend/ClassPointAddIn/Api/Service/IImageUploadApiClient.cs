using ClassPointAddIn.Api.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Service
{
    public interface IImageUploadApiClient
    {
        Task<SessionResponse> CreateSessionAsync(string name, string question = "");
        Task<List<SessionResponse>> GetTeacherSessionsAsync();
        Task<SessionResponse> GetSessionAsync(string sessionId);
        Task<SessionResponse> CloseSessionAsync(string sessionId);
        Task<SubmissionResponse> UploadImageAsync(string sessionCode, string imagePath, string studentName = "");
        Task<List<SubmissionResponse>> GetSessionSubmissionsAsync(string sessionCode);
        Task<SubmissionResponse> ToggleLikeAsync(string submissionId);
        Task<bool> DeleteSubmissionAsync(string submissionId);
        Task<byte[]> DownloadSubmissionsAsync(string sessionId);
    }
}
