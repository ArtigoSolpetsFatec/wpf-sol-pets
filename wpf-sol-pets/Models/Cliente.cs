using System;
using System.Collections.Generic;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets.Models
{
    public class Cliente
    {
        public string NomeCliente { get; set; }
        public string CpfCliente { get; set; }
        public string CnpjCliente { get; set; }
        public char SexoCliente { get; set; }
        public string NomeEmpresaCliente { get; set; }
        public string RgCliente { get; set; }
        public string UfRg { get; set; }
        public DateTime DataNascimentoCliente { get; set; }
        public DateTime DHUltimaAtualizacao { get; set; }
        public List<PetViewModel> PetsCliente { get; set; }
        public List<ContatoViewModel> ContatosCliente { get; set; }
        public List<EnderecoViewModel> EnderecosCliente { get; set; }
    }
}
