using System;
using System.Collections.Generic;

namespace BourbonVault.Core.DTOs
{
    public class BottleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistilleryId { get; set; }
        public string DistilleryName { get; set; }
        public string Type { get; set; }
        public int? AgeYears { get; set; }
        public decimal Proof { get; set; }
        // Back-compat/UI: bottle size in milliliters
        public int? BottleSizeML { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PurchaseLocation { get; set; }
        public decimal? CurrentEstimatedValue { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public int TastingNotesCount { get; set; }
        public double? AverageRating { get; set; }
    }

    public class BottleCreateDto
    {
        public string Name { get; set; }
        public int DistilleryId { get; set; }
        // Back-compat/UI convenience: editable/display name
        public string? DistilleryName { get; set; }
        public string Type { get; set; }
        public int? AgeYears { get; set; }
        public decimal Proof { get; set; }
        // Back-compat/UI: bottle size in milliliters
        public int? BottleSizeML { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? PurchaseLocation { get; set; }
        public decimal? CurrentEstimatedValue { get; set; }
        public string Status { get; set; } = "Unopened";
        public string? Notes { get; set; }
        public string? ImageUrl { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
        // Back-compat/UI: string tags used by UI; API may map to TagIds
        public List<string>? Tags { get; set; }
    }

    public class BottleUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistilleryId { get; set; }
        // Back-compat/UI convenience: editable/display name
        public string? DistilleryName { get; set; }
        public string Type { get; set; }
        public int? AgeYears { get; set; }
        public decimal Proof { get; set; }
        // Back-compat/UI: bottle size in milliliters
        public int? BottleSizeML { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? PurchaseLocation { get; set; }
        public decimal? CurrentEstimatedValue { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public string? ImageUrl { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
        // Back-compat/UI: string tags used by UI; API may map to TagIds
        public List<string>? Tags { get; set; }
    }
}
