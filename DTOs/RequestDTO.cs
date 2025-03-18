using System.ComponentModel.DataAnnotations;
using MyBGList.Attributes;
using DefaultValueAttribute = System.ComponentModel.DefaultValueAttribute;

namespace MyBGList.DTOs;

public class RequestDTO<T> : IValidatableObject
{
    [DefaultValue(0)] public int PageIndex { get; set; } = 0;

    [DefaultValue(10)] [Range(1, 100)] public int PageSize { get; set; } = 10;

    [DefaultValue("Name")] public string? SortColumn { get; set; } = "Name";

    [DefaultValue("ASC")]
    [SortOrderValidator]
    public string? SortOrder { get; set; } = "ASC";

    [DefaultValue(null)] public string? FilterQuery { get; set; } = null;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validator = new SortColumnValidatorAttribute(typeof(T));

        var result = validator.GetValidationResult(SortColumn, validationContext);

        return result != null ? new[] { result } : Array.Empty<ValidationResult>();
    }
}