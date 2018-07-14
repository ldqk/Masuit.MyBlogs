using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entity
{
    [Table("Hangfire.Job")]
    public partial class Job
    {
        public int Id { get; set; }
        public Nullable<int> StateId { get; set; }
        public string StateName { get; set; }
        public string InvocationData { get; set; }
        public string Arguments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
    }
}