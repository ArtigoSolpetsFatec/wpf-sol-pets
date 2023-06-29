using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._7TelaInicioVenda
{
    /// <summary>
    /// Lógica interna para ModalQtdeProduto.xaml
    /// </summary>
    public partial class ModalQtdeProduto : Window
    {
        private Pedido pedido = new();
        private readonly LoginViewModel login = new();
        private readonly FuncionarioViewModel funcionario = new();
        private readonly bool finalizacao;
        private double totalPedido = 0.0;

        public ModalQtdeProduto(Pedido pedido, LoginViewModel login, FuncionarioViewModel funcionario, bool finalizacao = false, double totalPedido = 0.0)
        {
            InitializeComponent();
            this.pedido = pedido;
            this.login = login;
            this.funcionario = funcionario;
            this.finalizacao = finalizacao;
            this.totalPedido = totalPedido;
        }

        private void Voltar(object sender, RoutedEventArgs e)
        {
            var telaPedido = new CrudPedido(login, funcionario, "QTDE-PRODUTO", pedido, finalizacao);
            telaPedido.Show();
            DialogResult = false;
        }

        private async void EditarQuantidadeproduto(object sender, RoutedEventArgs e)
        {
            try
            {
                var qtdeInformada = int.TryParse(txtQtde.Text, out int qtde);
                if (!qtdeInformada)
                    throw new Exception("Informe um número inteiro!");
                else if (!finalizacao)
                {
                    var produtoSelected = pedido.Produtos.Find(_ => (bool)_.Selected);
                    if (qtde == 0)
                    {
                        var excluirProduto = MessageBox.Show("Ao informar 0 o produto será excluído. \nDeseja continuar?"
                            , "Informativo", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (excluirProduto == MessageBoxResult.OK)
                        {
                            pedido.Produtos.Remove(produtoSelected);
                        }
                        else
                            return;
                    }

                    if (qtde <= produtoSelected.QtdeEstoque)
                    {
                        produtoSelected.Selected = null;
                        produtoSelected.QtdeProduto = qtde;
                    }
                    else if (qtde > produtoSelected.QtdeEstoque)
                    {
                        throw new Exception($"Quantidade informada deve ser menor ou igual a quantidade em estoque atual ({produtoSelected.QtdeEstoque})");
                    }

                    var telaPedido = new CrudPedido(login, funcionario, "QTDE-PRODUTO", pedido, finalizacao);
                    telaPedido.Show();
                    Close();
                }
                else if (finalizacao)
                {
                    var produtoSelected = pedido.Pedidos.Find(_ => _.Produto.Selected.GetValueOrDefault());
                    if (qtde == 0)
                    {
                        var msg = pedido.Pedidos.Count == 1 ? "Ao informar 0 o produto será excluído e o pedido cancelado. \nDeseja continuar?"
                            : "Ao informar 0 o produto será excluído. \nDeseja continuar?";
                        var excluirProduto = MessageBox.Show(msg, "Informativo", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (excluirProduto == MessageBoxResult.OK && pedido.Pedidos.Count > 1)
                        {
                            totalPedido = SomaTotalPedido();
                            await RemoveProdutoPedido(pedido.IdPedido, produtoSelected.Produto.IdProduto, (int)produtoSelected.Produto.QtdeProduto, totalPedido);
                        }
                        else if (excluirProduto == MessageBoxResult.OK && pedido.Pedidos.Count == 1)
                        {
                            CancelarPedido();
                            return;
                        }
                        else
                            return;
                    }

                    else if (qtde <= produtoSelected.Produto.QtdeEstoque)
                    {
                        produtoSelected.Produto.Selected = null;
                        produtoSelected.Produto.QtdeProduto = qtde;
                        totalPedido = SomaTotalPedido();
                        await EditaQtdeProduto(pedido.IdPedido, produtoSelected.Produto.IdProduto, (int)produtoSelected.Produto.QtdeProduto, totalPedido);
                    }
                    else if (qtde > produtoSelected.Produto.QtdeEstoque)
                    {
                        throw new Exception($"Quantidade informada deve ser menor ou igual a quantidade em estoque atual ({produtoSelected.Produto.QtdeEstoque})");
                    }

                    var telaPedido = new CrudPedido(login, funcionario, "QTDE-PRODUTO", pedido, finalizacao);
                    telaPedido.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Pedido> RemoveProdutoPedido(int idPedido, int idProduto, int qtdeProduto, double totalPedido)
        {
            var result = new Pedido();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/pedido/remove-produto/idPedido/{idPedido}/idProduto/{idProduto}/qtdeProduto/{qtdeProduto}/totalVenda/{totalPedido}";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Patch, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResultPedido(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private async Task<Pedido> EditaQtdeProduto(int idPedido, int idProduto, int qtdeProduto, double totalPedido)
        {
            var result = new Pedido();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/pedido/atualiza-qtde-produto/idPedido/{idPedido}/idProduto/{idProduto}/qtdeProduto/{qtdeProduto}/totalVenda/{totalPedido}";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Patch, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                result = await TratarResultPedido(response, true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private async Task<Pedido> TratarResultPedido(HttpResponseMessage response, bool edicaoPedido = false)
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
                    string msg = edicaoPedido ? "editar" : "remover";
                    throw new Exception($"Ocorreu um erro ao {msg} produto(s) do pedido!");
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    string msg = edicaoPedido ? "editar" : "remover";
                    throw new Exception($"Ocorreu um erro ao {msg} produto(s) do pedido!. Tente novamente!");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private async void CancelarPedido()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/pedido/cancelar-pedido/idPedido/{pedido.IdPedido}";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Patch, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                await TratarResultFinalizacaoPedido(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Pedido> TratarResultFinalizacaoPedido(HttpResponseMessage response)
        {
            var result = new Pedido();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Pedido cancelado com sucesso!",
                        "Informação pedido", MessageBoxButton.OK, MessageBoxImage.Information);

                    var telaMenu = new MenuAdmin(login, funcionario);
                    telaMenu.Show();
                    Close();
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    var responseUsua = MessageBox.Show($"Ocorreu um erro ao cancelar o pedido! \nPor favor, tente novamente",
                        $"Cancelar pedido", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    CancelarPedido();
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
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
            return (double)total;
        }
    }
}
