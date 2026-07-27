using System;

namespace ChatApp.Shared.DTOs.Users
{
    public class KarmaUpdateDto
    {
        public Guid UserId { get; set; }
        public int AddedPoints { get; set; }
        public int TotalKarmaPoints { get; set; }
        public string? NewTitle { get; set; }
    }
}
