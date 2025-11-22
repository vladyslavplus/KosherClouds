using System.ComponentModel.DataAnnotations;

namespace KosherClouds.BookingService.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime dateTime)
                return ValidationResult.Success;

            return dateTime > DateTime.UtcNow
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "Booking date must be in the future");
        }
    }
}
