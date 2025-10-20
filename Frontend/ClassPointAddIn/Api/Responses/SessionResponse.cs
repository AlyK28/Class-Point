using System;
using System.Collections.Generic;

namespace ClassPointAddIn.Api.Responses
{
    public class SessionResponse
    {
        public string Id { get; set; }
        public string SessionCode { get; set; }
        public string Name { get; set; }
        public string Question { get; set; }
        public string Teacher { get; set; }
        public string TeacherUsername { get; set; }
        public string Status { get; set; }
        public bool AllowAnonymous { get; set; }
        public int MaxSubmissions { get; set; }
        public int SubmissionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string QrCode { get; set; }
        public string PublicUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
