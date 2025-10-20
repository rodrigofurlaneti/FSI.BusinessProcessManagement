namespace FSI.BusinessProcessManagement.Domain.ValueObjects
{
    public sealed record PasswordHash
    {
        public string Hash { get; }
        public PasswordHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash)) throw new ArgumentException("Password hash is required.");
            if (hash.Length > 255) throw new ArgumentException("Password hash too long.");
            Hash = hash;
        }
        public override string ToString() => Hash;
    }
}
