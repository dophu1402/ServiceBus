using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Payment.Domain.Common
{
    [Index(nameof(CreatedTime))]
    [Index(nameof(UpdatedTime))]
    [Index(nameof(CreatedBy))]
    [Index(nameof(UpdatedBy))]
    public class BaseEntity
    {
        [Column(TypeName = "datetime2")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }

        [Column(TypeName = "datetime2")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime UpdatedTime { get; set; }

        [Column(TypeName = "datetime2")]
        public string? CreatedBy { get; set; }

        [Column(TypeName = "datetime2")]
        public string? UpdatedBy { get; set; }
    }
}
