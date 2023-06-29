using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wpf_sol_pets._11TelaMenuEstoque;
using wpf_sol_pets._13MenuFinanceiro;
using wpf_sol_pets._15MenuFuncionarios;
using wpf_sol_pets._2TelaAdministrativa._2._1ModalProdutosVencendo;
using wpf_sol_pets._3TelasBusca._3._1BuscarCliente;
using wpf_sol_pets._3TelasBusca._3._2BuscarProduto;
using wpf_sol_pets._3TelasBusca._3._3BuscarFornecedor;
using wpf_sol_pets._3TelasBusca._3._4BuscarPet;
using wpf_sol_pets._7TelaInicioVenda;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._2TelaAdministrativa
{
    /// <summary>
    /// Lógica interna para MenuAdmin.xaml
    /// </summary>
    public partial class MenuAdmin : Window
    {
        public readonly LoginViewModel infoLogin = new();
        private FuncionarioViewModel funcionario = new();

        public MenuAdmin(LoginViewModel infoLogin, FuncionarioViewModel funcionario, bool buscarProdutosAVencer = false)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
            ValidaLoginFuncionario();
            CarregaTextoInicial();
            if (buscarProdutosAVencer)
                BuscarProdutosAVencer();
        }

        private async void BuscarProdutosAVencer()
        {
            var result = new List<ProdutoViewModel>();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = "/produtos/produtos-vencimento";
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<List<ProdutoViewModel>>(responseJson);
                    if (result.Count > 0)
                    {
                        var modalListaProdutos = new ModalProdutosAVencer(infoLogin, funcionario, result);
                        modalListaProdutos.Owner = this;
                        modalListaProdutos.ShowDialog();
                        Close();
                    }
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    BuscarProdutosAVencer();
                }
                if (response.StatusCode == HttpStatusCode.PreconditionFailed
                    || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var msgError = await response.Content.ReadAsStringAsync();
                    throw new Exception(msgError);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidaLoginFuncionario()
        {
            try
            {
                if (infoLogin.IdLogin > 0)
                {
                    DateTime ultimaAtualizacaoSenha = Convert.ToDateTime(infoLogin.DhUltimaAtualizacao);
                    var diasUltAtualizacao = (int)ultimaAtualizacaoSenha.Subtract(DateTime.Today).TotalDays;
                    if (diasUltAtualizacao > 30)
                    {
                        MessageBox.Show($"Sua senha não é atualizada a {diasUltAtualizacao} dias. Aconselhável alterá-la", "Informação Senha",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async void CarregaTextoInicial()
        {
            string hojeFormatBr = $"{DateTime.Now:dd/MM/yyyy HH:mm}";
            txtDataHora.Text = hojeFormatBr;
            if (funcionario.IdFuncionario == 0)
                funcionario = await GetFuncionarioByIdLogin();
            string[] nomeFuncionario = funcionario.NomeCompleto.Split(' ');
            txtNome.Text = nomeFuncionario[0] + ' ' + nomeFuncionario[^1];
        }

        private async Task<FuncionarioViewModel> GetFuncionarioByIdLogin()
        {
            FuncionarioViewModel funcionario = new();
            var result = new FuncionarioViewModel();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = $"/funcionario/idLogin/{infoLogin.IdLogin}";
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<FuncionarioViewModel>(responseJson);
                    if (result.IdFuncionario > 0)
                    {
                        funcionario = result;
                    }
                }
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    MessageBox.Show("Funcionário não encontrado!", "Informação Funcionário", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    result = await GetFuncionarioByIdLogin();
                }
                if (response.StatusCode == HttpStatusCode.PreconditionFailed
                    || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var msgError = await response.Content.ReadAsStringAsync();
                    throw new Exception(msgError);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return funcionario;
        }

        private void BuscarCliente(object sender, RoutedEventArgs e)
        {
            var telaBuscaClie = new BuscarCliente(infoLogin, funcionario, "MENU-ADMIN");
            telaBuscaClie.Show();
            Close();
        }

        private void IniciaVenda(object sender, RoutedEventArgs e)
        {
            var telaPedido = new CrudPedido(infoLogin, funcionario, "MENU-ADMIN");
            telaPedido.Show();
            Close();
        }

        private void AvancaMenuEstoque(object sender, RoutedEventArgs e)
        {
            if (funcionario.IdFuncionario > 0)
            {
                var menuEstoque = new MenuEstoque(infoLogin, funcionario);
                menuEstoque.Show();
                Close();
            }
        }

        private void CarregaTelaBuscarProduto(object sender, RoutedEventArgs e)
        {
            var telaBuscarProduto = new BuscarProduto(infoLogin, funcionario, "MENU-ADMIN");
            telaBuscarProduto.Show();
            Close();
        }

        private void BuscarFornecedor(object sender, RoutedEventArgs e)
        {
            var telaBuscaFornecedor = new BuscarFornecedor(infoLogin, funcionario, "MENU-ADMIN");
            telaBuscaFornecedor.Show();
            Close();
        }

        private void CarregaMenuFinancas(object sender, RoutedEventArgs e)
        {
            var menuFinancas = new MenuFinanceiro(infoLogin, funcionario);
            menuFinancas.Show();
            Close();
        }

        private void BuscarPet(object sender, RoutedEventArgs e)
        {
            var telaBuscarPet = new BuscarPet(infoLogin, funcionario, "MENU-ADMIN");
            telaBuscarPet.Show();
            Close();
        }

        private void MenuFuncionarios(object sender, RoutedEventArgs e)
        {
            var menuFuncionarios = new MenuFuncionarios(infoLogin, funcionario);
            menuFuncionarios.Show();
            Close();
        }
    }
}
