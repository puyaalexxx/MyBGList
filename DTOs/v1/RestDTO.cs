using System.ComponentModel.DataAnnotations;

namespace MyBGList.DTOs.v1;

public class RestDTO<T>
{
    public T Data { get; set; } = default!;

    public int? PageIndex { get; set; }

    [Range(1, 100, ErrorMessage = "Page count must be between 1 and 100")]
    public int? PageSize { get; set; }

    public int? RecordCount { get; set; }

    public List<LinkDTO> Links { get; set; } = new();
}