using System;
using System.Collections.Generic;

namespace ClassPointAddIn.Api.Responses
{
    public class SubmissionResponse
    {
        public string Id { get; set; }
        public string Session { get; set; }
        public string SessionName { get; set; }
        public string SessionCode { get; set; }
        public string StudentName { get; set; }
        public string Image { get; set; }
        public string ImageUrl { get; set; }
        public string Thumbnail { get; set; }
        public string ThumbnailUrl { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public int Likes { get; set; }
        public bool IsLiked { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
