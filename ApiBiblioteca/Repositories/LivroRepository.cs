using ApiBiblioteca.Data;
using ApiBiblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiBiblioteca.Repositories
{
    // Única classe que conversa diretamente com o AppDbContext para a entidade Livro.
    // Não tem nenhuma regra de negócio aqui: só operações de acesso ao banco.
    public class LivroRepository
    {
        private readonly AppDbContext _context;

        public LivroRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Livro>> ObterTodosAsync()
        {
            return await _context.Livros
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Livro?> ObterPorIdAsync(Guid id)
        {
            return await _context.Livros
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Livro?> ObterPorIsbnAsync(string isbn)
        {
            return await _context.Livros
                .FirstOrDefaultAsync(l => l.Isbn == isbn);
        }

        public async Task<Livro> AdicionarAsync(Livro livro)
        {
            _context.Livros.Add(livro);
            await _context.SaveChangesAsync();
            return livro;
        }

        public async Task AtualizarAsync(Livro livro)
        {
            _context.Livros.Update(livro);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Livro livro)
        {
            _context.Livros.Remove(livro);
            await _context.SaveChangesAsync();
        }
    }
}
