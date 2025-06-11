using System.ComponentModel.DataAnnotations;
using Zap.Api.Common.Constants;

namespace Zap.Api.Data.Models;

public class CompanyRole : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
}
