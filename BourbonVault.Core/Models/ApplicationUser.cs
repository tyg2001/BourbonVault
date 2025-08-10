using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BourbonVault.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Bio { get; set; }
        
        // Navigation properties
        public virtual ICollection<Bottle> Bottles { get; set; }
        public virtual ICollection<TastingNote> TastingNotes { get; set; }
        
        public ApplicationUser()
        {
            Bottles = new HashSet<Bottle>();
            TastingNotes = new HashSet<TastingNote>();
        }
    }
}
