using System;
using System.Collections.Generic;

namespace BourbonVault.Tests.Integration.DTOs
{
    /// <summary>
    /// DTO for creating a new tasting note
    /// </summary>
    public class TastingNoteCreateDto
    {
        public int BottleId { get; set; }
        public string Notes { get; set; }
        public int Rating { get; set; }
        public string Setting { get; set; }
        public string GlassType { get; set; }
        public string Appearance { get; set; }
        public string Nose { get; set; }
        public string Palate { get; set; }
        public string Finish { get; set; }
        public bool IsPublic { get; set; }
        public DateTime TastingDate { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing tasting note
    /// </summary>
    public class TastingNoteUpdateDto
    {
        public string Notes { get; set; }
        public int Rating { get; set; }
        public string Setting { get; set; }
        public string GlassType { get; set; }
        public string Appearance { get; set; }
        public string Nose { get; set; }
        public string Palate { get; set; }
        public string Finish { get; set; }
        public bool IsPublic { get; set; }
        public DateTime TastingDate { get; set; }
    }

    /// <summary>
    /// DTO for returning tasting note data
    /// </summary>
    public class TastingNoteDto
    {
        public int Id { get; set; }
        public int BottleId { get; set; }
        public string BottleName { get; set; }
        public string Notes { get; set; }
        public int Rating { get; set; }
        public string Setting { get; set; }
        public string GlassType { get; set; }
        public string Appearance { get; set; }
        public string Nose { get; set; }
        public string Palate { get; set; }
        public string Finish { get; set; }
        public bool IsPublic { get; set; }
        public string UserId { get; set; }
        public DateTime TastingDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
