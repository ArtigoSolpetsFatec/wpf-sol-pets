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
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._4TelasCrudCliente._4_1CrudCliente;
using wpf_sol_pets._7TelaInicioVenda;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._4TelasCrudCliente._4._2CruPetCliente
{
    /// <summary>
    /// Lógica interna para CrudPetCliente.xaml
    /// </summary>
    public partial class CrudPetCliente : Window
    {
        private readonly bool cadastroCliente;
        private readonly bool edicaoCliente;
        private readonly bool exclusaoCliente;
        private readonly string telaAnterior;
        private readonly LoginViewModel login = new();
        private readonly FuncionarioViewModel funcionario = new();
        private Pedido pedidoIniciado = new();
        private ClienteViewModel cliente;
        private PetViewModel petCliente;
        private readonly string tipoTelaPet;
        public CrudPetCliente(LoginViewModel login, FuncionarioViewModel funcionario, string telaAnterior,
            ClienteViewModel cliente = null, PetViewModel petCliente = null, string tipoTelaPet = "",
            bool cadastroCliente = false, bool edicaoCliente = false, bool exclusaoCliente = false,
            Pedido pedidoIniciado = null)
        {
            InitializeComponent();
            this.telaAnterior = telaAnterior;
            this.login = login;
            this.funcionario = funcionario;
            this.cliente = cliente;
            this.pedidoIniciado = pedidoIniciado;
            this.cadastroCliente = cadastroCliente;
            this.edicaoCliente = edicaoCliente;
            this.exclusaoCliente = exclusaoCliente;
            this.tipoTelaPet = tipoTelaPet;
            this.petCliente = petCliente;
            ValidaTipoTela();
        }

        public async void CarregaInfoPet()
        {
            if (petCliente?.IdPet > 0)
            {

                txtNomePet.Text = petCliente.NomePet;
                txtTpAnimal.Text = petCliente.TipoAnimalPet;
                txtRacaPet.Text = petCliente.RacaPet;
                txtDhNascimento.Text = petCliente.DataNascimentoPet.ToString("dd/MM/yyyy");
                var tutorPet = await GetTutorPet();
                txtDonoPet.Text = "Tutor: " + tutorPet.NomeCliente;
            }
        }

        private async Task<ClienteViewModel> GetTutorPet()
        {

            var result = new ClienteViewModel();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/cliente/" + petCliente.IdCliente;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResultCliente(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private async Task<ClienteViewModel> TratarResultCliente(HttpResponseMessage response)
        {
            var result = new ClienteViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<ClienteViewModel>(responseJson);
                    return result;

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception("Ocorreu um erro ao buscar informações do tutor do pet!");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    await GetTutorPet();
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

        private void ValidaTipoTela()
        {
            if (!string.IsNullOrEmpty(tipoTelaPet))
            {
                if (tipoTelaPet.ToUpper().Trim().Contains("EDICAO"))
                {
                    stackBtnEditar.Visibility = Visibility.Visible;
                    stackBtnExcluir.Visibility = Visibility.Hidden;
                    stackBtnCadastrar.Visibility = Visibility.Hidden;
                    CarregaInfoPet();
                }
                else if (tipoTelaPet.ToUpper().Trim().Contains("CADASTRO"))
                {
                    stackBtnEditar.Visibility = Visibility.Hidden;
                    stackBtnExcluir.Visibility = Visibility.Hidden;
                    stackBtnCadastrar.Visibility = Visibility.Visible;
                }
                else
                {
                    stackBtnEditar.Visibility = Visibility.Hidden;
                    stackBtnExcluir.Visibility = Visibility.Visible;
                    stackBtnCadastrar.Visibility = Visibility.Hidden;
                    CarregaInfoPet();
                }
            }
        }

        private void VoltarTelaAnterior(object sender = null, RoutedEventArgs e = null)
        {
            if (telaAnterior.ToUpper().Contains("VINCULO"))
            {
                var telaPedido = new CrudPedido(login, funcionario, "CLIENTE-VINCULO", pedidoIniciado, true,
                    cliente.NomeCliente);
                telaPedido.Show();
            }
            else if (telaAnterior.ToUpper().Contains("PET"))
            {
                var menuAdmin = new MenuAdmin(login, funcionario);
                menuAdmin.Show();
            }
            else
            {
                var telaCliente = new CrudCliente(cadastroCliente, edicaoCliente, exclusaoCliente, login, funcionario, "PET", pedidoIniciado
                , cliente);
                telaCliente.Show();

            }
            Close();
        }

        private async void CadastrarDadosPet(object sender, RoutedEventArgs e)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnEditar.Visibility = Visibility.Hidden;
                var result = await CadastrarPet();
                if (result.IdPet > 0)
                    VoltarTelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnEditar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditarDadosPet(object sender, RoutedEventArgs e)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnEditar.Visibility = Visibility.Hidden;
                var result = await EditarPet();
                if (result > 0)
                    VoltarTelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnEditar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExcluirDadosPet(object sender, RoutedEventArgs e)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnExcluir.Visibility = Visibility.Hidden;
                var result = await ExcluirPet();
                if (result > 0)
                    VoltarTelaAnterior();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnExcluir.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<PetViewModel> CadastrarPet()
        {
            var result = new PetViewModel();
            var payloadEnvio = new PetCliente();
            try
            {
                if (DateTime.TryParseExact(txtDhNascimento.Text, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime dhNascimento))
                {
                    payloadEnvio.DataNascimentoPet = dhNascimento;
                }
                else
                {
                    throw new Exception("Data de nascimento é inválida! Exemplo correto: 05/12/2020");
                }
                payloadEnvio.NomePet = txtNomePet.Text;
                payloadEnvio.RacaPet = txtRacaPet.Text;
                payloadEnvio.TipoAnimalPet = txtTpAnimal.Text;
                payloadEnvio.IdCliente = petCliente.IdCliente;

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/cliente/cadastrar-pet";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                result = await TratarResultPet(response);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> EditarPet()
        {
            var result = 0;
            var payloadEnvio = new PetViewModel();
            try
            {
                if (DateTime.TryParseExact(txtDhNascimento.Text, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime dhNascimento))
                {
                    payloadEnvio.DataNascimentoPet = dhNascimento;
                }
                else
                {
                    throw new Exception("Data de nascimento é inválida! Exemplo correto: 05/12/2020");
                }
                payloadEnvio.NomePet = txtNomePet.Text;
                payloadEnvio.RacaPet = txtRacaPet.Text;
                payloadEnvio.TipoAnimalPet = txtTpAnimal.Text;
                payloadEnvio.IdPet = petCliente.IdPet;
                payloadEnvio.IdCliente = petCliente.IdCliente;

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/cliente/atualiza-pet-cliente";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Put, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                result = await TratarResultPetEditDelete(response, true);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> ExcluirPet()
        {
            var result = 0;
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = $"/cliente/{petCliente.IdCliente}/exclui-pet/{petCliente.IdPet}";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Delete, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                result = await TratarResultPetEditDelete(response, false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> TratarResultPetEditDelete(HttpResponseMessage response, bool edicao)
        {
            var result = new int();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<int>(responseJson);
                    var msg = edicao ? "atualizados" : "excluídos";
                    MessageBox.Show($"Dados {msg} com sucesso!", "Informações Pet", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return result;

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    var msg = edicao ? "atualizar" : "excluír";
                    throw new Exception($"Ocorreu um erro ao {msg} informações do pet! Tente novamente.");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    if (edicao)
                        await EditarPet();
                    else
                        await ExcluirPet();
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

        private async Task<PetViewModel> TratarResultPet(HttpResponseMessage response)
        {
            var result = new PetViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<PetViewModel>(responseJson);
                    MessageBox.Show($"Pet cadastrado com sucesso", "Informações Pet", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return result;
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception($"Ocorreu um erro ao cadastrar informações do pet! Tente novamente.");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    await CadastrarPet();
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
