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
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._5TelaCrudProduto;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._3TelasBusca._3._2BuscarProduto
{
    /// <summary>
    /// Lógica interna para BuscarProduto.xaml
    /// </summary>
    public partial class BuscarProduto : Window
    {
        private readonly FuncionarioViewModel funcionario = new();
        private readonly LoginViewModel login = new();
        private string telaAnterior = string.Empty;

        public BuscarProduto(LoginViewModel login, FuncionarioViewModel funcionario, string telaAnterior)
        {
            InitializeComponent();
            this.funcionario = funcionario;
            this.login = login;
            this.telaAnterior = telaAnterior;
        }

        private void GetProduto(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                btnBuscar.Visibility = Visibility.Hidden;
                Loading.Spin = true;
                if (comboIsbn.IsSelected && !string.IsNullOrEmpty(txtCampo.Text))
                {
                    GetProdutoByIsbn();
                }
                else if (string.IsNullOrEmpty(txtCampo.Text))
                {
                    throw new Exception("Obrigatório preencher o campo de pesquisa!");
                }

            }
            catch (Exception ex)
            {
                Loading.Spin = false;
                Loading.Visibility = Visibility.Hidden;
                btnBuscar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GetProdutoByIsbn()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/produto/isbn/" + txtCampo.Text;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                var objPesquisa = new { tipo = "isbn", valor = txtCampo.Text };
                var result = await TratarResult(response, "isbn", objPesquisa);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<List<ProdutoViewModel>> TratarResult(HttpResponseMessage response,
            string tipoRequisicao, dynamic objPesquisa)
        {
            var result = new List<ProdutoViewModel>();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<List<ProdutoViewModel>>(responseJson);
                    return result;
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    var responseUsua = MessageBox.Show("produto não encontrado! \nDeseja realizar seu cadastro?",
                        "Informação Produto", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK))
                    {
                        var telaCrudProduto = new CrudProduto(login, funcionario, "buscar", "cadastrar", objPesquisa);
                        telaCrudProduto.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    GeneralExtensions.TokenView = "";
                    if (tipoRequisicao.Equals("isbn"))
                        GetProdutoByIsbn();
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
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
            if (telaAnterior.Contains("ADMIN"))
            {
                var telaMenu = new MenuAdmin(login, funcionario);
                telaMenu.Show();
            }
            else if (telaAnterior.Contains("PRODUTO"))
            {
                var telaMenu = new MenuAdmin(login, funcionario);
                telaMenu.Show();
            }
            Close();
        }

        private void OnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetProduto();
            }
        }
    }
}
