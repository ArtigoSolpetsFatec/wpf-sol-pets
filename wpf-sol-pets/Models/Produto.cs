using System;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets.Models
{
    public class Produto : CategoriaViewModel
    {
        public string IsbnProduto { get; set; }
        public string NomeProduto { get; set; }
        public string MarcaProduto { get; set; }
        public double ValorUnitarioCusto { get; set; }
        public double ValorUnitarioVenda { get; set; }
        public int QtdeEstoque { get; set; }
        public FornecedorViewModel Fornecedor { get; set; }
        public double PesoProduto { get; set; }
        public DateTime DhUltimaAtualizacao { get; set; }
        public DateTime DataValidade { get; set; }
        public int IdadeAplicavel { get; set; }
        public double PesoAplicavel { get; set; }
        public string FotoProduto { get; set; }
        public bool ValorVendaEditavel { get; set; }
        public double ValorProdutoPedido { get; set; }

    }
}
