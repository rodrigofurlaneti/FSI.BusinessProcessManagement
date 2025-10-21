using FSI.BusinessProcessManagement.Domain.Exceptions;
namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public abstract class BaseEntity
    {
        public long Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        protected BaseEntity()
        {
            CreatedAt = DateTime.Now;
        }
        protected void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}
