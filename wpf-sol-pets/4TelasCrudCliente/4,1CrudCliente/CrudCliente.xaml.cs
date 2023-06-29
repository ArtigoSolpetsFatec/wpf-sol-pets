using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._4TelasCrudCliente._4._2CruPetCliente;
using wpf_sol_pets._7TelaInicioVenda;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._4TelasCrudCliente._4_1CrudCliente
{
    /// <summary>
    /// Lógica interna para CrudCliente.xaml
    /// </summary>
    public partial class CrudCliente : Window
    {
        private bool Cadastrar { get; set; }
        private bool Editar { get; set; }
        private bool Excluir { get; set; }
        private bool NumObrigatorio { get; set; }
        private readonly LoginViewModel login = new();
        private readonly FuncionarioViewModel funcionario = new();
        private readonly string telaAnterior;
        private Pedido pedidoIniciado = new();
        private ClienteViewModel cliente;

        public CrudCliente(bool cadastrar, bool editar, bool excluir, LoginViewModel login, FuncionarioViewModel funcionario,
            string telaAnterior, Pedido pedidoIniciado, ClienteViewModel cliente = null)
        {
            Cadastrar = cadastrar;
            Editar = editar;
            Excluir = excluir;
            InitializeComponent();
            VerificarTipoTela(cadastrar, editar, excluir, cliente);
            this.login = login;
            this.funcionario = funcionario;
            this.telaAnterior = telaAnterior;
            this.pedidoIniciado = pedidoIniciado;
            this.cliente = cliente;
        }

        private void VerificarTipoTela(bool cadastrar, bool editar, bool excluir, ClienteViewModel cliente = null)
        {
            if (cadastrar)
            {
                stackBtnCadastrar.Visibility = Visibility.Visible;
                stackBtnEditar.Visibility = Visibility.Hidden;
                stackBtnExcluir.Visibility = Visibility.Hidden;
            }
            else if (editar && cliente.IdCliente > 0)
            {
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                stackBtnEditar.Visibility = Visibility.Visible;
                stackBtnExcluir.Visibility = Visibility.Hidden;
                CarregarDadosCliente(cliente);
            }
            else if (excluir && cliente.IdCliente > 0)
            {
                stackBtnCadastrar.Visibility = Visibility.Hidden;
                stackBtnEditar.Visibility = Visibility.Hidden;
                stackBtnExcluir.Visibility = Visibility.Visible;
                CarregarDadosCliente(cliente);
            }
        }

        private void CarregarDadosCliente(ClienteViewModel cliente)
        {
            try
            {
                if (!string.IsNullOrEmpty(cliente.NomeCliente))
                {
                    txtCampoNome.Text = cliente.NomeCliente;
                }
                if (!string.IsNullOrEmpty(cliente.NomeEmpresaCliente))
                {
                    txtCampoNomeEmp.Text = cliente.NomeEmpresaCliente;
                }
                if (!string.IsNullOrEmpty(cliente.CpfCliente))
                {
                    txtCampoCpf.Text = cliente.CpfCliente;
                }
                if (!string.IsNullOrEmpty(cliente.CnpjCliente))
                {
                    txtCampoCNPJ.Text = cliente.CnpjCliente;
                }
                if (!char.IsWhiteSpace(cliente.SexoCliente))
                {
                    if (cliente.SexoCliente == 'F')
                    {
                        comboSexoFem.IsSelected = true;
                        comboSexoMasc.IsSelected = false;
                        comboSexoIndefinido.IsSelected = false;
                    }
                    else if (cliente.SexoCliente == 'M')
                    {
                        comboSexoMasc.IsSelected = true;
                        comboSexoFem.IsSelected = false;
                        comboSexoIndefinido.IsSelected = false;
                    }
                    else
                    {
                        comboSexoIndefinido.IsSelected = true;
                        comboSexoFem.IsSelected = false;
                        comboSexoMasc.IsSelected = false;
                    }
                }
                if (!string.IsNullOrEmpty(cliente.RgCliente))
                {
                    txtCampoRg.Text = cliente.RgCliente;
                }
                if (!string.IsNullOrEmpty(cliente.UfRg))
                {
                    VerificarUfRgCliente(cliente.UfRg);
                }
                if (cliente.DataNascimentoCliente != DateTime.MinValue)
                {
                    txtCampoDhNascimento.Text = cliente.DataNascimentoCliente.ToString("dd/MM/yyyy");
                }
                foreach (var pet in cliente.PetsCliente)
                {
                    pet.Idade = GeneralExtensions.CalculaIdade(pet.DataNascimentoPet, DateTime.Now);
                }
                ListPets.ItemsSource = cliente.PetsCliente;
                CarregaDadosEnderecoContatoCliente(cliente);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregaDadosEnderecoContatoCliente(ClienteViewModel cliente)
        {
            if (cliente.ContatosCliente.Count > 0)
            {
                var contatosOrdenados = cliente.ContatosCliente.OrderBy(x => x.DHUltimaAtualizacao);
                var ultimoContato = contatosOrdenados.FirstOrDefault();
                if (ultimoContato.IdContato > 0 && (!string.IsNullOrEmpty(ultimoContato.TelefoneCelular)))
                {
                    txtCampoCelular.Text = ultimoContato.TelefoneCelular;
                }
                if (ultimoContato.IdContato > 0 && (!string.IsNullOrEmpty(ultimoContato.TelefoneFixo)))
                {
                    txtCampoTelFixo.Text = ultimoContato.TelefoneFixo;
                }
                if (ultimoContato.IdContato > 0 && (!string.IsNullOrEmpty(ultimoContato.EmailPrincipal)))
                {
                    txtCampoEmail.Text = ultimoContato.EmailPrincipal;
                }
                if (ultimoContato.IdContato > 0 && (!string.IsNullOrEmpty(ultimoContato.EmailSecundario)))
                {
                    txtCampoEmailSec.Text = ultimoContato.EmailSecundario;
                }
            }
            if (cliente.EnderecosCliente.Count > 0)
            {
                var enderecosOrdenados = cliente.EnderecosCliente.OrderBy(x => x.DHUltimaAtualizacao);
                var ultimoEndereco = enderecosOrdenados.FirstOrDefault();
                if (ultimoEndereco.IdEndreco > 0 && (!string.IsNullOrEmpty(ultimoEndereco.Logradouro)))
                {
                    txtCampoLogradouro.Text = ultimoEndereco.Logradouro;
                }
                if (ultimoEndereco.IdEndreco > 0 && (!string.IsNullOrEmpty(ultimoEndereco.Complemento)))
                {
                    txtCampoComplemento.Text = ultimoEndereco.Complemento;
                }
                if (ultimoEndereco.IdEndreco > 0 && (!string.IsNullOrEmpty(ultimoEndereco.Bairro)))
                {
                    txtCampoBairro.Text = ultimoEndereco.Bairro;
                }
                if (ultimoEndereco.IdEndreco > 0 && (!string.IsNullOrEmpty(ultimoEndereco.Cep)))
                {
                    txtCampoCep.Text = ultimoEndereco.Cep;
                }
                if (ultimoEndereco.IdEndreco > 0 && (!string.IsNullOrEmpty(ultimoEndereco.Cidade)))
                {
                    txtCampoCidade.Text = ultimoEndereco.Cidade;
                }
                if (ultimoEndereco.IdEndreco > 0 && (!string.IsNullOrEmpty(ultimoEndereco.UfEstado)))
                {
                    VerificarUfCepCliente(ultimoEndereco.UfEstado);
                }
                if (ultimoEndereco.IdEndreco > 0 && ultimoEndereco.Numero > 0)
                {
                    txtCampoNumero.Text = ultimoEndereco.Numero.ToString();
                }
            }
        }

        private void VerificarUfRgCliente(string ufRgCliente)
        {
            switch (ufRgCliente)
            {
                case "AC":
                    comboEstadoRgAc.IsSelected = true;
                    break;
                case "AL":
                    comboEstadoRgAl.IsSelected = true;
                    break;
                case "AP":
                    comboEstadoRgAp.IsSelected = true;
                    break;
                case "AM":
                    comboEstadoRgAm.IsSelected = true;
                    break;
                case "BA":
                    comboEstadoRgBa.IsSelected = true;
                    break;
                case "CE":
                    comboEstadoRgCe.IsSelected = true;
                    break;
                case "DF":
                    comboEstadoRgDf.IsSelected = true;
                    break;
                case "ES":
                    comboEstadoRgEs.IsSelected = true;
                    break;
                case "GO":
                    comboEstadoRgGo.IsSelected = true;
                    break;
                case "MA":
                    comboEstadoRgMa.IsSelected = true;
                    break;
                case "MT":
                    comboEstadoRgMt.IsSelected = true;
                    break;
                case "MS":
                    comboEstadoRgMs.IsSelected = true;
                    break;
                case "MG":
                    comboEstadoRgMg.IsSelected = true;
                    break;
                case "PA":
                    comboEstadoRgPa.IsSelected = true;
                    break;
                case "PB":
                    comboEstadoRgPb.IsSelected = true;
                    break;
                case "PR":
                    comboEstadoRgPr.IsSelected = true;
                    break;
                case "PE":
                    comboEstadoRgPe.IsSelected = true;
                    break;
                case "PI":
                    comboEstadoRgPi.IsSelected = true;
                    break;
                case "RJ":
                    comboEstadoRgRj.IsSelected = true;
                    break;
                case "RN":
                    comboEstadoRgRn.IsSelected = true;
                    break;
                case "RS":
                    comboEstadoRgRs.IsSelected = true;
                    break;
                case "RO":
                    comboEstadoRgRo.IsSelected = true;
                    break;
                case "RR":
                    comboEstadoRgRr.IsSelected = true;
                    break;
                case "SC":
                    comboEstadoRgSc.IsSelected = true;
                    break;
                case "SP":
                    comboEstadoRgSp.IsSelected = true;
                    break;
                case "SE":
                    comboEstadoRgSe.IsSelected = true;
                    break;
                case "TO":
                    comboEstadoRgTo.IsSelected = true;
                    break;
                default:
                    comboEstadoRgVazio.IsSelected = true;
                    break;
            }
        }

        private void VerificarUfCepCliente(string ufCepCliente)
        {
            switch (ufCepCliente)
            {
                case "AC":
                    comboEstadoCepAc.IsSelected = true;
                    break;
                case "AL":
                    comboEstadoCepAl.IsSelected = true;
                    break;
                case "AP":
                    comboEstadoCepAp.IsSelected = true;
                    break;
                case "AM":
                    comboEstadoCepAm.IsSelected = true;
                    break;
                case "BA":
                    comboEstadoCepBa.IsSelected = true;
                    break;
                case "CE":
                    comboEstadoCepCe.IsSelected = true;
                    break;
                case "DF":
                    comboEstadoCepDf.IsSelected = true;
                    break;
                case "ES":
                    comboEstadoCepEs.IsSelected = true;
                    break;
                case "GO":
                    comboEstadoCepGo.IsSelected = true;
                    break;
                case "MA":
                    comboEstadoCepMa.IsSelected = true;
                    break;
                case "MT":
                    comboEstadoCepMt.IsSelected = true;
                    break;
                case "MS":
                    comboEstadoCepMs.IsSelected = true;
                    break;
                case "MG":
                    comboEstadoCepMg.IsSelected = true;
                    break;
                case "PA":
                    comboEstadoCepPa.IsSelected = true;
                    break;
                case "PB":
                    comboEstadoCepPb.IsSelected = true;
                    break;
                case "PR":
                    comboEstadoCepPr.IsSelected = true;
                    break;
                case "PE":
                    comboEstadoCepPe.IsSelected = true;
                    break;
                case "PI":
                    comboEstadoCepPi.IsSelected = true;
                    break;
                case "RJ":
                    comboEstadoCepRj.IsSelected = true;
                    break;
                case "RN":
                    comboEstadoCepRn.IsSelected = true;
                    break;
                case "RS":
                    comboEstadoCepRs.IsSelected = true;
                    break;
                case "RO":
                    comboEstadoCepRo.IsSelected = true;
                    break;
                case "RR":
                    comboEstadoCepRr.IsSelected = true;
                    break;
                case "SC":
                    comboEstadoCepSc.IsSelected = true;
                    break;
                case "SP":
                    comboEstadoCepSp.IsSelected = true;
                    break;
                case "SE":
                    comboEstadoCepSe.IsSelected = true;
                    break;
                case "TO":
                    comboEstadoCepTo.IsSelected = true;
                    break;
                default:
                    comboEstadoCepVazio.IsSelected = true;
                    break;
            }
        }

        private void VoltarTelaAnterior(object sender, RoutedEventArgs e)
        {
            if (telaAnterior.ToUpper().Trim().Contains("VINCULO"))
            {
                var telaPedido = new CrudPedido(login, funcionario, "CLIENTE", pedidoIniciado, true, txtCampoNome.Text);
                telaPedido.Show();
            }
            else
            {
                var telaMenu = new MenuAdmin(login, funcionario);
                telaMenu.Show();
            }
            Close();
        }

        private void OnChangeNome(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!Excluir)
                {
                    if (!string.IsNullOrEmpty(txtCampoNome.Text))
                    {
                        string nome = txtCampoNome.Text;
                        string[] nomeSobrenome = nome.Split(' ');
                        if (nomeSobrenome.Length == 1)
                        {
                            throw new Exception("Obrigatório informar ao menos 1 sobrenome!");
                        }
                        CpfObrigatorio.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CpfObrigatorio.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnChangeNomeEmpresa(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!Excluir)
                {
                    CnpjObrigatorio.Visibility = !string.IsNullOrEmpty(txtCampoNomeEmp.Text)
                        ? Visibility.Visible : Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnChangeCpf(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!Excluir)
                {
                    NomeClieObrigatorio.Visibility = !string.IsNullOrEmpty(txtCampoCpf.Text)
                        ? Visibility.Visible : Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnChangeCnpj(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!Excluir)
                {
                    NomeEmpObrigatorio.Visibility = !string.IsNullOrEmpty(txtCampoCNPJ.Text)
                        ? Visibility.Visible : Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<ClienteViewModel> CadastrarCliente()
        {
            var result = new ClienteViewModel();
            var payloadEnvio = new Cliente();
            var enderecoCliente = new EnderecoViewModel();
            var contatoCliente = new ContatoViewModel();

            try
            {
                payloadEnvio.NomeCliente = txtCampoNome.Text;
                payloadEnvio.NomeEmpresaCliente = txtCampoNomeEmp.Text;
                payloadEnvio.CpfCliente = txtCampoCpf.Text;
                payloadEnvio.CnpjCliente = txtCampoCNPJ.Text;
                payloadEnvio.SexoCliente = comboSexoFem.IsSelected ? 'F' : comboSexoMasc.IsSelected ? 'M' : 'I';
                payloadEnvio.RgCliente = txtCampoRg.Text;
                payloadEnvio.UfRg = GetRgSelecionado();
                payloadEnvio.DataNascimentoCliente = DateTime.TryParse(txtCampoDhNascimento.Text, out DateTime dhNascimento) ? dhNascimento :
                    throw new Exception("Data de nascimento informada é inválida!");
                enderecoCliente.Bairro = txtCampoBairro.Text;
                enderecoCliente.Numero = int.TryParse(txtCampoNumero.Text, out int numero) ? numero :
                    throw new Exception("Número do endereço informado é inválido! OBS: Informe apenas números inteiros");
                enderecoCliente.Complemento = txtCampoComplemento.Text;
                enderecoCliente.Cep = txtCampoCep.Text;
                enderecoCliente.UfEstado = !GetUfEstadoSelecionado().Equals("Selecione") && !string.IsNullOrEmpty(txtCampoCidade.Text) ? GetUfEstadoSelecionado() :
                    throw new Exception("Selecione a UF do estado do endereço!");
                enderecoCliente.Cidade = txtCampoCidade.Text;
                payloadEnvio.EnderecosCliente = new() { enderecoCliente };
                contatoCliente.EmailPrincipal = txtCampoEmail.Text;
                contatoCliente.EmailSecundario = txtCampoEmailSec.Text;
                contatoCliente.TelefoneCelular = txtCampoCelular.Text;
                contatoCliente.TelefoneFixo = txtCampoTelFixo.Text;
                payloadEnvio.ContatosCliente = new() { contatoCliente };

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/cliente";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResult(response, payloadEnvio.CpfCliente, true);
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                btnCadastrar.Visibility = Visibility.Visible;
                Loading.Spin = false;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private string GetRgSelecionado()
        {

            foreach (ComboBoxItem item in comboUfRg.Items)
            {
                if (item.IsSelected)
                {
                    return item.Content.ToString();
                }
            }
            return string.Empty;
        }

        private string GetUfEstadoSelecionado()
        {

            foreach (ComboBoxItem item in comboUfEstado.Items)
            {
                if (item.IsSelected)
                {
                    return item.Content.ToString();
                }
            }
            return string.Empty;
        }

        private async void CadastraDadosCliente(object sender, RoutedEventArgs e)
        {
            Loading.Visibility = Visibility.Visible;
            btnCadastrar.Visibility = Visibility.Hidden;
            Loading.Spin = true;
            await CadastrarCliente();
        }

        private async void EditarDadosCliente(object sender, RoutedEventArgs e)
        {
            Loading.Visibility = Visibility.Visible;
            btnEditar.Visibility = Visibility.Hidden;
            Loading.Spin = true;
            await EditarCliente();
        }

        private async Task<ClienteViewModel> EditarCliente()
        {
            var result = new ClienteViewModel();
            var payloadEnvio = new Cliente();
            var enderecoCliente = new EnderecoViewModel();
            var contatoCliente = new ContatoViewModel();
            try
            {
                payloadEnvio.NomeCliente = txtCampoNome.Text;
                payloadEnvio.NomeEmpresaCliente = txtCampoNomeEmp.Text;
                payloadEnvio.CpfCliente = txtCampoCpf.Text;
                payloadEnvio.CnpjCliente = txtCampoCNPJ.Text;
                payloadEnvio.SexoCliente = comboSexoFem.IsSelected ? 'F' : comboSexoMasc.IsSelected ? 'M' : 'I';
                payloadEnvio.RgCliente = txtCampoRg.Text;
                payloadEnvio.UfRg = GetRgSelecionado();
                payloadEnvio.DataNascimentoCliente = DateTime.TryParse(txtCampoDhNascimento.Text, out DateTime dhNascimento) ? dhNascimento :
                    throw new Exception("Data de nascimento informada é inválida!");
                enderecoCliente.Bairro = txtCampoBairro.Text;
                if (NumObrigatorio && string.IsNullOrEmpty(txtCampoNumero.Text))
                    throw new Exception("Obrigatório informar o número do endereço!");
                enderecoCliente.Numero = int.TryParse(txtCampoNumero.Text, out int numero) ? numero :
                    throw new Exception("Número do endereço informado é inválido! OBS: Informe apenas números inteiros");
                enderecoCliente.Complemento = txtCampoComplemento.Text;
                enderecoCliente.Cep = txtCampoCep.Text;
                enderecoCliente.UfEstado = !GetUfEstadoSelecionado().Equals("Selecione") && !string.IsNullOrEmpty(txtCampoCidade.Text) ? GetUfEstadoSelecionado() :
                    throw new Exception("Selecione a UF do estado do endereço!");
                enderecoCliente.Cidade = txtCampoCidade.Text;
                payloadEnvio.EnderecosCliente = new() { enderecoCliente };
                contatoCliente.EmailPrincipal = txtCampoEmail.Text;
                contatoCliente.EmailSecundario = txtCampoEmailSec.Text;
                contatoCliente.TelefoneCelular = txtCampoCelular.Text;
                contatoCliente.TelefoneFixo = txtCampoTelFixo.Text;
                payloadEnvio.ContatosCliente = new() { contatoCliente };

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/cliente/atualiza-cliente";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Put, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResult(response, payloadEnvio.CpfCliente, false);
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                btnEditar.Visibility = Visibility.Visible;
                Loading.Spin = false;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private async Task<ClienteViewModel> ExcluirCliente()
        {
            var result = new ClienteViewModel();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "cliente/excluir-cliente/" + cliente.IdCliente;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Put, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResult(response, string.Empty, false, true);
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                btnEditar.Visibility = Visibility.Visible;
                Loading.Spin = false;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }

        private async Task<ClienteViewModel> TratarResult(HttpResponseMessage response, string cpf,
            bool cadastroCliente, bool exclusaoCliente = false)
        {
            var result = new ClienteViewModel();
            var pedido = new Pedido();
            string word = cadastroCliente ? "cadastrar" : exclusaoCliente ? "excluir" : "editar";
            string msgException = $"Ocorreu um erro ao {word} dados do cliente.\n Tente novamente!";
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    if (cadastroCliente)
                    {
                        btnCadastrar.Visibility = Visibility.Visible;
                    }
                    else if (exclusaoCliente)
                    {
                        btnExcluir.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnEditar.Visibility = Visibility.Visible;
                    }
                    Loading.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<ClienteViewModel>(responseJson);
                    string msg = cadastroCliente ? "cadastrados" : exclusaoCliente ? "excluídos" : "editados";

                    MessageBox.Show($"Dados {msg} com sucesso!", "Informação cliente", MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    if (pedidoIniciado != null)
                    {
                        pedido = await VincularClientePedido(result.IdCliente);
                        var telaPedido = new CrudPedido(login, funcionario, "CLIENTE-VINCULO", pedido, true, result.NomeCliente);
                        telaPedido.Show();
                        Close();
                    }
                    else
                    {
                        var telaMenu = new MenuAdmin(login, funcionario);
                        telaMenu.Show();
                        Close();
                    }
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception(msgException);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    throw new Exception(msgException);

                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else
                {
                    throw new Exception(msgException);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
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

        private async void ExcluirCadastroCliente(object sender, RoutedEventArgs e)
        {
            Loading.Visibility = Visibility.Visible;
            btnEditar.Visibility = Visibility.Hidden;
            Loading.Spin = true;
            await ExcluirCliente();
        }

        private async void PesquisarCEP(object sender, RoutedEventArgs e)
        {
            btnPesquisar.Visibility = Visibility.Hidden;
            LoadingPesq.Visibility = Visibility.Visible;
            LoadingPesq.Spin = true;
            try
            {
                if (string.IsNullOrEmpty(txtCampoCep.Text))
                    throw new Exception("Obrigatório informar o CEP!");
                else
                {
                    await GetInfoCep();
                }
            }
            catch (Exception ex)
            {
                btnPesquisar.Visibility = Visibility.Visible;
                LoadingPesq.Visibility = Visibility.Hidden;
                LoadingPesq.Spin = false;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<CepCorreios> GetInfoCep()
        {
            var result = new CepCorreios();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                var cep = txtCampoCep.Text.Replace("-", "").Trim();
                string url = $"/{cep}/json/";
                var uri = new Uri("https://viacep.com.br/ws" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                result = await TratarResultCEP(response);
                Endereco endereco = new()
                {
                    Logradouro = result.Logradouro,
                    Cidade = result.Localidade,
                    UfEstado = result.Uf,
                    Complemento = result.Complemento,
                    Bairro = result.Bairro,
                };
                CarregarDadosRetornoCep(endereco);
                NumObrigatorio = true;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<CepCorreios> TratarResultCEP(HttpResponseMessage response)
        {
            var result = new CepCorreios();
            string msgException = "Ocorreu um erro ao buscar CEP! \nTente novamente ou insira os dados manualmente!";
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    btnPesquisar.Visibility = Visibility.Visible;
                    LoadingPesq.Visibility = Visibility.Hidden;
                    LoadingPesq.Spin = false;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<CepCorreios>(responseJson);
                    return result;
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception(msgException);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception(msgException);
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed
                    || response.StatusCode == HttpStatusCode.InternalServerError
                    || response.StatusCode == HttpStatusCode.BadRequest)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else
                {
                    throw new Exception(msgException);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void CarregarDadosRetornoCep(Endereco endereco)
        {
            txtCampoLogradouro.Text = endereco.Logradouro;
            txtCampoComplemento.Text = endereco.Complemento;
            txtCampoBairro.Text = endereco.Bairro;
            txtCampoCidade.Text = endereco.Cidade;
            VerificarUfCepCliente(endereco.UfEstado);
            txtCampoNumero.Text = "";
        }

        private void SetNumero(object sender, TextChangedEventArgs e)
        {
            NumObrigatorio = string.IsNullOrEmpty(txtCampoNumero.Text) && !string.IsNullOrEmpty(txtCampoCep.Text);
        }

        private void EditarPetSelecionado(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListPets.SelectedItems.Count > 1)
                {
                    throw new Exception("Não é possível editar mais de 1 pet!");
                }
                else if (ListPets.SelectedItems.Count == 0)
                {
                    throw new Exception("Selecione 1 pet que terá seu cadastro editado!");
                }
                else
                {
                    var telaCadastroPet = new CrudPetCliente(login, funcionario, "CLIENTE", cliente,
                        (PetViewModel)ListPets.SelectedItems[0], "EDICAO", Cadastrar, Editar, Excluir, pedidoIniciado);
                    telaCadastroPet.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AdicionarPetCliente(object sender, RoutedEventArgs e)
        {
            var telaCadastroPet = new CrudPetCliente(login, funcionario, "CLIENTE", cliente, null, "CADASTRO",
                Cadastrar, Editar, Excluir, pedidoIniciado);
            telaCadastroPet.Show();
            Close();
        }

        private async void ExcluirPetsSelecionados(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListPets.SelectedItems.Count == 0)
                {
                    throw new Exception("Selecione 1 Pet que terá seu cadastro excluído!");
                }
                else
                {
                    if (cliente != null && cliente.IdCliente > 0 && cliente.PetsCliente.Count > 0)
                    {
                        foreach (PetViewModel pet in ListPets.SelectedItems)
                        {
                            await ExcluirPet(cliente.IdCliente, pet.IdPet);
                        }
                    }
                    else
                    {
                        foreach (PetViewModel pet in ListPets.SelectedItems)
                        {
                            ListPets.Items.Remove(pet); //testar
                        }
                        //excluir pet adicionado no cadastro de cliente e pet vinculado 
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<int> ExcluirPet(int idCliente, int idPet)
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                var cep = txtCampoCep.Text.Replace("-", "").Trim();
                string url = $"/cliente/{idCliente}/exclui-pet/{idPet}";
                var uri = new Uri("https://viacep.com.br/ws" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                var result = await TratarResultPetDelete(response, idCliente, idPet);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> TratarResultPetDelete(HttpResponseMessage response,
            int idCliente, int idPet)
        {
            var result = new int();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<int>(responseJson);
                    MessageBox.Show($"Dados excluídos com sucesso!", "Informações Pet", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return result;

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception($"Ocorreu um erro ao excluír informações do pet! Tente novamente.");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    await ExcluirPet(idCliente, idPet);
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
