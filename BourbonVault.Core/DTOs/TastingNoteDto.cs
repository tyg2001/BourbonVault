using System;

namespace BourbonVault.Core.DTOs
{
    public class TastingNoteDto
    {
        public int Id { get; set; }
        public int BottleId { get; set; }
        public string BottleName { get; set; }
        public string DistilleryName { get; set; }
        public DateTime TastingDate { get; set; }
        // UI metadata
        public string? Setting { get; set; }
        public string? GlassType { get; set; }
        public int? RestTimeMinutes { get; set; }
        
        // Rating properties
        public int AppearanceRating { get; set; }
        public int NoseRating { get; set; }
        public int TasteRating { get; set; }
        public int FinishRating { get; set; }
        public int OverallRating { get; set; }
        // Back-compat: many UI pages refer to a single Rating
        public int Rating => OverallRating;
        
        // Descriptive notes
        public string AppearanceNotes { get; set; }
        public string NoseNotes { get; set; }
        public string TasteNotes { get; set; }
        public string FinishNotes { get; set; }
        public string AdditionalNotes { get; set; }
        // Back-compat aliases used by older UI
        public string? Appearance => AppearanceNotes;
        public string? Nose => NoseNotes;
        public string? Palate => TasteNotes;
        public string? Finish => FinishNotes;
        
        public bool IsPublic { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class TastingNoteCreateDto
    {
        public int BottleId { get; set; }
        public DateTime TastingDate { get; set; } = DateTime.Now;
        // UI metadata
        public string? Setting { get; set; }
        public string? GlassType { get; set; }
        public int? RestTimeMinutes { get; set; }
        
        // Rating properties
        public int AppearanceRating { get; set; }
        public int NoseRating { get; set; }
        public int TasteRating { get; set; }
        public int FinishRating { get; set; }
        public int OverallRating { get; set; }
        // Back-compat/UI alias for OverallRating
        public int Rating { get => OverallRating; set => OverallRating = value; }
        
        // Descriptive notes
        public string AppearanceNotes { get; set; }
        public string NoseNotes { get; set; }
        public string TasteNotes { get; set; }
        public string FinishNotes { get; set; }
        public string AdditionalNotes { get; set; }
        // Back-compat aliases used by older UI
        public string? Appearance { get => AppearanceNotes; set => AppearanceNotes = value; }
        public string? Nose { get => NoseNotes; set => NoseNotes = value; }
        public string? Palate { get => TasteNotes; set => TasteNotes = value; }
        public string? Finish { get => FinishNotes; set => FinishNotes = value; }
        
        public bool IsPublic { get; set; } = false;
        public string? ImageUrl { get; set; }
    }

    public class TastingNoteUpdateDto
    {
        public int Id { get; set; }
        public int BottleId { get; set; }
        public DateTime TastingDate { get; set; }
        // UI metadata
        public string? Setting { get; set; }
        public string? GlassType { get; set; }
        public int? RestTimeMinutes { get; set; }
        
        // Rating properties
        public int AppearanceRating { get; set; }
        public int NoseRating { get; set; }
        public int TasteRating { get; set; }
        public int FinishRating { get; set; }
        public int OverallRating { get; set; }
        // Back-compat/UI alias for OverallRating
        public int Rating { get => OverallRating; set => OverallRating = value; }
        
        // Descriptive notes
        public string AppearanceNotes { get; set; }
        public string NoseNotes { get; set; }
        public string TasteNotes { get; set; }
        public string FinishNotes { get; set; }
        public string AdditionalNotes { get; set; }
        // Back-compat aliases used by older UI
        public string? Appearance { get => AppearanceNotes; set => AppearanceNotes = value; }
        public string? Nose { get => NoseNotes; set => NoseNotes = value; }
        public string? Palate { get => TasteNotes; set => TasteNotes = value; }
        public string? Finish { get => FinishNotes; set => FinishNotes = value; }
        
        public bool IsPublic { get; set; }
        public string? ImageUrl { get; set; }
    }
}
