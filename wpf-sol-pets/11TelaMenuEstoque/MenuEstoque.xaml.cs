using System.Windows;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._3TelasBusca._3._2BuscarProduto;
using wpf_sol_pets._5TelaCrudProduto;
using wpf_sol_pets._6CrudCategoria;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._11TelaMenuEstoque
{
    /// <summary>
    /// Lógica interna para MenuEstoque.xaml
    /// </summary>
    public partial class MenuEstoque : Window
    {
        private readonly FuncionarioViewModel funcionario;
        private readonly LoginViewModel login;

        public MenuEstoque(LoginViewModel login, FuncionarioViewModel funcionario)
        {
            this.login = login;
            this.funcionario = funcionario;
            InitializeComponent();
        }

        private void AvancaTelaCrudProdutos(object sender, RoutedEventArgs e)
        {
            var telaCrudProduto = new CrudProduto(login, funcionario, "estoque", "cadastrar");
            telaCrudProduto.Show();
            Close();
        }

        private void VoltaTelaAnterior(object sender, RoutedEventArgs e)
        {
            var menuAdmin = new MenuAdmin(login, funcionario);
            menuAdmin.Show();
            Close();
        }

        private void BuscarProduto(object sender, RoutedEventArgs e)
        {
            var telaBuscarProduto = new BuscarProduto(login, funcionario, "MENU-ESTOQUE");
            telaBuscarProduto.Show();
            Close();
        }

        private void CadastrarCategoria(object sender, RoutedEventArgs e)
        {
            var telaCadastroCategoria = new CrudCategoriaProduto("cadastro", "MENU-ESTOQUE", funcionario,
                login);
            telaCadastroCategoria.Show();
            Close();
        }

        private void BuscarCategoria(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidade em desenvolvimento", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
