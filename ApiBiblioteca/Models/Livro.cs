using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiBiblioteca.Models
{
    public class Livro
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter até 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O autor é obrigatório.")]
        [StringLength(150, ErrorMessage = "O autor deve ter até 150 caracteres.")]
        public string Autor { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ISBN é obrigatório.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "O ISBN deve ter exatamente 13 caracteres.")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "O ISBN deve conter apenas números e hífens.")]
        public string Isbn { get; set; } = string.Empty;

        [Range(1440, 2200, ErrorMessage = "Informe um ano de publicação válido.")]
        public int AnoPublicacao { get; set; }

        [Required(ErrorMessage = "A editora é obrigatória.")]
        [StringLength(150, ErrorMessage = "A editora deve ter até 150 caracteres.")]
        public string Editora { get; set; } = string.Empty;

        // Controlado pelo Service, não pelo cliente da API.
        public bool Disponivel { get; set; } = true;

        [JsonIgnore]
        public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    }
}
