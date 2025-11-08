using System;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public abstract class BaseEntity
    {
        public long Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }

        protected void Touch()
        {
            var now = DateTime.UtcNow;

            if (UpdatedAt.HasValue && now <= UpdatedAt.Value)
            {
                UpdatedAt = UpdatedAt.Value.AddTicks(1);
            }
            else
            {
                UpdatedAt = now;
            }
        }
    }
}
