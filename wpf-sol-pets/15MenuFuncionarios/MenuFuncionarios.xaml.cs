using System.Windows;
using wpf_sol_pets._10TelaCrudCargo;
using wpf_sol_pets._16CrudFuncionario;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._3TelasBusca._3._6BuscarCargo;
using wpf_sol_pets._3TelasBusca._3._7BuscarFuncionario;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._15MenuFuncionarios
{
    /// <summary>
    /// Lógica interna para MenuFuncionarios.xaml
    /// </summary>
    public partial class MenuFuncionarios : Window
    {
        private readonly LoginViewModel infoLogin = new();
        private readonly FuncionarioViewModel funcionario = new();

        public MenuFuncionarios(LoginViewModel infoLogin, FuncionarioViewModel funcionario)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
        }

        private void CadastrarFuncionario(object sender, RoutedEventArgs e)
        {
            var crudFuncionarios = new CrudFuncionario(infoLogin, funcionario, "CADASTRO");
            crudFuncionarios.Show();
            Close();
        }

        private void BuscarFuncionario(object sender, RoutedEventArgs e)
        {
            var buscarFuncionario = new BuscarFuncionario(infoLogin, funcionario);
            buscarFuncionario.Show();
            Close();
        }

        private void CadastrarCargo(object sender, RoutedEventArgs e)
        {
            var cadastroCargo = new CrudCargo(infoLogin, funcionario, "CADASTRO");
            cadastroCargo.Show();
            Close();
        }

        private void BuscarCargo(object sender, RoutedEventArgs e)
        {
            var buscaCargo = new BuscarCargo(infoLogin, funcionario);
            buscaCargo.Show();
            Close();
        }

        private void VoltarTelaAnterior(object sender, RoutedEventArgs e)
        {
            var menuAdmin = new MenuAdmin(infoLogin, funcionario);
            menuAdmin.Show();
            Close();
        }
    }
}
