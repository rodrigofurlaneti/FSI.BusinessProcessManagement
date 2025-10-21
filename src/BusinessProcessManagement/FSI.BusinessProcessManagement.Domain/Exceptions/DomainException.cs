namespace FSI.BusinessProcessManagement.Domain.Exceptions
{
    /// <summary>Exceção de regras de domínio.</summary>
    public class DomainException : System.Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message, System.Exception inner) : base(message, inner) { }
    }
}
