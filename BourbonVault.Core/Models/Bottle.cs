using System;
using System.Collections.Generic;

namespace BourbonVault.Core.Models
{
    public class Bottle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistilleryId { get; set; }
        public string Type { get; set; } // Bourbon, Rye, etc.
        public int? AgeYears { get; set; }
        public decimal Proof { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? PurchaseLocation { get; set; }
        public decimal? CurrentEstimatedValue { get; set; }
        public string Status { get; set; } // Unopened, Opened, Finished
        public string? Notes { get; set; }
        public string? ImageUrl { get; set; }
        public string UserId { get; set; }
        
        // Navigation properties
        public virtual Distillery Distillery { get; set; }
        public virtual ICollection<TastingNote> TastingNotes { get; set; } = new List<TastingNote>();
        public virtual ICollection<BottleTag> BottleTags { get; set; } = new List<BottleTag>();
    }
}
