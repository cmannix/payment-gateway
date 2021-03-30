namespace PaymentGateway.Acquirer.InMemory
{
    public record InMemoryAcquirerOptions(AuthoriseBehaviour AuthoriseBehaviour)
    {
        public const string Acquirer = "InMemoryAcquirer";

        public InMemoryAcquirerOptions() : this(AuthoriseBehaviour.Approve) { }
    }

    public enum AuthoriseBehaviour
    {
        Approve,
        Deny,
        Error
    }
}
