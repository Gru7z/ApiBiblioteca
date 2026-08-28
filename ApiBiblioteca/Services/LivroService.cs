using ApiBiblioteca.Exceptions;
using ApiBiblioteca.Models;
using ApiBiblioteca.Repositories;

namespace ApiBiblioteca.Services
{
    // Concentra as regras de negócio de Livro.
    // Nunca acessa o AppDbContext diretamente: toda persistência passa pelo LivroRepository.
    public class LivroService
    {
        private readonly LivroRepository _livroRepository;

        public LivroService(LivroRepository livroRepository)
        {
            _livroRepository = livroRepository;
        }

        public async Task<IEnumerable<Livro>> ObterTodosAsync()
        {
            return await _livroRepository.ObterTodosAsync();
        }

        public async Task<Livro> ObterPorIdAsync(Guid id)
        {
            var livro = await _livroRepository.ObterPorIdAsync(id);

            if (livro is null)
            {
                throw new NotFoundException($"Livro com Id '{id}' não foi encontrado.");
            }

            return livro;
        }

        public async Task<Livro> CriarAsync(Livro livro)
        {
            // Regra de negócio 1: ISBN não pode ser duplicado.
            var livroComMesmoIsbn = await _livroRepository.ObterPorIsbnAsync(livro.Isbn);
            if (livroComMesmoIsbn is not null)
            {
                throw new ConflictException($"Já existe um livro cadastrado com o ISBN '{livro.Isbn}'.");
            }

            livro.Id = Guid.NewGuid();
            livro.Disponivel = true; // todo livro novo nasce disponível

            return await _livroRepository.AdicionarAsync(livro);
        }

        public async Task<Livro> AtualizarAsync(Guid id, Livro livro)
        {
            var livroExistente = await _livroRepository.ObterPorIdAsync(id);

            if (livroExistente is null)
            {
                throw new NotFoundException($"Livro com Id '{id}' não foi encontrado.");
            }

            // Regra de negócio 1: ao trocar o ISBN, garantir que ele não pertença a outro livro.
            var livroComMesmoIsbn = await _livroRepository.ObterPorIsbnAsync(livro.Isbn);
            if (livroComMesmoIsbn is not null && livroComMesmoIsbn.Id != id)
            {
                throw new ConflictException($"Já existe um livro cadastrado com o ISBN '{livro.Isbn}'.");
            }

            livroExistente.Titulo = livro.Titulo;
            livroExistente.Autor = livro.Autor;
            livroExistente.Isbn = livro.Isbn;
            livroExistente.AnoPublicacao = livro.AnoPublicacao;
            livroExistente.Editora = livro.Editora;
            // Disponivel não é alterado por aqui: só muda através do fluxo de Empréstimo/Devolução.

            await _livroRepository.AtualizarAsync(livroExistente);

            return livroExistente;
        }

        public async Task RemoverAsync(Guid id)
        {
            var livro = await _livroRepository.ObterPorIdAsync(id);

            if (livro is null)
            {
                throw new NotFoundException($"Livro com Id '{id}' não foi encontrado.");
            }

            await _livroRepository.RemoverAsync(livro);
        }
    }
}
