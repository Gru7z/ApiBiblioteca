using ApiBiblioteca.Exceptions;
using ApiBiblioteca.Models;
using ApiBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBiblioteca.Controllers
{
    [Route("api/emprestimos")]
    [ApiController]
    public class EmprestimosController : ControllerBase
    {
        private readonly EmprestimoService _emprestimoService; //Service que contém as regras de negócio de empréstimo.

        public EmprestimosController(EmprestimoService emprestimoService) //Injeção de dependência do service no controller
        {
            _emprestimoService = emprestimoService;
        } //Liga o controller no service

        // GET: api/emprestimos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Emprestimo>>> GetEmprestimos() //Retorna todos os empréstimos
        {
            return Ok(await _emprestimoService.ObterTodosAsync());
        }

        // GET: api/emprestimos/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Emprestimo>> GetEmprestimoPorId(Guid id)
        {
            try
            {
                var emprestimo = await _emprestimoService.ObterPorIdAsync(id);
                return Ok(emprestimo);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/emprestimos
        [HttpPost]
        public async Task<ActionResult<Emprestimo>> PostEmprestimo(Emprestimo emprestimo)
        {
            try
            {
                var emprestimoCriado = await _emprestimoService.CriarAsync(emprestimo);

                return CreatedAtAction(
                    nameof(GetEmprestimoPorId),
                    new { id = emprestimoCriado.Id },
                    emprestimoCriado);
            }
            catch (NotFoundException ex)
            {
                // Regra 2: LivroId inexistente.
                return NotFound(ex.Message);
            }
            catch (ConflictException ex)
            {
                // Regra 3: livro indisponível.
                return Conflict(ex.Message);
            }
        }

        // PUT: api/emprestimos/{id}/devolver
        [HttpPut("{id:guid}/devolver")]
        public async Task<ActionResult<Emprestimo>> DevolverEmprestimo(Guid id)
        {
            try
            {
                var emprestimoDevolvido = await _emprestimoService.DevolverAsync(id);
                return Ok(emprestimoDevolvido);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ConflictException ex)
            {
                // Regra 5: empréstimo já devolvido.
                return Conflict(ex.Message);
            }
        }
    }
}
