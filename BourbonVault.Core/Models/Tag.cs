using System.Collections.Generic;

namespace BourbonVault.Core.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        
        // Navigation properties
        public virtual ICollection<BottleTag> BottleTags { get; set; } = new List<BottleTag>();
    }
}
