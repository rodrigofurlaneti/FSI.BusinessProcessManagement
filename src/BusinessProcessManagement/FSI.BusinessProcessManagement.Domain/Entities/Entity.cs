namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public abstract class Entity
    {
        public long Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        protected Entity()
        {
            CreatedAt = DateTime.UtcNow;
        }
        protected void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}
