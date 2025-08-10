using System;

namespace BourbonVault.Core.Models
{
    public class TastingNote
    {
        public int Id { get; set; }
        public int BottleId { get; set; }
        public string UserId { get; set; }
        public DateTime TastingDate { get; set; }
        
        // Rating properties (1-10 scale)
        public int AppearanceRating { get; set; }
        public int NoseRating { get; set; }
        public int TasteRating { get; set; }
        public int FinishRating { get; set; }
        public int OverallRating { get; set; }
        
        // Descriptive notes
        public string AppearanceNotes { get; set; }
        public string NoseNotes { get; set; }
        public string TasteNotes { get; set; }
        public string FinishNotes { get; set; }
        public string AdditionalNotes { get; set; }
        
        // Settings
        public bool IsPublic { get; set; }
        public string? ImageUrl { get; set; }
        
        // Navigation properties
        public virtual Bottle Bottle { get; set; }
    }
}
