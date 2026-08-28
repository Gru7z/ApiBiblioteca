using ApiBiblioteca.Exceptions;
using ApiBiblioteca.Models;
using ApiBiblioteca.Repositories;

namespace ApiBiblioteca.Services
{
    // Concentra as regras de negócio de Empréstimo.
    // Nunca acessa o AppDbContext diretamente: toda persistência passa pelos Repositories.
    public class EmprestimoService
    {
        private readonly EmprestimoRepository _emprestimoRepository;
        private readonly LivroRepository _livroRepository;

        public EmprestimoService(
            EmprestimoRepository emprestimoRepository,
            LivroRepository livroRepository)
        {
            _emprestimoRepository = emprestimoRepository;
            _livroRepository = livroRepository;
        }

        public async Task<IEnumerable<Emprestimo>> ObterTodosAsync()
        {
            return await _emprestimoRepository.ObterTodosAsync();
        }

        public async Task<Emprestimo> ObterPorIdAsync(Guid id)
        {
            var emprestimo = await _emprestimoRepository.ObterPorIdAsync(id);

            if (emprestimo is null)
            {
                throw new NotFoundException($"Empréstimo com Id '{id}' não foi encontrado.");
            }

            return emprestimo;
        }

        public async Task<Emprestimo> CriarAsync(Emprestimo emprestimo)
        {
            // Regra de negócio 2: o livro precisa existir.
            var livro = await _livroRepository.ObterPorIdAsync(emprestimo.LivroId);
            if (livro is null)
            {
                throw new NotFoundException($"Livro com Id '{emprestimo.LivroId}' não foi encontrado.");
            }

            // Regra de negócio 3: o livro precisa estar disponível.
            if (!livro.Disponivel)
            {
                throw new ConflictException($"O livro '{livro.Titulo}' já está emprestado e não está disponível no momento.");
            }

            emprestimo.Id = Guid.NewGuid();
            emprestimo.DataEmprestimo = DateTime.UtcNow;
            emprestimo.DataDevolucao = null;

            await _emprestimoRepository.AdicionarAsync(emprestimo);

            // Ao emprestar, o livro deixa de estar disponível.
            livro.Disponivel = false;
            await _livroRepository.AtualizarAsync(livro);

            return emprestimo;
        }

        public async Task<Emprestimo> DevolverAsync(Guid id)
        {
            var emprestimo = await _emprestimoRepository.ObterPorIdAsync(id);

            if (emprestimo is null)
            {
                throw new NotFoundException($"Empréstimo com Id '{id}' não foi encontrado.");
            }

            // Regra de negócio 5: não pode devolver duas vezes o mesmo empréstimo.
            if (emprestimo.DataDevolucao is not null)
            {
                throw new ConflictException($"O empréstimo com Id '{id}' já foi devolvido anteriormente.");
            }

            // Regra de negócio 4: devolução.
            emprestimo.DataDevolucao = DateTime.UtcNow;
            await _emprestimoRepository.AtualizarAsync(emprestimo);

            var livro = await _livroRepository.ObterPorIdAsync(emprestimo.LivroId);
            if (livro is not null)
            {
                livro.Disponivel = true;
                await _livroRepository.AtualizarAsync(livro);
            }

            return emprestimo;
        }
    }
}
