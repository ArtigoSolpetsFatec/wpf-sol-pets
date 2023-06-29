using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._6CrudCategoria;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._3TelasBusca._3._5BuscarCategoria
{
    /// <summary>
    /// Lógica interna para BuscarCategoria.xaml
    /// </summary>
    public partial class BuscarCategoria : Window
    {
        private readonly FuncionarioViewModel funcionario = new();
        private readonly LoginViewModel login = new();
        private readonly string telaAnteiror;
        public BuscarCategoria(LoginViewModel login, FuncionarioViewModel funcionario, string telaAnteiror)
        {
            InitializeComponent();
            this.login = login;
            this.funcionario = funcionario;
            this.telaAnteiror = telaAnteiror;
        }

        private void OnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BuscaCategoria();
        }

        private async void BuscaCategoria(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                btnBuscar.Visibility = Visibility.Hidden;
                if (!string.IsNullOrEmpty(txtCampo.Text))
                    await BuscarCategoriaByName();
                else
                    throw new Exception("Obrigatório informar o nome da categoria!");
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                btnBuscar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<CategoriaViewModel> BuscarCategoriaByName()
        {
            var result = new CategoriaViewModel();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = "/categoria/nome-categoria/" + txtCampo.Text;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResult(response);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<CategoriaViewModel>(responseJson);

                    var responseUsua = MessageBox.Show("Categoria encontrada! \nDeseja visualizar e editar seu cadastro?",
                        "Informação Categoria", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (responseUsua.Equals(MessageBoxResult.Yes))
                    {
                        var telaCrudCategoria = new CrudCategoriaProduto("editar", "BUSCA-CATEGORIA",
                            funcionario, login, result);
                        telaCrudCategoria.Show();
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.No))
                    {
                        var responseUsuaExcluir = MessageBox.Show("Categoria encontrada! \nDeseja excluir seu cadastro? " +
                            "\nOBS: Para excluir uma categoria, é obrigatório alterar TODOS os produtos que a utilizam antes." +
                            "Caso contrário, a exclusão irá falhar!!",
                            "Informação categoria", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (responseUsuaExcluir.Equals(MessageBoxResult.Yes))
                        {
                            var telaCrudCategoria = new CrudCategoriaProduto("excluir", "BUSCA-CATEGORIA",
                            funcionario, login, result);
                            telaCrudCategoria.Show();
                            Close();
                        }
                    }
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseUsua = MessageBox.Show("Categoria não encontrada! \nDeseja realizar seu cadastro?",
                        "Informação categoria", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK))
                    {
                        var telaCrudCategoria = new CrudCategoriaProduto("cadastro", "BUSCA-CATEGORIA",
                            funcionario, login, null, new { nomeCategoria = txtCampo.Text });
                        telaCrudCategoria.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    GeneralExtensions.TokenView = "";
                    BuscaCategoria();
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

        private void VoltaTelaAnterior(object sender, RoutedEventArgs e)
        {
            if (telaAnteiror.ToUpper().Trim().Contains("ADMIN"))
            {
                var menuAdmin = new MenuAdmin(login, funcionario);
                menuAdmin.Show();
            }
            Close();
        }
    }
}
