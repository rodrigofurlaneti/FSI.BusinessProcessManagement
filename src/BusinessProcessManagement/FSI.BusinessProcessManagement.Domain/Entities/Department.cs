using FSI.BusinessProcessManagement.Domain.Exceptions;
namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region Department
    public sealed class Department : BaseEntity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }


        private Department() { }


        public Department(string name, string? description = null)
        {
            SetName(name);
            Description = description?.Trim();
        }


        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Department name is required.");
            if (name.Length > 150)
                throw new DomainException("Department name cannot be longer than 150 characters.");


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
