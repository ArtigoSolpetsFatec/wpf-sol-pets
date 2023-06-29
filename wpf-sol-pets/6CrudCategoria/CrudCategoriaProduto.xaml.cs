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

namespace wpf_sol_pets._6CrudCategoria
{
    /// <summary>
    /// Lógica interna para CrudCategoriaProduto.xaml
    /// </summary>
    public partial class CrudCategoriaProduto : Window
    {
        private readonly FuncionarioViewModel funcionario = new();
        private readonly LoginViewModel login = new();
        private readonly string telaAnterior;
        private readonly dynamic objPesquisado;
        private CategoriaViewModel categoria;

        public CrudCategoriaProduto(string tipoTela, string telaAnterior, FuncionarioViewModel funcionario,
            LoginViewModel login, CategoriaViewModel categoria = null, dynamic objPesquisado = null)
        {
            InitializeComponent();
            this.telaAnterior = telaAnterior;
            this.funcionario = funcionario;
            this.login = login;
            this.objPesquisado = objPesquisado;
            this.categoria = categoria;
            VerificaTipoTela(tipoTela);
            CarregaInfoCategoria();
        }

        private void CarregaInfoCategoria()
        {
            try
            {
                if (categoria != null)
                {
                    txtCampoAnimalAplicavel.Text = categoria.TipoAnimal;
                    txtCampoCategoria.Text = categoria.TipoCategoria;
                    txtCampoDescricao.Text = categoria.DescricaoCategoria;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VerificaTipoTela(string tipoTela)
        {
            stackBtnCadastrar.Visibility = tipoTela.ToLower().Contains("cadastro") ? Visibility.Visible : Visibility.Hidden;
            stackBtnEditar.Visibility = tipoTela.ToLower().Contains("editar") ? Visibility.Visible : Visibility.Hidden;
            stackBtnExcluir.Visibility = tipoTela.ToLower().Contains("excluir") ? Visibility.Visible : Visibility.Hidden;

            if (tipoTela.ToLower().Contains("cadastro"))
            {
                txtCampoCategoria.Text = objPesquisado.nomeCategoria;
            }
        }

        private void VoltarTelaAnterior(object sender = null, RoutedEventArgs e = null)
        {
            if (telaAnterior.Contains("produto"))
            {
                var telaProduto = new CrudProduto(login, funcionario, "CRUD-CATEGORIA", "cadastrar", objPesquisado);
                telaProduto.Show();
            }
            else
            {
                var menuAdmin = new MenuAdmin(login, funcionario);
                menuAdmin.Show();
            }
            Close();
        }

        private async void CadatrarCategoria(object sender = null, RoutedEventArgs e = null)
        {
            var payloadEnvio = new Categoria();
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                payloadEnvio.DescricaoCategoria = txtCampoDescricao.Text;
                payloadEnvio.TipoAnimal = txtCampoAnimalAplicavel.Text;
                payloadEnvio.TipoCategoria = txtCampoCategoria.Text;

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/categoria";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResult(response);
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnCadastrar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<CategoriaViewModel> TratarResult(HttpResponseMessage response)
        {
            var result = new CategoriaViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    Loading.Spin = false;
                    stackBtnCadastrar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<CategoriaViewModel>(responseJson);

                    if (result.IdCategoria > 0)
                    {
                        MessageBox.Show("Categoria cadastrada com sucesso!", "Sucesso", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        VoltarTelaAnterior();
                    }

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception("Ocorreu um erro ao cadastrar produto!");
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    CadatrarCategoria();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private async Task<int> EditarCategoria()
        {
            var result = 0;
            var payloadEnvio = new CategoriaViewModel();
            try
            {
                payloadEnvio.DescricaoCategoria = txtCampoDescricao.Text;
                payloadEnvio.TipoCategoria = txtCampoCategoria.Text;
                payloadEnvio.TipoAnimal = txtCampoAnimalAplicavel.Text;
                payloadEnvio.IdCategoria = categoria.IdCategoria;

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/categoria/atualiza-categoria";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Put, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                result = await TratarResultCategoriaEditDelete(response, true);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> ExcluirCategoria()
        {
            var result = 0;
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = $"/categoria/exclui-categoria/{categoria.IdCategoria}";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Delete, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                result = await TratarResultCategoriaEditDelete(response, false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> TratarResultCategoriaEditDelete(HttpResponseMessage response, bool edicao)
        {
            var result = 0;
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<int>(responseJson);
                    var msg = edicao ? "atualizados" : "excluídos";
                    MessageBox.Show($"Dados {msg} com sucesso!", "Informações Categoria", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return result;

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    var msg = edicao ? "atualizar" : "excluír";
                    throw new Exception($"Ocorreu um erro ao {msg} informações da categoria! Tente novamente.");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    if (edicao)
                        await EditarCategoria();
                    else
                        await ExcluirCategoria();
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

        private async void ExcluiDadosCategoria(object sender, RoutedEventArgs e)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                var result = await ExcluirCategoria();
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

        private async void EditaDadosCategoria(object sender, RoutedEventArgs e)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                var result = await EditarCategoria();
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
    }
}
