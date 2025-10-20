namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region Screen
    public sealed class Screen : Entity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        private Screen() { }
        public Screen(string name, string? description = null)
        {
            SetName(name);
            Description = description?.Trim();
        }
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Screen name is required.");
            if (name.Length > 150) throw new DomainException("Screen name too long (max 150).");
            Name = name.Trim();
            Touch();
        }
        public void SetDescription(string? d)
        {
            Description = d?.Trim();
            Touch();
        }
    }
    #endregion
}
