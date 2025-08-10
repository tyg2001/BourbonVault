using System.Collections.Generic;

namespace BourbonVault.Core.Models
{
    public class Distillery
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Region { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int? YearFounded { get; set; }
        
        // Navigation properties
        public virtual ICollection<Bottle> Bottles { get; set; } = new List<Bottle>();
    }
}
