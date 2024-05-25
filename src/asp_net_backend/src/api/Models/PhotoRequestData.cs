using System.ComponentModel.DataAnnotations;

namespace Restify.API.Models;

public record PhotoRequestData(
    [Required] string ImageBase64,
    [Required] string ImageFileExtension
);