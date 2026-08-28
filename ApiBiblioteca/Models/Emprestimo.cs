using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiBiblioteca.Models
{
    public class Emprestimo
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O LivroId é obrigatório.")]
        public Guid LivroId { get; set; }

        [JsonIgnore]
        public Livro? Livro { get; set; }

        [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome do usuário deve ter até 150 caracteres.")]
        public string NomeUsuario { get; set; } = string.Empty;

        // Preenchido automaticamente pelo Service no momento do empréstimo.
        public DateTime DataEmprestimo { get; set; }

        // Nulo enquanto o livro não for devolvido.
        public DateTime? DataDevolucao { get; set; }
    }
}
