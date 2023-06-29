using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wpf_sol_pets._10TelaCrudCargo;
using wpf_sol_pets._15MenuFuncionarios;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._3TelasBusca._3._6BuscarCargo
{
    /// <summary>
    /// Lógica interna para BuscarCargo.xaml
    /// </summary>
    public partial class BuscarCargo : Window
    {
        private readonly LoginViewModel infoLogin = new();
        private FuncionarioViewModel funcionario = new();

        public BuscarCargo(LoginViewModel infoLogin, FuncionarioViewModel funcionario)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
        }

        private void OnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BuscarCargoByName();
        }

        private async void BuscarCargoByName(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                btnBuscar.Visibility = Visibility.Hidden;

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = "/cargo/descricao-cargo/" + txtCampo.Text;
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

        private async Task<CargoViewModel> TratarResult(HttpResponseMessage response)
        {
            var result = new CargoViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<CargoViewModel>(responseJson);

                    var responseUsua = MessageBox.Show("Cargo encontrado! \nDeseja visualizar e editar seu cadastro?",
                        "Informação cargo", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (responseUsua.Equals(MessageBoxResult.Yes))
                    {
                        var telaCrudCargo = new CrudCargo(infoLogin, funcionario, "EDICAO", result);
                        telaCrudCargo.Show();
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.No))
                    {
                        var responseUsuaExcluir = MessageBox.Show("Cargo encontrado! \nDeseja excluir seu cadastro? " +
                            "\nOBS: Para excluir um cargo, é obrigatório alterar TODOS os funcionários que o possuem antes." +
                            "Caso contrário, a exclusão irá falhar!!",
                            "Informação cargo", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (responseUsuaExcluir.Equals(MessageBoxResult.Yes))
                        {
                            var telaCrudCargo = new CrudCargo(infoLogin, funcionario, "EXCLUSAO", result);
                            telaCrudCargo.Show();
                            Close();
                        }
                    }
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseUsua = MessageBox.Show("Cargo não encontrado! \nDeseja realizar seu cadastro?",
                        "Informação cargo", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK))
                    {
                        var telaCrudCargo = new CrudCargo(infoLogin, funcionario, "CADASTRO", null);
                        telaCrudCargo.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    GeneralExtensions.TokenView = "";
                    BuscarCargoByName();
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
            var menuFuncionario = new MenuFuncionarios(infoLogin, funcionario);
            menuFuncionario.Show();
            Close();
        }
    }
}
