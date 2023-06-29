using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wpf_sol_pets._15MenuFuncionarios;
using wpf_sol_pets._16CrudFuncionario;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._3TelasBusca._3._7BuscarFuncionario
{
    /// <summary>
    /// Lógica interna para BuscarFuncionario.xaml
    /// </summary>
    public partial class BuscarFuncionario : Window
    {
        private readonly LoginViewModel infoLogin = new();
        private FuncionarioViewModel funcionario = new();

        public BuscarFuncionario(LoginViewModel infoLogin, FuncionarioViewModel funcionario)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
        }

        private void OnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BuscarFuncionarioBy();
        }

        private async void BuscarFuncionarioBy(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                btnBuscar.Visibility = Visibility.Hidden;
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = "/funcionario/nome-funcionario/" + txtCampo.Text;
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
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                btnBuscar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<FuncionarioViewModel> TratarResult(HttpResponseMessage response)
        {
            var result = new FuncionarioViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<FuncionarioViewModel>(responseJson);

                    var responseUsua = MessageBox.Show("Funcionário encontrado! \nDeseja visualizar e editar seu cadastro?",
                        "Informação funcionário", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (responseUsua.Equals(MessageBoxResult.Yes))
                    {
                        var telaCrudFuncionario = new CrudFuncionario(infoLogin, funcionario, "EDICAO", result);
                        telaCrudFuncionario.Show();
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.No))
                    {
                        var responseUsuaExcluir = MessageBox.Show("Funcionário encontrad! \nDeseja excluir seu cadastro?",
                            "Informação funcionário", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (responseUsuaExcluir.Equals(MessageBoxResult.Yes))
                        {
                            var telaCrudFuncionario = new CrudFuncionario(infoLogin, funcionario, "EXCLUSAO", result);
                            telaCrudFuncionario.Show();
                            Close();
                        }
                    }
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseUsua = MessageBox.Show("Funcionário não encontrado! \nDeseja realizar seu cadastro?",
                        "Informação funcionário", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK))
                    {
                        var telaCrudFuncionario = new CrudFuncionario(infoLogin, funcionario, "CADASTRO", null);
                        telaCrudFuncionario.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    GeneralExtensions.TokenView = "";
                    BuscarFuncionarioBy();
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

        private void VoltarTelaFuncionario(object sender, RoutedEventArgs e)
        {
            var menuFuncionario = new MenuFuncionarios(infoLogin, funcionario);
            menuFuncionario.Show();
            Close();
        }
    }
}
