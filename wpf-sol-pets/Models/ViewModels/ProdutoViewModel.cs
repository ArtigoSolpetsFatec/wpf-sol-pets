namespace wpf_sol_pets.Models.ViewModels
{
    public class ProdutoViewModel : Produto
    {
        public int IdProduto { get; set; }
        public int? QtdeProduto { get; set; }
        public bool? Selected { get; set; }
    }
}
