namespace ApiBiblioteca.Exceptions
{
    // Lançada pelos Services quando um recurso não existe.
    // O Controller captura essa exceção e devolve 404 Not Found.
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
