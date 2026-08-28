using ApiBiblioteca.Data;
using ApiBiblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiBiblioteca.Repositories
{
    // Única classe que conversa diretamente com o AppDbContext para a entidade Emprestimo.
    // Não tem nenhuma regra de negócio aqui: só operações de acesso ao banco.
    public class EmprestimoRepository
    {
        private readonly AppDbContext _context;

        public EmprestimoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Emprestimo>> ObterTodosAsync()
        {
            return await _context.Emprestimos
                .Include(e => e.Livro)
                .AsNoTracking()
                .OrderByDescending(e => e.DataEmprestimo)
                .ToListAsync();
        }

        public async Task<Emprestimo?> ObterPorIdAsync(Guid id)
        {
            return await _context.Emprestimos
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Emprestimo> AdicionarAsync(Emprestimo emprestimo)
        {
            _context.Emprestimos.Add(emprestimo);
            await _context.SaveChangesAsync();
            return emprestimo;
        }

        public async Task AtualizarAsync(Emprestimo emprestimo)
        {
            _context.Emprestimos.Update(emprestimo);
            await _context.SaveChangesAsync();
        }
    }
}
