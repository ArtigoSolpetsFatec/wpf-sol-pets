using System;

namespace wpf_sol_pets.Models.ViewModels
{
    public class FuncionarioViewModel : CargoViewModel
    {
        public int IdFuncionario { get; set; }
        public string NomeCompleto { get; set; }
        public string Cpf { get; set; }
        public string Rg { get; set; }
        public DateTime DhInicio { get; set; }
        public DateTime DhNascimento { get; set; }
        public int QtdeDependentes { get; set; }
        public LoginViewModel Login { get; set; }
        public DateTime? DhUltimaAtualizacaoFunc { get; set; }
        public DateTime? DhInativo { get; set; }
    }
}
