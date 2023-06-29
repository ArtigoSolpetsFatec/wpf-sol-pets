using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets._3TelasBusca._3._1BuscarCliente;
using wpf_sol_pets._5TelaCrudProduto;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._7TelaInicioVenda
{
    /// <summary>
    /// Lógica interna para CrudPedido.xaml
    /// </summary>
    public partial class CrudPedido : Window
    {
        private Pedido pedido = new();
        private readonly FuncionarioViewModel funcionario = new();
        private readonly string telaAnterior = string.Empty;
        private readonly LoginViewModel login = new();
        private bool finalizacao;
        private readonly string nomeCliente = string.Empty;
        private bool pesqAutomaticaFinalizada = false;
        private double valorDesconto;
        private int idProdutoValorPorQuilo;

        public CrudPedido(LoginViewModel login, FuncionarioViewModel funcionario, string telaAnterior,
            Pedido pedido = null, bool finalizacao = false, string nomeCliente = null, double valorDesconto = 0.0,
            int idProdutoValorPorQuilo = 0)
        {
            InitializeComponent();
            this.login = login;
            this.funcionario = funcionario;
            this.telaAnterior = telaAnterior;
            this.pedido = pedido;
            this.finalizacao = finalizacao;
            this.nomeCliente = nomeCliente;
            this.valorDesconto = valorDesconto;
            this.idProdutoValorPorQuilo = idProdutoValorPorQuilo;
            GetPedidosCache();
        }

        private async void GetPedidosCache()
        {
            if (finalizacao)
            {
                btnAvancar.Visibility = Visibility.Hidden;
                btnFinalizar.Visibility = Visibility.Visible;
                btnVoltar.Visibility = Visibility.Hidden;
                btnCancelar.Visibility = Visibility.Visible;
            }
            else
            {
                btnAvancar.Visibility = Visibility.Visible;
                btnFinalizar.Visibility = Visibility.Hidden;
                btnVoltar.Visibility = Visibility.Visible;
                btnCancelar.Visibility = Visibility.Hidden;
            }
            if (!string.IsNullOrEmpty(nomeCliente))
            {
                infoCliente.Visibility = Visibility.Visible;
                txtNomeCliente.Text = nomeCliente;
            }

            if (pedido != null && pedido.Produtos?.Count > 0)
            {
                ListProdutos.ItemsSource = null;
                ListProdutos.ItemsSource = pedido.Produtos;
            }
            else if (pedido?.Pedidos?.Count > 0)
            {
                btnEditar.IsEnabled = true;
                btnExcluir.IsEnabled = true;
                btnAvancar.IsEnabled = true;
                btnEditar.ToolTip = "Edita quantidade do produto selecionado";
                btnExcluir.ToolTip = "Exclui o(s) produto(s) selecionado(s)";
                GetPedidoAFinalizar();
                SomaTotalPedido();
            }
            else
            {
                pedido = new();
                pedido.Produtos = new();
            }
            if (idProdutoValorPorQuilo > 0)
            {
                var totalVenda = SomaTotalPedido();
                var produtoPrecoVariavel = pedido.Produtos.Find(_ => _.IdProduto == idProdutoValorPorQuilo);
                if (pedido.IdPedido > 0)
                {
                    var resultPedido = await AdicionaProdutoPedido(idProdutoValorPorQuilo, totalVenda, 1, produtoPrecoVariavel.ValorProdutoPedido);
                    pedido = resultPedido;
                }
                idProdutoValorPorQuilo = 0;
            }
            HabilitaDesabilitaBotoes();
        }

        private void HabilitaDesabilitaBotoes()
        {
            if (pedido?.Produtos?.Count == 0)
            {
                btnEditar.IsEnabled = false;
                btnExcluir.IsEnabled = false;
                btnAvancar.IsEnabled = false;
                btnEditarVlrProduto.IsEnabled = false;
                btnAppDesconto.IsEnabled = false;
                btnEditar.ToolTip = "Não há itens no pedido";
                btnExcluir.ToolTip = "Não há itens no pedido";
                txtTotal.Text = "";
            }
            else
            {
                btnEditar.IsEnabled = true;
                btnExcluir.IsEnabled = true;
                btnAvancar.IsEnabled = true;
                btnEditarVlrProduto.IsEnabled = true;
                btnAppDesconto.IsEnabled = true;
                btnEditar.ToolTip = "Edita quantidade do produto selecionado";
                btnExcluir.ToolTip = "Exclui o(s) produto(s) selecionado(s)";
                SomaTotalPedido();
            }
        }

        private void GetPedidoAFinalizar()
        {
            foreach (var pedidoProduto in pedido.Pedidos)
            {
                pedidoProduto.Produto.QtdeProduto = pedidoProduto.QtdeProdutos;
                if (pedidoProduto.Produto.ValorUnitarioVenda == 0)
                    pedidoProduto.Produto.ValorUnitarioVenda = pedidoProduto.Produto.ValorProdutoPedido;
                pedido.Produtos.Add(pedidoProduto.Produto);
            }
            ListProdutos.ItemsSource = null;
            ListProdutos.ItemsSource = pedido.Produtos;
        }

        private double SomaTotalPedido()
        {
            double? total = 0.0;
            if (pedido?.Produtos?.Count > 0)
            {
                foreach (var produto in pedido.Produtos)
                {
                    var valorPedidoProduto = produto.ValorProdutoPedido == 0 ?
                        produto.ValorUnitarioVenda * produto.QtdeProduto : produto.ValorUnitarioVenda;
                    total += valorPedidoProduto;
                }
            }
            total = valorDesconto > 0.0 ? total - valorDesconto : total;
            txtTotal.Text = total?.ToString("F");
            txtTotalDesconto.Text = valorDesconto > 0.0 ? valorDesconto.ToString("F") : string.Empty;
            return (double)total;
        }

        private int SomaQtdePedido()
        {
            int? qtdeProdutos = 0;
            if (pedido.Produtos.Count > 0)
            {
                foreach (var produto in pedido.Produtos)
                {
                    qtdeProdutos += produto.QtdeProduto;
                }
            }
            return (int)qtdeProdutos;
        }

        private async Task<Pedido> IniciaVenda()
        {
            var result = new Pedido();
            var payloadEnvio = new Pedido();
            try
            {
                payloadEnvio.StatusVenda = 'I';
                payloadEnvio.DhVenda = DateTime.Now;
                payloadEnvio.Entregar = false;
                payloadEnvio.TotalVenda = SomaTotalPedido();
                payloadEnvio.QtdeProdutos = SomaQtdePedido();
                payloadEnvio.Finalizado = false;
                payloadEnvio.Produtos = pedido.Produtos;

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/pedido";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
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

        private async Task<Pedido> TratarResultPedido(HttpResponseMessage response, bool removeuProduto = false, bool edicaoQtdeProd = false)
        {
            var result = new Pedido();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    LoadingAvancar.Visibility = Visibility.Hidden;
                    LoadingAvancar.Spin = false;
                    btnAvancar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<Pedido>(responseJson);
                    pedido = result;
                    if (!removeuProduto && !edicaoQtdeProd)
                    {
                        var responseUsua = MessageBox.Show("Venda iniciada! \nDeseja vincular o cliente através de seu CPF? \n**Aconselhável para ações de marketing**",
                       "Informação Venda", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (responseUsua.Equals(MessageBoxResult.OK))
                        {
                            var telaBuscaCliente = new BuscarCliente(login, funcionario, "TELA-PEDIDOS", result);
                            telaBuscaCliente.Show();
                            Close();
                        }
                        else
                        {
                            finalizacao = true;
                            GetPedidosCache();
                        }
                    }
                    else
                    {
                        finalizacao = true;
                        GetPedidosCache();
                    }

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    string msg = !removeuProduto && !edicaoQtdeProd ? "Ocorreu um erro ao cadastrar o pedido!" : !removeuProduto && edicaoQtdeProd ? "Ocorreu um erro ao adicionar produto ao pedido!" :
                        "Ocorreu um erro ao remover produto(s) do pedido!";
                    throw new Exception(msg);
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    string msg = !removeuProduto && !edicaoQtdeProd ? "Ocorreu um erro ao cadastrar o pedido. Tente novamente!" : !removeuProduto && edicaoQtdeProd ?
                        "Ocorreu um erro ao adicionar produto ao pedido. Tente novamente!" : "Ocorreu um erro ao remover produto(s) do pedido!. Tente novamente!";
                    throw new Exception(msg);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private void ValidaEstoqueProduto(ProdutoViewModel produto)
        {
            try
            {
                if (produto.QtdeEstoque == 0)
                    throw new Exception("Produto com estoque indisponível!");
                else if (produto.QtdeEstoque == produto.QtdeProduto)
                    throw new Exception("Não é possível adicionar mais quantidades desse produto devido a seu estoque!");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async void AdicionarProduto(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                Loading.Visibility = Visibility.Visible;
                btnAdicionar.Visibility = Visibility.Hidden;
                Loading.Spin = true;
                if (pedido.Produtos.Count > 0 && !finalizacao)
                {
                    foreach (var produto in pedido.Produtos.ToList())
                    {
                        var hasProduto = pedido.Produtos.Any(_ => _.IsbnProduto.Equals(txtPesquisaprod.Text));
                        if (comboIsbn.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && !hasProduto)
                        {
                            await GetProdutoByIsbn();
                        }
                        else if (comboIsbn.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && produto.IsbnProduto.Equals(txtPesquisaprod.Text) && hasProduto)
                        {
                            Loading.Visibility = Visibility.Hidden;
                            btnAdicionar.Visibility = Visibility.Visible;
                            Loading.Spin = false;
                            ValidaEstoqueProduto(produto);
                            produto.QtdeProduto++;
                            if (produto.ValorProdutoPedido > 0)
                            {
                                var modalValorProduto = new ModalValorProduto(login, funcionario, pedido, finalizacao,
                                    produto.IdProduto, true);
                                modalValorProduto.Owner = this;
                                modalValorProduto.ShowDialog();
                                Close();
                            }
                        }
                        else if (comboNome.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && !hasProduto)
                        {
                            await GetProdutoByNome();
                        }
                        else if (comboNome.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && produto.NomeProduto.Trim().ToUpper().Equals(txtPesquisaprod.Text)
                            && hasProduto)
                        {
                            Loading.Visibility = Visibility.Hidden;
                            btnAdicionar.Visibility = Visibility.Visible;
                            Loading.Spin = false;
                            ValidaEstoqueProduto(produto);
                            produto.QtdeProduto++;
                        }
                        else if (string.IsNullOrEmpty(txtPesquisaprod.Text) && !pesqAutomaticaFinalizada)
                        {
                            throw new Exception("Obrigatório preencher o campo de pesquisa!");
                        }
                    }
                    SetListaProdutos();
                }
                else if (pedido.Pedidos?.Count > 0 && finalizacao)
                {

                    foreach (var pedidoProduto in pedido.Pedidos.ToList())
                    {
                        var hasProdutoPedido = pedido.Pedidos.Any(_ => _.Produto.IsbnProduto.Equals(txtPesquisaprod.Text));

                        if (comboIsbn.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && !hasProdutoPedido)
                        {
                            await GetProdutoByIsbn();
                        }
                        else if (comboIsbn.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && pedidoProduto.Produto.IsbnProduto.Equals(txtPesquisaprod.Text) && hasProdutoPedido)
                        {
                            Loading.Visibility = Visibility.Hidden;
                            btnAdicionar.Visibility = Visibility.Visible;
                            Loading.Spin = false;
                            ValidaEstoqueProduto(pedidoProduto.Produto);
                            pedidoProduto.Produto.QtdeProduto++;
                            var totalVenda = SomaTotalPedido();
                            await EditaQtdeProduto(pedido.IdPedido, pedidoProduto.Produto.IdProduto, (int)pedidoProduto.Produto.QtdeProduto, totalVenda);
                        }
                        else if (comboNome.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && !hasProdutoPedido)
                        {
                            await GetProdutoByNome();
                        }
                        else if (comboNome.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text)
                            && pedidoProduto.Produto.NomeProduto.Trim().ToUpper().Equals(txtPesquisaprod.Text)
                            && hasProdutoPedido)
                        {
                            Loading.Visibility = Visibility.Hidden;
                            btnAdicionar.Visibility = Visibility.Visible;
                            Loading.Spin = false;
                            ValidaEstoqueProduto(pedidoProduto.Produto);
                            pedidoProduto.Produto.QtdeProduto++;
                        }
                        else if (string.IsNullOrEmpty(txtPesquisaprod.Text) && !pesqAutomaticaFinalizada)
                        {
                            throw new Exception("Obrigatório preencher o campo de pesquisa!");
                        }
                    }
                    SetListaProdutos();
                }
                else
                {
                    if (comboIsbn.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text))
                    {
                        await GetProdutoByIsbn();
                    }
                    else if (comboNome.IsSelected && !string.IsNullOrEmpty(txtPesquisaprod.Text))
                    {
                        await GetProdutoByNome();
                    }
                    else if (string.IsNullOrEmpty(txtPesquisaprod.Text) && !pesqAutomaticaFinalizada)
                    {
                        throw new Exception("Obrigatório preencher o campo de pesquisa!");
                    }
                    SetListaProdutos();
                }
            }
            catch (Exception ex)
            {
                Loading.Spin = false;
                Loading.Visibility = Visibility.Hidden;
                btnAdicionar.Visibility = Visibility.Visible;
                txtPesquisaprod.Text = "";
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                result = await TratarResultPedido(response, false, true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private void SetListaProdutos()
        {
            ListProdutos.ItemsSource = null;
            var produtosSemDup = pedido.Produtos.DistinctBy(_ => _.IdProduto);
            pedido.Produtos = produtosSemDup.ToList();
            ListProdutos.ItemsSource = pedido.Produtos.DistinctBy(_ => _.IdProduto);
            HabilitaDesabilitaBotoes();
            txtPesquisaprod.Text = "";
        }

        private async Task<ProdutoViewModel> GetProdutoByIsbn()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/produto/isbn/" + txtPesquisaprod.Text;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                var objPesquisa = new { tipo = "isbn", valor = txtPesquisaprod.Text };
                var result = await TratarResult(response, "isbn", objPesquisa);
                if (result.IdProduto > 0 && !finalizacao && result.ValorUnitarioVenda == 0.0)
                {
                    ValidaEstoqueProduto(result);
                    result.QtdeProduto = 1;
                    result.ValorVendaEditavel = true;
                    pedido.Produtos.Add(result);
                    ListProdutos.ItemsSource = null;
                    ListProdutos.ItemsSource = pedido.Produtos;
                    HabilitaDesabilitaBotoes();
                    var telaValorProduto = new ModalValorProduto(login, funcionario, pedido, finalizacao, result.IdProduto);
                    telaValorProduto.Owner = this;
                    telaValorProduto.ShowDialog();
                    Close();
                }
                else if (result.IdProduto > 0 && finalizacao && result.ValorUnitarioVenda == 0.0)
                {
                    ValidaEstoqueProduto(result);
                    result.QtdeProduto = 1;
                    result.ValorVendaEditavel = true;
                    pedido.Produtos.Add(result);
                    ListProdutos.ItemsSource = null;
                    ListProdutos.ItemsSource = pedido.Produtos;
                    var telaValorProduto = new ModalValorProduto(login, funcionario, pedido, finalizacao, result.IdProduto);
                    telaValorProduto.Owner = this;
                    telaValorProduto.Show();
                    Close();
                }
                else if (result.IdProduto > 0 && !finalizacao && result.ValorUnitarioVenda > 0.0)
                {
                    ValidaEstoqueProduto(result);
                    result.QtdeProduto = 1;
                    pedido.Produtos.Add(result);
                    ListProdutos.ItemsSource = null;
                    ListProdutos.ItemsSource = pedido.Produtos;
                    HabilitaDesabilitaBotoes();
                }
                else if (result.IdProduto > 0 && finalizacao && result.ValorUnitarioVenda > 0.0)
                {
                    ValidaEstoqueProduto(result);
                    result.QtdeProduto = 1;
                    pedido.Produtos.Add(result);
                    ListProdutos.ItemsSource = null;
                    ListProdutos.ItemsSource = pedido.Produtos;
                    var totalVenda = SomaTotalPedido();
                    var resultPedido = await AdicionaProdutoPedido(result.IdProduto, totalVenda, (int)result.QtdeProduto);
                    pedido = resultPedido;
                    GetPedidoAFinalizar();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<Pedido> AdicionaProdutoPedido(int idProduto, double totalVenda, int qtdeProduto, double valorProdutoPedido = 0.0)
        {
            var payloadEnvio = new PedidoProdutoViewModel();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                payloadEnvio.Finalizado = false;
                payloadEnvio.IdPedido = pedido.IdPedido;
                payloadEnvio.Produto.IdProduto = idProduto;
                payloadEnvio.QtdeProdutos = qtdeProduto;
                payloadEnvio.TotalVenda = totalVenda;
                payloadEnvio.Produto.ValorProdutoPedido = valorProdutoPedido;

                string url = $"/pedido/adiciona-produto";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Patch, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                var result = await TratarResultAddProdPedido(response, idProduto, totalVenda, qtdeProduto);
                if (result.IdPedido > 0)
                {
                    pedido = result;
                    GetPedidosCache();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<Pedido> TratarResultAddProdPedido(HttpResponseMessage response, int idProduto, double totalVenda, int qtdeProduto)
        {
            var result = new Pedido();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnAdicionar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<Pedido>(responseJson);

                    return result;
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception("Ocorreu um erro ao adicionar produto ao pedido. Tente novamente!");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnAdicionar.Visibility = Visibility.Visible;
                    GeneralExtensions.TokenView = "";
                    await AdicionaProdutoPedido(idProduto, totalVenda, qtdeProduto);
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else
                {
                    string messageError = "Ocorreu um erro ao buscar o produto. Tente novamente!";
                    throw new Exception(messageError);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private async Task<ProdutoViewModel> GetProdutoByNome()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = "/produtos/nomeProduto/" + txtPesquisaprod.Text;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                var objPesquisa = new { tipo = "isbn", valor = txtPesquisaprod.Text };
                var result = await TratarResult(response, "nome", objPesquisa);
                if (result.IdProduto > 0)
                {
                    ValidaEstoqueProduto(result);
                    result.QtdeProduto = 1;
                    pedido.Produtos.Add(result);
                    ListProdutos.ItemsSource = null;
                    ListProdutos.ItemsSource = pedido.Produtos;
                    HabilitaDesabilitaBotoes();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<ProdutoViewModel> TratarResult(HttpResponseMessage response,
            string tipoRequisicao, dynamic objPesquisa)
        {
            var result = new ProdutoViewModel();
            var resultByNome = new List<ProdutoViewModel>();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnAdicionar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (tipoRequisicao.Equals("isbn"))
                        result = JsonConvert.DeserializeObject<ProdutoViewModel>(responseJson);
                    else
                    {
                        resultByNome = JsonConvert.DeserializeObject<List<ProdutoViewModel>>(responseJson);
                        result = resultByNome[0];
                    }

                    return result;
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnAdicionar.Visibility = Visibility.Visible;
                    var responseUsua = MessageBox.Show("produto não encontrado! \nDeseja realizar seu cadastro?",
                        "Informação Produto", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (responseUsua.Equals(MessageBoxResult.OK))
                    {
                        var telaCrudProduto = new CrudProduto(login, funcionario, "TELA-PEDIDOS", "cadastrar", objPesquisa, pedido, finalizacao);
                        telaCrudProduto.Show();
                        Close();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Loading.Spin = false;
                    Loading.Visibility = Visibility.Hidden;
                    btnAdicionar.Visibility = Visibility.Visible;
                    GeneralExtensions.TokenView = "";
                    if (tipoRequisicao.Equals("isbn"))
                        await GetProdutoByIsbn();
                    else
                        await GetProdutoByNome();

                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else
                {
                    string messageError = "Ocorreu um erro ao buscar o produto. Tente novamente!";
                    throw new Exception(messageError);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private void EditarQuantidadeProduto(object sender, RoutedEventArgs e)
        {
            if (!finalizacao)
            {
                if (ListProdutos.SelectedItems.Count > 1 || ListProdutos.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Selecione 1 produto", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    foreach (var produto in pedido.Produtos)
                    {
                        foreach (var produtoSelected in ListProdutos.SelectedItems)
                        {
                            produto.Selected = produto.Equals(produtoSelected);
                        }
                    }
                    var modalQtde = new ModalQtdeProduto(pedido, login, funcionario);
                    modalQtde.Owner = this;
                    modalQtde.ShowDialog();
                    Close();
                }
            }
            else
            {
                if (ListProdutos.SelectedItems.Count > 1 || ListProdutos.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Selecione 1 produto", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    foreach (var pedido in pedido.Pedidos)
                    {
                        foreach (ProdutoViewModel produtoSelected in ListProdutos.SelectedItems)
                        {
                            pedido.Produto.Selected = pedido.Produto.IdProduto == produtoSelected.IdProduto;
                        }
                    }
                    var modalQtde = new ModalQtdeProduto(pedido, login, funcionario, true);
                    modalQtde.Owner = this;
                    modalQtde.ShowDialog();
                    Close();
                }
            }
        }

        private async void AvancaProximaTela(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingAvancar.Visibility = Visibility.Visible;
                LoadingAvancar.Spin = true;
                btnAvancar.Visibility = Visibility.Hidden;
                await IniciaVenda();
            }
            catch (Exception ex)
            {
                LoadingAvancar.Visibility = Visibility.Hidden;
                LoadingAvancar.Spin = false;
                btnAvancar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VoltarTelaAnterior(object sender, RoutedEventArgs e)
        {
            var telaMenu = new MenuAdmin(login, funcionario);
            telaMenu.Show();
            Close();
        }

        private async void ExcluiProdutosSelecionados(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListProdutos.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Selecione ao menos 1 produto a ser excluído", "Erro"
                        , MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (!finalizacao)
                {
                    ValidaQtdeProdutosSelecionados();
                    var qtdeProdRemovidos = pedido.Produtos.RemoveAll(_ => (bool)_.Selected);
                    MessageBox.Show($"{qtdeProdRemovidos} produto(s) removido(s)", "Informação"
                        , MessageBoxButton.OK, MessageBoxImage.Information);
                    ListProdutos.ItemsSource = null;
                    ListProdutos.ItemsSource = pedido.Produtos;
                    HabilitaDesabilitaBotoes();
                }
                else
                {
                    int qtdeProdRemovidos = 0;
                    ValidaQtdeProdutosSelecionados();
                    foreach (var pedidoProduto in pedido?.Pedidos)
                    {
                        if (pedidoProduto.Produto.Selected.GetValueOrDefault())
                        {
                            pedido = await RemoveProdutoPedido(pedido.IdPedido, pedidoProduto.Produto.IdProduto, (int)pedidoProduto.Produto.QtdeProduto);
                            qtdeProdRemovidos++;
                        }
                    }

                    MessageBox.Show($"{qtdeProdRemovidos} produto(s) removido(s)", "Informação"
                        , MessageBoxButton.OK, MessageBoxImage.Information);
                    ListProdutos.ItemsSource = null;
                    ListProdutos.ItemsSource = pedido.Produtos;
                    HabilitaDesabilitaBotoes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidaQtdeProdutosSelecionados()
        {
            if (ListProdutos.SelectedItems.Count != pedido.Produtos.Count && !finalizacao)
            {
                foreach (var produtoOfList in ListProdutos.SelectedItems)
                {
                    foreach (var produtoOfPedido in pedido.Produtos)
                    {
                        if (produtoOfList.Equals(produtoOfPedido))
                            produtoOfPedido.Selected = true;
                        else if (produtoOfPedido.Selected == null)
                            produtoOfPedido.Selected = false;
                    }
                }
            }
            else if (!finalizacao)
            {
                foreach (var produto in pedido.Produtos)
                {
                    produto.Selected = true;
                }
            }
            else if (ListProdutos.SelectedItems.Count != pedido.Produtos.Count && finalizacao)
            {
                foreach (ProdutoViewModel produtoOfList in ListProdutos.SelectedItems)
                {
                    foreach (var produtoOfPedido in pedido.Pedidos)
                    {
                        if (produtoOfList.IdProduto == produtoOfPedido.Produto.IdProduto)
                            produtoOfPedido.Produto.Selected = true;
                        else if (produtoOfPedido.Produto.Selected == null)
                            produtoOfPedido.Produto.Selected = false;
                    }
                }
            }
            else if (finalizacao)
            {
                CancelarPedido();
            }
        }

        private async void FinalizarPedido(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;
                string url = $"/pedido/finalizar-pedido/status/{'F'}/idPedido/{pedido.IdPedido}/finalizado/{true}";
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

        private async Task<Pedido> TratarResultFinalizacaoPedido(HttpResponseMessage response, bool cancelamento = false)
        {
            var result = new Pedido();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (!cancelamento)
                        result = JsonConvert.DeserializeObject<Pedido>(responseJson);
                    string finalizado = !cancelamento ? "finalizado" : "cancelado";
                    MessageBox.Show($"Pedido {finalizado} com sucesso!",
                        "Informação pedido", MessageBoxButton.OK, MessageBoxImage.Information);

                    var telaMenu = new MenuAdmin(login, funcionario);
                    telaMenu.Show();
                    Close();
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    string finalizado = !cancelamento ? "finalizar" : "cancelar";
                    var responseUsua = MessageBox.Show($"Ocorreu um erro ao {finalizado} pedido! \nPor favor, tente novamente",
                        $"{finalizado} pedido", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    if (!cancelamento)
                        FinalizarPedido();
                    else
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

        private void OnEnter(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                AdicionarProduto();
            }

        }

        private async void CancelarPedido(object sender = null, RoutedEventArgs e = null)
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

                await TratarResultFinalizacaoPedido(response, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Pedido> RemoveProdutoPedido(int idPedido, int idProduto, int qtdeProduto)
        {
            var result = new Pedido();
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/pedido/remove-produto/idPedido/{idPedido}/idProduto/{idProduto}/qtdeProduto/{qtdeProduto}";
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

        private void OnChangePesquisa(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPesquisaprod.Text) && txtPesquisaprod.Text.Length > 11)
                AdicionarProduto();
        }

        private void AplicarDesconto(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(txtTotal.Text, out double totalPedido))
                {
                    var modalValorDesconto = new ModalValorDesconto(login, funcionario, pedido,
                        finalizacao, totalPedido, valorDesconto, nomeCliente);
                    modalValorDesconto.Owner = this;
                    modalValorDesconto.ShowDialog();
                    Close();
                }
                else
                {
                    throw new Exception("Obrigatório inserir ao menos 1 produto para aplicar desconto!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditarValorProduto(object sender, RoutedEventArgs e)
        {
            int idProdutoSelecionado = 0;
            try
            {
                if (finalizacao)
                {

                }
                else
                {
                    if (ListProdutos.SelectedItems.Count > 1 || ListProdutos.SelectedItems.Count == 0)
                    {
                        MessageBox.Show("Selecione 1 produto", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (ListProdutos.SelectedItems.Count > 0)
                    {
                        foreach (Produto produto in ListProdutos.SelectedItems)
                        {
                            if (!produto.ValorVendaEditavel)
                                throw new Exception("Não é possível editar o valor desse produto!");
                        }
                    }
                    else
                    {
                        foreach (ProdutoViewModel produtoSelected in ListProdutos.SelectedItems)
                        {
                            idProdutoSelecionado = produtoSelected.IdProduto;
                        }
                        var valorDesconto = !string.IsNullOrEmpty(txtTotalDesconto.Text) 
                            ? Convert.ToDouble(txtTotalDesconto.Text) : 0.0;
                        var modalVlrProduto = new ModalValorProduto(login, funcionario, pedido, finalizacao,
                            idProdutoSelecionado, false, valorDesconto);
                        modalVlrProduto.Owner = this;
                        modalVlrProduto.ShowDialog();
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}
