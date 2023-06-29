using System;
using System.Collections.Generic;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets.Models
{
    public class Pedido : PedidoViewModel
    {
        public DateTime DhVenda { get; set; }
        public char StatusVenda { get; set; }
        public bool Entregar { get; set; }
        public ClienteViewModel Cliente { get; set; }
        public double TotalVenda { get; set; }
        public int QtdeProdutos { get; set; }
        public bool Finalizado { get; set; }
        public List<ProdutoViewModel> Produtos { get; set; }
        public List<PedidoProdutoViewModel> Pedidos { get; set; }
        public double ValorDesconto { get; set; }
    }
}
