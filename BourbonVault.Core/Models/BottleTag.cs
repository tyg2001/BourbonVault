namespace BourbonVault.Core.Models
{
    public class BottleTag
    {
        public int Id { get; set; }
        public int BottleId { get; set; }
        public int TagId { get; set; }
        
        // Navigation properties
        public virtual Bottle Bottle { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
