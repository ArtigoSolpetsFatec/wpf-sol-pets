using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using wpf_sol_pets._11TelaMenuEstoque;
using wpf_sol_pets._12TelaCrudFornecedor;
using wpf_sol_pets._3TelasBusca._3._2BuscarProduto;
using wpf_sol_pets._6CrudCategoria;
using wpf_sol_pets._7TelaInicioVenda;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._5TelaCrudProduto
{
    /// <summary>
    /// Lógica interna para CrudProduto.xaml
    /// </summary>
    public partial class CrudProduto : Window
    {
        private readonly FuncionarioViewModel funcionario = new();
        private readonly LoginViewModel login = new();
        private List<dynamic> fornecedores = new();
        private List<dynamic> categorias = new();
        private FornecedorViewModel fornecedorSelected = new();
        private CategoriaViewModel categoriaSelected = new();
        private readonly string telaAnterior = string.Empty;
        private readonly string tipoTela = string.Empty;
        private readonly dynamic objPesquisado;
        private readonly Pedido pedido = new();
        private bool vendidoPorQuilo = false;
        private readonly bool finalizacao;

        public CrudProduto(LoginViewModel login, FuncionarioViewModel funcionario, string telaAnterior,
            string tipoTela, dynamic objPesquisado = null, Pedido pedido = null, bool finalizacao = false)
        {
            InitializeComponent();
            this.funcionario = funcionario;
            this.telaAnterior = telaAnterior;
            this.login = login;
            this.objPesquisado = objPesquisado;
            this.tipoTela = tipoTela;
            this.pedido = pedido;
            this.finalizacao = finalizacao;
            GetFornecedores();
            GetCategorias();
            PreencheCampoPesqAnterior();
            VerificaTipoTela();
        }

        private void VerificaTipoTela()
        {
            stackBtnCadastrar.Visibility = tipoTela.Equals("cadastrar") ? Visibility.Visible : Visibility.Hidden;
            stackBtnEditar.Visibility = tipoTela.Equals("editar") ? Visibility.Visible : Visibility.Hidden;
            stackBtnExcluir.Visibility = tipoTela.Equals("excluir") ? Visibility.Visible : Visibility.Hidden;
        }

        private void PreencheCampoPesqAnterior()
        {
            if ((telaAnterior.Contains("buscar") || telaAnterior.Contains("PEDIDOS")) && objPesquisado != null)
            {
                switch (objPesquisado.tipo)
                {
                    case "isbn":
                        txtCampoIsbn.Text = objPesquisado.valor;
                        break;
                    case "nomeProduto":
                        txtCampoNomeProd.Text = objPesquisado.valor;
                        break;
                }
            }
        }

        private async void GetFornecedores()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/fornecedores";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                fornecedores = await TratarResult(response, "fornecedor");
                if (fornecedores.Count >= 0)
                    PreencheCombos(fornecedores, "fornecedor");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GetCategorias()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/categorias";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                categorias = await TratarResult(response, "categoria");
                if (categorias.Count >= 0)
                {
                    PreencheCombos(categorias, "categoria");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<dynamic>> TratarResult(HttpResponseMessage response, string metodoRequisicao)
        {
            var result = new List<dynamic>();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var resultApi = JsonConvert.DeserializeObject<List<dynamic>>(responseJson);
                    if (metodoRequisicao.Contains("fornecedor"))
                    {
                        result.Add(new FornecedorViewModel() { NomeFornecedor = "Cadastrar fornecedor" });
                        foreach (var objResult in resultApi)
                        {
                            var fornecedor = GeneralExtensions.ConvertToType<FornecedorViewModel>(objResult);
                            result.Add(fornecedor);
                        }
                        return result;
                    }
                    else if (metodoRequisicao.Contains("categoria"))
                    {
                        result.Add(new CategoriaViewModel() { TipoCategoria = "Cadastrar categoria" });
                        foreach (var objResult in resultApi)
                        {
                            var categoria = GeneralExtensions.ConvertToType<CategoriaViewModel>(objResult);
                            result.Add(categoria);
                        }
                        return result;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    GetFornecedores();
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
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

        private void PreencheCombos(List<dynamic> dynamics, string tipoObjeto)
        {
            if (tipoObjeto.Contains("fornecedor"))
                ComboFornecedores.ItemsSource = fornecedores;
            else if (tipoObjeto.Contains("categoria"))
                comboCategorias.ItemsSource = categorias;
        }

        private void SetFornecedor(object sender, SelectionChangedEventArgs e)
        {
            foreach (var fornecedor in fornecedores)
            {
                foreach (var item in e.AddedItems)
                {
                    if (item.Equals(fornecedor) && fornecedor.IdFornecedor == 0)
                    {
                        var telaFornecedor = new CrudFornecedor(login, "cadastrar", "tela-produto", funcionario);
                        telaFornecedor.Show();
                        Close();
                    }
                    else if (item.Equals(fornecedor))
                    {
                        fornecedorSelected = GeneralExtensions.ConvertToType<FornecedorViewModel>(fornecedor); ;
                    }
                }
            }
        }

        private void SetCategoria(object sender, SelectionChangedEventArgs e)
        {
            foreach (var categoria in categorias)
            {
                foreach (var item in e.AddedItems)
                {
                    if (item.Equals(categoria) && categoria.IdCategoria == 0)
                    {
                        var telaFornecedor = new CrudCategoriaProduto("cadastrar", "tela-produto", funcionario, login);
                        telaFornecedor.Show();
                        Close();
                    }
                    else if (item.Equals(categoria))
                    {
                        categoriaSelected = GeneralExtensions.ConvertToType<CategoriaViewModel>(categoria);
                    }
                }
            }
        }

        private void BuscarImagem(object sender, RoutedEventArgs e)
        {
            BitmapImage myBitmapImage = new();
            imageProduto.Source = myBitmapImage;
            try
            {
                var file = new OpenFileDialog();
                file.Title = "Selecione um Arquivo";
                file.InitialDirectory = "";
                file.Filter = "Image Files (*.gif,*.jpg,*.jpeg,*.bmp,*.png)|*.gif;*.jpg;*.jpeg;*.bmp;*.png";
                file.FilterIndex = 1;
                file.ShowDialog();

                txtCampoImagem.Text = file.FileName;
                var nomeArquivo = NomeArquivo(txtCampoImagem.Text.Trim());
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(txtCampoImagem.Text.Trim());
                myBitmapImage.DecodePixelWidth = 200;
                myBitmapImage.EndInit();
                imageProduto.Source = myBitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string NomeArquivo(string nomeArquivo)
        {
            int posicaoBarra = nomeArquivo.LastIndexOf(@"\");
            string _nomeArquivo = nomeArquivo.Substring(posicaoBarra + 1);
            return _nomeArquivo;
        }

        private void VoltarTelaAnterior(object sender = null, RoutedEventArgs e = null)
        {
            if (telaAnterior.Contains("buscar"))
            {
                var telaBuscarProd = new BuscarProduto(login, funcionario, "CRUD-PRODUTO");
                telaBuscarProd.Show();
            }
            else if (telaAnterior.Contains("estoque"))
            {
                var telaMenuEstoque = new MenuEstoque(login, funcionario);
                telaMenuEstoque.Show();
            }
            else if (telaAnterior.Contains("PEDIDOS"))
            {
                var telaCrudFornecedor = new CrudPedido(login, funcionario, "tela-produto", pedido, finalizacao);
                telaCrudFornecedor.Show();
            }
            Close();
        }

        private async void CadastrarProduto(object sender = null, RoutedEventArgs e = null)
        {
            var payloadEnvio = new Produto();
            payloadEnvio.Fornecedor = new();
            try
            {
                Loading.Visibility = Visibility.Visible;
                Loading.Spin = true;
                stackBtnCadastrar.Visibility = Visibility.Hidden;

                payloadEnvio.IsbnProduto = txtCampoIsbn.Text;
                payloadEnvio.MarcaProduto = txtCampoMarca.Text;
                payloadEnvio.NomeProduto = txtCampoNomeProd.Text;
                payloadEnvio.QtdeEstoque = int.TryParse(txtCampoQuantidade.Text, out int qtde) ?
                    Convert.ToInt32(txtCampoQuantidade.Text) : throw new Exception("Informe um número inteiro para quantidade!");
                payloadEnvio.ValorUnitarioVenda = double.TryParse(txtCampoValUnitVenda.Text, out double valorVenda) ?
                    Convert.ToDouble(txtCampoValUnitVenda.Text) : throw new Exception("Informe um número decimal para o valor de venda do produto!");
                payloadEnvio.ValorUnitarioCusto = double.TryParse(txtCampoValUnitCusto.Text, out double valorCusto) ?
                    Convert.ToDouble(txtCampoValUnitCusto.Text) : throw new Exception("Informe um número decimal para o valor de custo do produto!");
                payloadEnvio.PesoAplicavel = double.TryParse(txtCampoPesoApp.Text, out double pesoApp) ?
                    Convert.ToDouble(txtCampoPesoApp.Text) : throw new Exception("Informe um número decimal para o peso aplicável do produto!");
                payloadEnvio.PesoProduto = double.TryParse(txtCampoPesoProd.Text, out double pesoProd) ?
                    Convert.ToDouble(txtCampoPesoProd.Text) : throw new Exception("Informe um número decimal para o peso do produto!");
                payloadEnvio.IdadeAplicavel = int.TryParse(txtCampoIdadeApp.Text, out int idadeApp) ?
                    Convert.ToInt32(txtCampoIdadeApp.Text) : throw new Exception("Informe um número inteiro para a idade aplicável do produto!");
                payloadEnvio.IdCategoria = categoriaSelected.IdCategoria;
                payloadEnvio.Fornecedor.IdFornecedor = fornecedorSelected.IdFornecedor;
                payloadEnvio.FotoProduto = !string.IsNullOrEmpty(imageProduto.Source.ToString()) ?
                    imageProduto.Source.ToString().Replace(@"file:///", "") : null;
                payloadEnvio.ValorVendaEditavel = vendidoPorQuilo;
                if (DateTime.TryParseExact(txtCampoDhValidade.Text, "dd/MM/yyyy",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out DateTime dhValidade))
                {
                    payloadEnvio.DataValidade = dhValidade;
                }
                else
                {
                    throw new Exception("Data de validade é inválida! Exemplo correto: 05/12/2025");
                }

                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/produto";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Content = new StringContent(JsonConvert.SerializeObject(payloadEnvio), Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResult(response);
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Hidden;
                Loading.Spin = false;
                stackBtnCadastrar.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<ProdutoViewModel> TratarResult(HttpResponseMessage response)
        {
            var result = new ProdutoViewModel();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    Loading.Visibility = Visibility.Hidden;
                    Loading.Spin = false;
                    stackBtnCadastrar.Visibility = Visibility.Visible;
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<ProdutoViewModel>(responseJson);

                    if (result.IdProduto > 0)
                    {
                        MessageBox.Show("Produto cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        if (pedido?.Produtos?.Count > 0)
                            pedido.Produtos.Add(result);
                        VoltarTelaAnterior();
                    }

                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception("Ocorreu um erro ao cadastrar produto!");
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    CadastrarProduto();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private void SetVendaPorQuilo(object sender, SelectionChangedEventArgs e)
        {
            vendidoPorQuilo = vendidoPorQuiloS.IsSelected;
        }
    }
}
