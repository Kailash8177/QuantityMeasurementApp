using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantityMeasurementModelLayer.Entities
{
    [Table("refresh_tokens")]
    public class RefreshTokenEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Required]
        [Column("token")]
        public string Token { get; set; } = string.Empty;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by_ip")]
        [MaxLength(50)]
        public string? CreatedByIp { get; set; }

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        [Column("revoked_by_ip")]
        [MaxLength(50)]
        public string? RevokedByIp { get; set; }

        [Column("is_active")]
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
