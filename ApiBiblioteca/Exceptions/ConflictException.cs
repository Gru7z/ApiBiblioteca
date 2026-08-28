namespace ApiBiblioteca.Exceptions
{
    // Lançada pelos Services quando uma regra de negócio impede a operação.
    // O Controller captura essa exceção e devolve 409 Conflict.
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
