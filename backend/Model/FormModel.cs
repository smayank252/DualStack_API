using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FormApp.Models
{
    public class FormModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;  // Default empty string

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty; // Default empty string

        public string? Address { get; set; } // Optional field (nullable)

        [Required]
        public IFormFile Photo { get; set; } = null!; // Initialized as null
    }
}

