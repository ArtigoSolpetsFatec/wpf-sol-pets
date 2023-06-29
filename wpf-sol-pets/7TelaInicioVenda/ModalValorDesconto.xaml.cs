using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._7TelaInicioVenda
{
    /// <summary>
    /// Lógica interna para ModalValorDesconto.xaml
    /// </summary>
    public partial class ModalValorDesconto : Window
    {
        private Pedido pedido;
        private readonly LoginViewModel login;
        private readonly FuncionarioViewModel funcionario;
        private readonly bool finalizacao;
        private readonly double totalPedido;
        private readonly string nomeCliente;
        private double valorDesconto;

        public ModalValorDesconto(LoginViewModel login, FuncionarioViewModel funcionario, Pedido pedido,
            bool finalizacao, double totalPedido, double valorDesconto = 0.0, string nomeCliente = null)
        {
            InitializeComponent();
            this.login = login;
            this.funcionario = funcionario;
            this.pedido = pedido;
            this.finalizacao = finalizacao;
            this.totalPedido = totalPedido;
            this.valorDesconto = valorDesconto;
            this.nomeCliente = nomeCliente;
        }

        private async void AplicaDescontoInformado(object sender = null, RoutedEventArgs e = null)
        {
            var result = new Pedido();
            try
            {
                if (double.TryParse(txtPorcentagem.Text, out double porcentagemDesconto))
                {
                    valorDesconto += totalPedido * (porcentagemDesconto / 100);
                }
                else
                    throw new Exception("Informe um número decimal para porcentagem de desconto. \nEx: 5.0, 12.5");
                if (pedido.IdPedido > 0)
                {
                    var objTokenClient = await GeneralExtensions.GetToken();
                    var token = objTokenClient.token;
                    var client = objTokenClient.client;
                    var totalVenda = SomaTotalPedido();
                    string url = $"/pedido/atualiza-total/idPedido/{pedido.IdPedido}/totalVenda/{totalVenda}/valorDesconto/{valorDesconto}";
                    var uri = new Uri("http://localhost:64967" + url);
                    HttpRequestMessage request = new(HttpMethod.Patch, url);
                    request.RequestUri = uri;
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                    result = await TratarResultPedido(response);
                    if (result.IdPedido > 0)
                        VoltaTelaAnterior();
                }
                else
                {
                    VoltaTelaAnterior();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Pedido> TratarResultPedido(HttpResponseMessage response)
        {
            var result = new Pedido();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<Pedido>(responseJson);
                    pedido = result;
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception($"Ocorreu um erro ao aplicar desconto no pedido. Tente Novamente!");
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    throw new Exception($"Ocorreu um erro ao aplicar desconto no pedido. Tente Novamente!");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private double SomaTotalPedido()
        {
            double? total = 0.0;
            if (pedido?.Produtos?.Count > 0)
            {
                foreach (var produto in pedido.Produtos)
                {
                    var valorPedidoProduto = produto.ValorUnitarioVenda * produto.QtdeProduto;
                    total += valorPedidoProduto;
                }
            }
            else if (pedido.Pedidos.Count > 0)
            {
                foreach (var pedidoProduto in pedido.Pedidos)
                {
                    var valorPedidoProduto = pedidoProduto.Produto.ValorUnitarioVenda * pedidoProduto.Produto.QtdeProduto;
                    total += valorPedidoProduto;
                }
            }
            total = valorDesconto > 0.0 ? total - valorDesconto : total;
            return (double)total;
        }

        private void VoltaTelaAnterior(object sender = null, RoutedEventArgs e = null)
        {
            var telaPedido = new CrudPedido(login, funcionario, "MODAL-DESCONTO", pedido, finalizacao,
                nomeCliente, valorDesconto);
            telaPedido.Show();
            Close();
        }
    }
}
