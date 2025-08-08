namespace DbAutoChat.Win.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;

    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(string errorMessage, string errorCode = "VALIDATION_ERROR")
        => new() { IsValid = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
}