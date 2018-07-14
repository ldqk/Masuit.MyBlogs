using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entity
{
    [Table("Hangfire.AggregatedCounter")]
    public partial class AggregatedCounter
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public long Value { get; set; }
        public DateTime? ExpireAt { get; set; }
    }
}