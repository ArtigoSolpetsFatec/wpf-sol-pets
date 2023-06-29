using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wpf_sol_pets._12TelaCrudFornecedor;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._3TelasBusca._3._3BuscarFornecedor
{
    /// <summary>
    /// Lógica interna para BuscarFornecedor.xaml
    /// </summary>
    public partial class BuscarFornecedor : Window
    {
        public readonly LoginViewModel login = new();
        private FuncionarioViewModel funcionario = new();
        private readonly string telaAnterior;

        public BuscarFornecedor(LoginViewModel login, FuncionarioViewModel funcionario, string telaAnterior)
        {
            InitializeComponent();
            this.telaAnterior = telaAnterior;
            this.login = login;
            this.funcionario = funcionario;
        }

        private async void BuscarFornecedorBy(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = CNPJ.IsSelected ? $"/fornecedor/{txtCampo.Text}/cnpj" : $"/fornecedor/{txtCampo.Text}/nome";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResult(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<FornecedorViewModel>> TratarResult(HttpResponseMessage response)
        {
            var result = new List<FornecedorViewModel>();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<List<FornecedorViewModel>>(responseJson);
                    var responseUsua = MessageBox.Show("Fornecedor encontrado! \nDeseja visualizar e editar seu cadastro?",
                      "Informação fornecedor", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (responseUsua.Equals(MessageBoxResult.Yes))
                    {
                        var telaCrudCliente = new CrudFornecedor(login, "editar", "BUSCA-FORNECEDOR", funcionario, null, result[0]);
                        telaCrudCliente.Show();
                        Close();
                    }
                    else
                    {
                        var excluiCadastro = MessageBox.Show("Fornecedor encontrado! \nDeseja visualizar e excluir seu cadastro?",
                                "Informação fornecedor", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (excluiCadastro.Equals(MessageBoxResult.Yes))
                        {
                            var telaCrudCliente = new CrudFornecedor(login, "excluir", "BUSCA-FORNECEDOR", funcionario, null, result[0]);
                            telaCrudCliente.Show();
                            Close();
                        }
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    var responseUsua = MessageBox.Show("Fornecedor não encontrado! \nDeseja realizar seu cadastro?",
                        "Informação fornecedor", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK))
                    {
                        var telaCrudCliente = new CrudFornecedor(login, "cadastrar", "BUSCA-FORNECEDOR", funcionario, null);
                        telaCrudCliente.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    BuscarFornecedorBy();
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private void VoltarTelaAnterior(object sender, RoutedEventArgs e)
        {
            if (telaAnterior.ToUpper().Contains("ADMIN"))
            {
                var telaAdmin = new MenuAdmin(login, funcionario);
                telaAdmin.Show();
            }
            Close();
        }

        private void OnEnter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarFornecedorBy();
            }
        }
    }
}
