namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region Role


    public sealed class Role : Entity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }


        private Role() { }


        public Role(string name, string? description = null)
        {
            SetName(name);
            Description = description?.Trim();
        }


        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Role name is required.");
            if (name.Length > 100)
                throw new DomainException("Role name cannot be longer than 100 characters.");


            Name = name.Trim();
            Touch();
        }


        public void SetDescription(string? description)
        {
            Description = description?.Trim();
            Touch();
        }
    }


    #endregion
}
