using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wpf_sol_pets._15MenuFuncionarios;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._10TelaCrudCargo
{
    /// <summary>
    /// Lógica interna para CrudCargo.xaml
    /// </summary>
    public partial class CrudCargo : Window
    {
        private readonly LoginViewModel infoLogin = new();
        private readonly FuncionarioViewModel funcionario = new();
        private CargoViewModel cargo;
        private readonly string tipoTela;

        public CrudCargo(LoginViewModel infoLogin, FuncionarioViewModel funcionario, string tipoTela,
            CargoViewModel cargo = null)
        {
            InitializeComponent();
            ValidaCamposObrigatorios();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
            this.tipoTela = tipoTela;
            ValidaTipoTela();
            CarregaInfoCargo();
        }

        private void CarregaInfoCargo()
        {
            try
            {
                if (cargo != null)
                {
                    txtDesCargo.Text = cargo.NomeCargo;
                    txtSalCargo.Text = cargo.Salario.ToString("2F");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidaTipoTela()
        {
            stackBtnCadastrar.Visibility = tipoTela.ToUpper().Trim().Contains("CADASTRO") ? Visibility.Visible : Visibility.Hidden;
            stackBtnEditar.Visibility = tipoTela.ToUpper().Trim().Contains("EDICAO") ? Visibility.Visible : Visibility.Hidden;
            stackBtnExcluir.Visibility = tipoTela.ToUpper().Trim().Contains("EXCLUSAO") ? Visibility.Visible : Visibility.Hidden;
        }

        private async void ValidaCadastroCargo(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDesCargo.Text) && !string.IsNullOrEmpty(txtSalCargo.Text))
            {
                btnCadastrar.IsEnabled = false;
                btnCadastrar.ToolTip = "Cadastra informações do cargo";
                await CadastrarCargo();
            }
        }

        private async Task<CargoViewModel> CadastrarCargo()
        {
            var cargo = new Cargo();
            var result = new CargoViewModel();
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                cargo.NomeCargo = txtDesCargo.Text;
                if (double.TryParse(txtSalCargo.Text, out double salario))
                    cargo.Salario = double.Parse(txtSalCargo.Text, CultureInfo.InvariantCulture);
                else
                    throw new Exception("Informe um número decimal para o sálário. Ex: 1500.00");

                string url = $"/cargo";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(cargo), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResult(response);
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnCadastrar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private async Task<CargoViewModel> TratarResult(HttpResponseMessage response)
        {
            var result = new CargoViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    Loading.Spin = false;
                    stackBtnCadastrar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<CargoViewModel>(responseJson);

                    if (result.IdCargo > 0)
                    {
                        MessageBox.Show("Cargo cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        txtDesCargo.Text = "";
                        txtSalCargo.Text = "";
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
                    result = await CadastrarCargo();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private void OnChange(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidaCamposObrigatorios();
        }

        private async void ValidaCamposObrigatorios(bool cadastrarCargo = false)
        {
            if (!string.IsNullOrEmpty(txtDesCargo.Text) && !string.IsNullOrEmpty(txtSalCargo.Text))
            {
                btnCadastrar.IsEnabled = true;
                btnCadastrar.ToolTip = "Cadastra informações do cargo";
                if (cadastrarCargo)
                    await CadastrarCargo();
            }
            else
            {
                btnCadastrar.IsEnabled = false;
                btnCadastrar.ToolTip = "Preencha todos os campos obrigatórios!";
            }
        }

        private void VoltarATelaAnterior(object sender = null, RoutedEventArgs e = null)
        {
            var menuFuncionarios = new MenuFuncionarios(infoLogin, funcionario);
            menuFuncionarios.Show();
            Close();
        }

        private void ClickEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidaCamposObrigatorios(true);
            }
        }

        private async void ExcluirCargo(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnExcluir.Visibility = Visibility.Hidden;
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/cargo/excluir-cargo/" + cargo.IdCargo;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Put, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                var result = await TratarResultCargoEditDelete(response, false);
                if (result > 0)
                    VoltarATelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnExcluir.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditarCargo(object sender = null, RoutedEventArgs e = null)
        {
            var payloadEnvio = new CargoViewModel();
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnEditar.Visibility = Visibility.Hidden;
                payloadEnvio.IdCargo = cargo.IdCargo;
                payloadEnvio.NomeCargo = txtDesCargo.Text;
                if (double.TryParse(txtSalCargo.Text, out double salario))
                    payloadEnvio.Salario = salario;
                else
                    throw new Exception("Digite um salário válido! Ex: 1550.00");
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/cargo/atualiza-cargo";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Put, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                var result = await TratarResultCargoEditDelete(response, true);
                if (result > 0)
                    VoltarATelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnEditar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<int> TratarResultCargoEditDelete(HttpResponseMessage response, bool edicao)
        {
            var result = new int();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<int>(responseJson);
                    var msg = edicao ? "atualizados" : "excluídos";
                    MessageBox.Show($"Dados {msg} com sucesso!", "Informações Cargo", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return result;

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    var msg = edicao ? "atualizar" : "excluír";
                    throw new Exception($"Ocorreu um erro ao {msg} informações do cargo! Tente novamente.");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    if (edicao)
                        EditarCargo();
                    else
                        ExcluirCargo();
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
