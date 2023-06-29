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
using wpf_sol_pets._4TelasCrudCliente._4._2CruPetCliente;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._3TelasBusca._3._4BuscarPet
{
    /// <summary>
    /// Lógica interna para BuscarPet.xaml
    /// </summary>
    public partial class BuscarPet : Window
    {
        private readonly LoginViewModel infoLogin;
        private readonly FuncionarioViewModel funcionario;
        private readonly string telaAnterior;
        private List<PetViewModel> pets = new();
        public BuscarPet(LoginViewModel infoLogin, FuncionarioViewModel funcionario, string telaAnterior)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
            this.telaAnterior = telaAnterior;
            pop1.IsOpen = false;
        }

        private void OnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarPetByName();
            }
        }

        private async void BuscarPetByName(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                btnBuscar.Visibility = Visibility.Hidden;
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = "/cliente/nome-pet/" + txtCampo.Text;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                await TratarResult(response);
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                btnBuscar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<PetViewModel>> TratarResult(HttpResponseMessage response)
        {
            var result = new List<PetViewModel>();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<List<PetViewModel>>(responseJson);
                    if (result.Count > 1)
                    {
                        pets = result;
                        pop1.IsOpen = true;
                    }
                    else
                    {
                        var petTutor = result[0];

                        var responseUsua = MessageBox.Show("Pet encontrado! \nDeseja visualizar e editar seu cadastro?",
                            "Informação pet", MessageBoxButton.YesNo, MessageBoxImage.Information);

                        if (responseUsua.Equals(MessageBoxResult.Yes) && petTutor.IdPet > 0)
                        {
                            var telaCrudCliente = new CrudPetCliente(infoLogin, funcionario, "BUSCAR-PET", null, petTutor, "EDICAO");
                            telaCrudCliente.Show();
                            Close();
                        }
                        else if (responseUsua.Equals(MessageBoxResult.No) && petTutor.IdPet > 0)
                        {
                            var responseUsuaExcluir = MessageBox.Show("Pet encontrado! \nDeseja excluir seu cadastro?",
                                "Informação pet", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (responseUsuaExcluir.Equals(MessageBoxResult.Yes))
                            {
                                var telaCrudCliente = new CrudPetCliente(infoLogin, funcionario, "BUSCAR-PET", null, petTutor, "EXCLUSAO");
                                telaCrudCliente.Show();
                                Close();
                            }
                        }
                    }

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseUsua = MessageBox.Show("Pet não encontrado! \nDeseja realizar seu cadastro?" +
                        " \nOBS: Cadastre apenas pet's para clientes que já possuem cadastro no sistema!!",
                        "Informação PET", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK))
                    {
                        var telaCrudCliente = new CrudPetCliente(infoLogin, funcionario, "BUSCAR-PET", null, null, "CADASTRO");
                        telaCrudCliente.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    GeneralExtensions.TokenView = "";
                    BuscarPetByName();
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
            if (telaAnterior.ToUpper().Trim().Contains("ADMIN"))
            {
                var telaMenu = new MenuAdmin(infoLogin, funcionario);
                telaMenu.Show();
            }
            Close();
        }

        private void Cancelar(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Obrigatório informar o CPF do tutor!", "Atenção", MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
        }

        private async void Confirmar(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                var petTutor = new PetViewModel();
                if (string.IsNullOrEmpty(txtCpf.Text))
                    throw new Exception("Obrigatório informar o CPF do tutor!");
                else if (!txtCpf.Text.ValidarCPF())
                    throw new Exception("CPF informado é inválido!");
                var clientes = await GetClienteByCpf(txtCpf.Text);

                foreach (var pet in pets)
                {
                    foreach (var cliente in clientes)
                    {
                        if (pet.IdCliente == cliente.IdCliente)
                            petTutor = pet;
                    }
                }
                if (petTutor.IdPet > 0)
                {
                    var responseUsua = MessageBox.Show("Pet encontrado! \nDeseja visualizar e editar seu cadastro?",
                                "Informação pet", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (responseUsua.Equals(MessageBoxResult.Yes) && petTutor.IdPet > 0)
                    {
                        var telaCrudCliente = new CrudPetCliente(infoLogin, funcionario, "BUSCAR-PET", null, petTutor, "EDICAO");
                        telaCrudCliente.Show();
                        pop1.IsOpen = false;
                        Close();
                    }
                    else if (responseUsua.Equals(MessageBoxResult.No) && petTutor.IdPet > 0)
                    {
                        var responseUsuaExcluir = MessageBox.Show("Pet encontrado! \nDeseja excluir seu cadastro?",
                            "Informação pet", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (responseUsuaExcluir.Equals(MessageBoxResult.Yes))
                        {
                            var telaCrudCliente = new CrudPetCliente(infoLogin, funcionario, "BUSCAR-PET", null, petTutor, "EXCLUSAO");
                            telaCrudCliente.Show();
                            pop1.IsOpen = false;
                            Close();
                        }
                    }
                }
                else
                {
                    throw new Exception("Ocorreu um erro ao buscar o Pet. Tente novamente!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
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

                result = await TratarResult(response, cpf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private async Task<List<ClienteViewModel>> TratarResult(HttpResponseMessage response, string cpf)
        {
            var result = new List<ClienteViewModel>();

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<List<ClienteViewModel>>(responseJson);
                    return result;
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    MessageBox.Show("Cliente não encontrado! \nVerifique se os dados estão corretos!",
                         "Informação cliente", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Visibility = Visibility.Hidden;
                    btnBuscar.Visibility = Visibility.Visible;
                    Loading.Spin = false;
                    GeneralExtensions.TokenView = "";
                    await GetClienteByCpf(cpf);
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
