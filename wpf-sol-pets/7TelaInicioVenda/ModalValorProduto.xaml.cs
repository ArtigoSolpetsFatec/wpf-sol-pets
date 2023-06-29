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
    /// Lógica interna para ModalValorProduto.xaml
    /// </summary>
    public partial class ModalValorProduto : Window
    {
        private Pedido pedido = new();
        private readonly FuncionarioViewModel funcionario = new();
        private readonly LoginViewModel login = new();
        private readonly bool finalizacao;
        private readonly int idProduto;
        private readonly bool somaValorproduto;
        private readonly double valorDesconto;

        public ModalValorProduto(LoginViewModel login, FuncionarioViewModel funcionario, Pedido pedido,
            bool finalizacao, int idProduto, bool somaValorproduto = false,double valorDesconto = 0.0)
        {
            InitializeComponent();
            this.pedido = pedido;
            this.funcionario = funcionario;
            this.login = login;
            this.finalizacao = finalizacao;
            this.idProduto = idProduto;
            this.somaValorproduto = somaValorproduto;
            this.valorDesconto = valorDesconto;
        }

        private void VoltarTelaAnterior(object sender = null, RoutedEventArgs e = null)
        {
            var telaPedido = new CrudPedido(login, funcionario, "VALOR-PRODUTO", pedido, finalizacao, null, 0.0, idProduto);
            telaPedido.Show();
            Close();
        }

        private async void OnChangeValorProduto(object sender, RoutedEventArgs e)
        {
            var result = new Pedido();
            try
            {
                if (double.TryParse(txtVlrProduto.Text, out double valorProduto))
                {
                    OnChangeValorProduto(valorProduto);
                }
                else
                    throw new Exception("Informe um número decimal para o valor do produto. \nEx: 5.0, 12.50");
                if (finalizacao)
                {
                    var objTokenClient = await GeneralExtensions.GetToken();
                    var token = objTokenClient.token;
                    var client = objTokenClient.client;
                    var totalVenda = SomaTotalPedido();
                    string url = $"/pedido/atualiza-valor-produto/idPedido/{pedido.IdPedido}/idProduto/{idProduto}/totalVenda/{totalVenda}/valorProduto/{valorProduto}";
                    var uri = new Uri("http://localhost:64967" + url);
                    HttpRequestMessage request = new(HttpMethod.Patch, url);
                    request.RequestUri = uri;
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                    result = await TratarResultPedido(response);
                    if (result.IdPedido > 0)
                        VoltarTelaAnterior();
                }
                else
                {
                    VoltarTelaAnterior();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnChangeValorProduto(double valorProduto)
        {
            if (pedido?.Produtos?.Count > 0)
            {
                foreach (var produto in pedido.Produtos)
                {
                    if (produto.IdProduto == idProduto && !somaValorproduto)
                    {
                        produto.ValorUnitarioVenda = valorProduto;
                        produto.ValorProdutoPedido = valorProduto;
                    }
                    else if (produto.IdProduto == idProduto && somaValorproduto)
                    {
                        produto.ValorUnitarioVenda += valorProduto;
                        produto.ValorProdutoPedido += valorProduto;
                    }
                }
            }
            else if (pedido.Pedidos.Count > 0)
            {
                foreach (var pedidoProduto in pedido.Pedidos)
                {
                    if (pedidoProduto.Produto.IdProduto == idProduto && !somaValorproduto)
                    {
                        pedidoProduto.Produto.ValorUnitarioVenda = valorProduto;
                        pedidoProduto.Produto.ValorProdutoPedido = valorProduto;
                    }
                    else if (pedidoProduto.Produto.IdProduto == idProduto && somaValorproduto)
                    {
                        pedidoProduto.Produto.ValorUnitarioVenda += valorProduto;
                        pedidoProduto.Produto.ValorProdutoPedido += valorProduto;
                    }
                }
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
                    throw new Exception($"Ocorreu um erro ao salvar valor do produto no pedido. Tente Novamente!");
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    throw new Exception($"Ocorreu um erro ao salvar valor do produto no pedido. Tente Novamente!");
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

        private void FecharJanela(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //var responseUsua = MessageBox.Show("Deseja realmente fechar a janela?\nAo fechar a janela, o programa será finalizado!",
            //    "Atenção", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            //if (responseUsua.Equals(MessageBoxResult.Yes))
            //    e.Cancel = false;
            //else
            //    e.Cancel = true;
        }
    }
}
