using ApiBiblioteca.Exceptions;
using ApiBiblioteca.Models;
using ApiBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBiblioteca.Controllers
{
    [Route("api/livros")]
    [ApiController]
    public class LivrosController : ControllerBase
    {
        private readonly LivroService _livroService;

        public LivrosController(LivroService livroService)
        {
            _livroService = livroService;
        }

        // GET: api/livros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Livro>>> GetLivros()
        {
            return Ok(await _livroService.ObterTodosAsync());
        }

        // GET: api/livros/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Livro>> GetLivroPorId(Guid id)
        {
            try
            {
                var livro = await _livroService.ObterPorIdAsync(id);
                return Ok(livro);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/livros
        [HttpPost]
        public async Task<ActionResult<Livro>> PostLivro(Livro livro)
        {
            try
            {
                var livroCriado = await _livroService.CriarAsync(livro);

                return CreatedAtAction(
                    nameof(GetLivroPorId),
                    new { id = livroCriado.Id },
                    livroCriado);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // PUT: api/livros/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Livro>> PutLivro(Guid id, Livro livro)
        {
            try
            {
                var livroAtualizado = await _livroService.AtualizarAsync(id, livro);
                return Ok(livroAtualizado);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // DELETE: api/livros/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLivro(Guid id)
        {
            try
            {
                await _livroService.RemoverAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
