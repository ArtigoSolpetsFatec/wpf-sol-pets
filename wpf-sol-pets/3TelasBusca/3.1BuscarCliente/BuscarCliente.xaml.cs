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
using wpf_sol_pets._4TelasCrudCliente._4_1CrudCliente;
using wpf_sol_pets._7TelaInicioVenda;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._3TelasBusca._3._1BuscarCliente
{
    /// <summary>
    /// Lógica interna para BuscarCliente.xaml
    /// </summary>
    public partial class BuscarCliente : Window
    {
        private readonly LoginViewModel login = new();
        private readonly FuncionarioViewModel funcionario = new();
        private readonly string telaAnterior;
        private Pedido pedidoIniciado = new();

        public BuscarCliente(LoginViewModel login, FuncionarioViewModel funcionario, string telaAnterior, Pedido pedidoIniciado = null)
        {
            InitializeComponent();
            this.funcionario = funcionario;
            this.login = login;
            this.telaAnterior = telaAnterior;
            this.pedidoIniciado = pedidoIniciado;
        }

        private async void BuscarClienteBy(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                btnBuscar.Visibility = Visibility.Hidden;
                Loading.Spin = true;

                string resultCampo = ValidaCampos();

                if (comboCpf.IsSelected && !string.IsNullOrEmpty(resultCampo))
                {
                    var clienteList = await GetClienteByCpf(resultCampo);
                }
                else if (comboNome.IsSelected && !string.IsNullOrEmpty(resultCampo))
                {
                    var cliente = await GetClienteByNome(resultCampo);
                }
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                btnBuscar.Visibility = Visibility.Visible;
                Loading.Spin = false;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ValidaCampos()
        {
            string textoCampo;
            try
            {
                textoCampo = !string.IsNullOrEmpty(txtCampo.Text) ?
                   txtCampo.Text.Replace(".", "").Replace("-", "").Replace("/", "") :
                   throw new Exception("Obrigatório informar um dado no campo");

                if (comboCpf.IsSelected && textoCampo.Length != 11)
                {
                    throw new Exception("O CPF informado deve possuir 11 números!");
                }
                else if (comboCpf.IsSelected && textoCampo.Length == 11)
                {
                    if (!textoCampo.ValidarCPF())
                    {
                        throw new Exception("O CPF informado é inválido!");
                    }
                }
                else if (comboNome.IsSelected)
                {
                    string[] nomeCompleto = textoCampo.Split(' ');
                    if (nomeCompleto.Length < 2)
                    {
                        throw new Exception("Obrigatório informar ao menos 1 sobrenome");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return textoCampo;
        }

        private async Task<List<ClienteViewModel>> GetClienteByCpf(string cpf)
        {
            var result = new List<ClienteViewModel>();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/clientes/cpf-cliente/" + cpf;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResult(response, cpf, true, string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private async Task<List<ClienteViewModel>> TratarResult(HttpResponseMessage response, string cpf,
            bool getByCpf, string nome)
        {
            var result = new List<ClienteViewModel>();
            var pedido = new Pedido();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<List<ClienteViewModel>>(responseJson);
                    if (pedidoIniciado != null)
                        pedido = await VincularClientePedido(result[0].IdCliente);

                    var responseUsua = MessageBox.Show("Cliente encontrado! \nDeseja visualizar e editar seu cadastro?",
                        "Informação cliente", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (responseUsua.Equals(MessageBoxResult.Yes) && result.Count > 0 && result[0].IdCliente > 0 && pedidoIniciado != null)
                    {
                        var telaCrudCliente = new CrudCliente(false, true, false, login, funcionario, "CLIENTE-VINCULO", pedido, result[0]);
                        telaCrudCliente.Show();
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.Yes) && result.Count > 0 && result[0].IdCliente > 0 && pedidoIniciado == null)
                    {
                        var telaCrudCliente = new CrudCliente(false, true, false, login, funcionario, "CLIENTE-BUSCA", pedido, result[0]);
                        telaCrudCliente.Show();
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.No) && result.Count > 0 && result[0].IdCliente > 0 && pedidoIniciado != null)
                    {
                        var telaPedido = new CrudPedido(login, funcionario, "CLIENTE-VINCULO", pedido, true, result[0].NomeCliente);
                        telaPedido.Show();
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.No) && result.Count > 0 && result[0].IdCliente > 0 && pedidoIniciado != null)
                    {
                        var responseUsuaExcluir = MessageBox.Show("Cliente encontrado! \nDeseja excluir seu cadastro?",
                            "Informação cliente", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (responseUsuaExcluir.Equals(MessageBoxResult.Yes))
                        {
                            var telaCrudCliente = new CrudCliente(false, false, true, login, funcionario, "CLIENTE-VINCULO", pedido, result[0]);
                            telaCrudCliente.Show();
                            Close();
                        }
                    }
                    else if (responseUsua.Equals(MessageBoxResult.No) && result.Count > 0 && result[0].IdCliente > 0 && pedidoIniciado == null)
                    {
                        var responseUsuaExcluir = MessageBox.Show("Cliente encontrado! \nDeseja excluir seu cadastro?",
                            "Informação cliente", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (responseUsuaExcluir.Equals(MessageBoxResult.Yes))
                        {
                            var telaCrudCliente = new CrudCliente(false, false, true, login, funcionario, "CLIENTE-BUSCA", pedido, result[0]);
                            telaCrudCliente.Show();
                            Close();
                        }
                    }

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseUsua = MessageBox.Show("Cliente não encontrado! \nDeseja realizar seu cadastro?",
                        "Informação cliente", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK) && pedidoIniciado != null)
                    {
                        var telaCrudCliente = new CrudCliente(true, false, false, login, funcionario, "CLIENTE-VINCULO", pedidoIniciado, null);
                        telaCrudCliente.Show();
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.OK) && pedidoIniciado == null)
                    {
                        var telaCrudCliente = new CrudCliente(true, false, false, login, funcionario, "CLIENTE-BUSCA", pedidoIniciado, null);
                        telaCrudCliente.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized && getByCpf)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    GeneralExtensions.TokenView = "";
                    await GetClienteByCpf(cpf);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized && !getByCpf)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    GeneralExtensions.TokenView = "";
                    await GetClienteByNome(nome);
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

        private async Task<List<ClienteViewModel>> GetClienteByNome(string nome)
        {
            var result = new List<ClienteViewModel>();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/clientes/{nome}/nome-cliente";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResult(response, string.Empty, false, nome);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private void VoltarTelaAnterior(object sender, RoutedEventArgs e)
        {
            if (login.IdLogin > 0 && telaAnterior.ToUpper().Contains("ADMIN"))
            {
                var telaMenuAdmin = new MenuAdmin(login, funcionario);
                telaMenuAdmin.Show();
            }
            else if (telaAnterior.ToUpper().Contains("VINCULO"))
            {
                var telaPedido = new CrudPedido(login, funcionario, "VINCULO-CLIENTE", pedidoIniciado);
                telaPedido.Show();
            }
            Close();
        }

        private async Task<Pedido> VincularClientePedido(int idCliente)
        {
            var result = new Pedido();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/pedido/vincula-cliente/idPedido/{pedidoIniciado.IdPedido}/idCliente/{idCliente}";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Patch, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResultPedido(response, idCliente);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private async Task<Pedido> TratarResultPedido(HttpResponseMessage response, int idCliente)
        {
            var result = new Pedido();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<Pedido>(responseJson);
                    pedidoIniciado = result;

                    MessageBox.Show("Cliente vinculado com sucesso!",
                        "Informação cliente", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    var responseUsua = MessageBox.Show("Ocorreu um erro ao vincular cliente! \nPor favor, tente novamente",
                        "Vincular Cliente", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    await VincularClientePedido(idCliente);
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

        private void OnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarClienteBy();
            }
        }
    }
}
