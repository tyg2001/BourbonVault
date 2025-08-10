using System;
using System.Collections.Generic;

namespace BourbonVault.Tests.Integration.DTOs
{
    /// <summary>
    /// DTO for creating a new bottle
    /// </summary>
    public class BottleCreateDto
    {
        public string Name { get; set; }
        public int DistilleryId { get; set; } = 1; // Default for tests
        public string Type { get; set; }
        public int? AgeYears { get; set; }
        public decimal Proof { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now; // Default for tests
        public string PurchaseLocation { get; set; } = "Test Location"; // Default for tests
        public decimal? CurrentEstimatedValue { get; set; }
        public string Status { get; set; } = "Unopened";
        public string Notes { get; set; }
        public string ImageUrl { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// DTO for updating an existing bottle
    /// </summary>
    public class BottleUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistilleryId { get; set; }
        public string Type { get; set; }
        public int? AgeYears { get; set; }
        public decimal Proof { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PurchaseLocation { get; set; }
        public decimal? CurrentEstimatedValue { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string ImageUrl { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// DTO for returning bottle data
    /// </summary>
    public class BottleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistilleryId { get; set; }
        public string DistilleryName { get; set; }
        public string Type { get; set; }
        public int? AgeYears { get; set; }
        public decimal Proof { get; set; }
        public decimal PurchasePrice { get; set; }
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
}
