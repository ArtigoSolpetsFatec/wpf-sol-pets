using System.Windows;
using wpf_sol_pets._14TelaIndicadores;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._13MenuFinanceiro
{
    /// <summary>
    /// Lógica interna para MenuFinanceiro.xaml
    /// </summary>
    public partial class MenuFinanceiro : Window
    {
        public readonly LoginViewModel infoLogin = new();
        private readonly FuncionarioViewModel funcionario = new();

        public MenuFinanceiro(LoginViewModel infoLogin, FuncionarioViewModel funcionario)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
            CarregaTextoInicial();
        }

        private void CarregaTextoInicial()
        {
            string[] nomeFuncionario = funcionario.NomeCompleto.Split(' ');
            txtNome.Text = nomeFuncionario[0] + ' ' + nomeFuncionario[^1];
        }

        private void VoltaTelaAnterior(object sender, RoutedEventArgs e)
        {
            var menuAdmin = new MenuAdmin(infoLogin, funcionario);
            menuAdmin.Show();
            Close();
        }

        private void CarregaTelaIndicadoresAno(object sender, RoutedEventArgs e)
        {
            var telaIndicadores = new TelaIndicadores(infoLogin, funcionario, "ANO");
            telaIndicadores.Show();
            Close();
        }

        private void CarregaTelaIndicadoresMes(object sender, RoutedEventArgs e)
        {
            var telaIndicadores = new TelaIndicadores(infoLogin, funcionario, "MES");
            telaIndicadores.Show();
            Close();
        }

        private void CarregaTelaIndicadoresDia(object sender, RoutedEventArgs e)
        {
            var telaIndicadores = new TelaIndicadores(infoLogin, funcionario, "DIA");
            telaIndicadores.Show();
            Close();
        }

        private void CarregarTelaCrudCompra(object sender, RoutedEventArgs e)
        {

        }

        private void CarregarBuscaConta(object sender, RoutedEventArgs e)
        {

        }

        private void CarregarTelaCrudConta(object sender, RoutedEventArgs e)
        {

        }
    }
}
