using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._5TelaCrudProduto;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._12TelaCrudFornecedor
{
    /// <summary>
    /// Lógica interna para CrudFornecedor.xaml
    /// </summary>
    public partial class CrudFornecedor : Window
    {
        private readonly FuncionarioViewModel funcionario = new();
        private readonly LoginViewModel login = new();
        private readonly dynamic objPesquisado;
        private FornecedorViewModel fornecedor;

        private readonly string telaAnterior;

        public CrudFornecedor(LoginViewModel login, string tipoTela, string telaAnterior,
            FuncionarioViewModel funcionario, dynamic objPesquisado = null, FornecedorViewModel fornecedor = null)
        {
            InitializeComponent();
            this.login = login;
            this.telaAnterior = telaAnterior;
            this.funcionario = funcionario;
            this.objPesquisado = objPesquisado;
            this.fornecedor = fornecedor;
            VerificaTipoTela(tipoTela);
            CarregaInfoFornecedor();
        }

        private void CarregaInfoFornecedor()
        {
            try
            {
                if (fornecedor != null)
                {
                    txtNomeEmpresa.Text = fornecedor.NomeFornecedor;
                    txtCnpj.Text = fornecedor.CnpjFornecedor;
                    var ultimoContato = fornecedor.Contatos[^1];
                    txtCampoCelular.Text = ultimoContato.TelefoneCelular;
                    txtCampoEmail.Text = ultimoContato.EmailPrincipal;
                    txtCampoEmailSec.Text = ultimoContato.EmailSecundario;
                    txtCampoTelFixo.Text = ultimoContato.TelefoneFixo;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VerificaTipoTela(string tipoTela)
        {
            stackBtnCadastrar.Visibility = tipoTela.ToLower().Contains("cadastrar") ? Visibility.Visible : Visibility.Hidden;
            stackBtnEditar.Visibility = tipoTela.ToLower().Contains("editar") ? Visibility.Visible : Visibility.Hidden;
            stackBtnExcluir.Visibility = tipoTela.ToLower().Contains("excluir") ? Visibility.Visible : Visibility.Hidden;
        }

        private void VoltarTelaAnterior(object sender = null, RoutedEventArgs e = null)
        {
            if (telaAnterior.Contains("produto"))
            {
                var telaProduto = new CrudProduto(login, funcionario, "fornecedor", "cadastrar", objPesquisado);
                telaProduto.Show();
            }
            else
            {
                var telaMenuAdmin = new MenuAdmin(login, funcionario);
                telaMenuAdmin.Show();
            }
            Close();
        }

        private async void CadastrarFornecedor(object sender = null, RoutedEventArgs e = null)
        {
            var payloadEnvio = new Fornecedor();
            var contatoForn = new Contato();
            try
            {
                payloadEnvio.NomeFornecedor = txtNomeEmpresa.Text;
                payloadEnvio.CnpjFornecedor = txtCnpj.Text;
                contatoForn.EmailPrincipal = txtCampoEmail.Text;
                contatoForn.EmailSecundario = txtCampoEmailSec.Text;
                contatoForn.TelefoneCelular = txtCampoCelular.Text;
                contatoForn.TelefoneFixo = txtCampoTelFixo.Text;
                payloadEnvio.Contatos.Add(contatoForn);

                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/fornecedor";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResult(response);
                if (result.IdFornecedor > 0)
                    VoltarTelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnCadastrar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimparCampos()
        {
            var stringVazia = string.Empty;
            txtCampoEmail.Text = stringVazia;
            txtCnpj.Text = stringVazia;
            txtCampoEmail.Text = stringVazia;
            txtCampoEmailSec.Text = stringVazia;
            txtCampoCelular.Text = stringVazia;
            txtCampoTelFixo.Text = stringVazia;
        }

        private async Task<FornecedorViewModel> TratarResult(HttpResponseMessage response)
        {
            var result = new FornecedorViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    Loading.Spin = false;
                    stackBtnCadastrar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<FornecedorViewModel>(responseJson);

                    if (result.IdFornecedor > 0)
                    {
                        MessageBox.Show("Fornecedor cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        LimparCampos();
                    }

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception("Ocorreu um erro ao cadastrar cargo!");
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    CadastrarFornecedor();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private async void ExcluirFornecedor(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/fornecedor/excluir-fornecedor/{fornecedor.IdFornecedor}";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResultFornecedorEditDelete(response, false);
                if (result > 0)
                    VoltarTelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnCadastrar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditarFornecedor(object sender = null, RoutedEventArgs e = null)
        {
            var payloadEnvio = new Fornecedor();
            var contatoForn = new Contato();
            try
            {
                payloadEnvio.NomeFornecedor = txtNomeEmpresa.Text;
                payloadEnvio.CnpjFornecedor = txtCnpj.Text;
                contatoForn.EmailPrincipal = txtCampoEmail.Text;
                contatoForn.EmailSecundario = txtCampoEmailSec.Text;
                contatoForn.TelefoneCelular = txtCampoCelular.Text;
                contatoForn.TelefoneFixo = txtCampoTelFixo.Text;
                payloadEnvio.Contatos.Add(contatoForn);

                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/fornecedor/atualiza-fornecedor";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResultFornecedorEditDelete(response, true);
                if (result > 0)
                    VoltarTelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnCadastrar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<int> TratarResultFornecedorEditDelete(HttpResponseMessage response, bool edicao)
        {
            var result = 0;
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<int>(responseJson);
                    var msg = edicao ? "atualizados" : "excluídos";
                    MessageBox.Show($"Dados {msg} com sucesso!", "Informações fornecedor", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return result;
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    var msg = edicao ? "atualizar" : "excluír";
                    throw new Exception($"Ocorreu um erro ao {msg} informações do fornecedor! Tente novamente.");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    if (edicao)
                        EditarFornecedor();
                    else
                        ExcluirFornecedor();
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


    }
}
