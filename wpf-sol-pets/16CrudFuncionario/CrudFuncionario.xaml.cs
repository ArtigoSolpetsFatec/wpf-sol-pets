using System.Windows;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._16CrudFuncionario
{
    /// <summary>
    /// Lógica interna para CrudFuncionario.xaml
    /// </summary>
    public partial class CrudFuncionario : Window
    {
        private readonly LoginViewModel infoLogin = new();
        private readonly FuncionarioViewModel funcionario = new();
        private readonly FuncionarioViewModel funcionarioEdicao;
        private readonly string tipoTela;

        public CrudFuncionario(LoginViewModel infoLogin, FuncionarioViewModel funcionario, string tipoTela,
            FuncionarioViewModel funcionarioEdicao = null)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
            this.tipoTela = tipoTela;
            this.funcionarioEdicao = funcionarioEdicao;
        }
    }
}
