namespace FSI.BusinessProcessManagement.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Address { get; }
        public Email(string address)
        {
            if (string.IsNullOrWhiteSpace(address) || !address.Contains('@'))
                throw new ArgumentException("E-mail inválido.");
            Address = address.Trim().ToLowerInvariant();
        }
        public override string ToString() => Address;
    }
}
