namespace wpf_sol_pets.Models.ViewModels
{
    public class PedidoProdutoViewModel
    {
        public ProdutoViewModel Produto { get; set; }
        public int QtdeProdutos { get; set; }
        public bool Finalizado { get; set; }
        public double? TotalVenda { get; set; }
        public int? IdPedido { get; set; }
    }
}
