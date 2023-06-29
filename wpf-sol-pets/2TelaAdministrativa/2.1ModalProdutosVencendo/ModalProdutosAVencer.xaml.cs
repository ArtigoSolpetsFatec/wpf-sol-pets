using System;
using System.Collections.Generic;
using System.Windows;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._2TelaAdministrativa._2._1ModalProdutosVencendo
{
    /// <summary>
    /// Lógica interna para ModalProdutosAVencer.xaml
    /// </summary>
    public partial class ModalProdutosAVencer : Window
    {
        public readonly LoginViewModel infoLogin = new();
        private readonly FuncionarioViewModel funcionario = new();
        private List<ProdutoViewModel> produtos = new();

        public ModalProdutosAVencer(LoginViewModel infoLogin, FuncionarioViewModel funcionario,
            List<ProdutoViewModel> produtos)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
            this.produtos = produtos;
            PreencherListaProdutos();
        }

        private void PreencherListaProdutos()
        {
            var produtosAVencer = new List<ProdutoAVencer>();
            var produtoAVencer = new ProdutoAVencer();
            try
            {
                MessageBox.Show("Os Produtos da lista irão vencer em 3 meses ou menos. " +
                    "Aconselhável ação de marketing para vendê-los antes do vencimento atentando-se as leis " +
                    "e avisando os clientes!!.",
                    "Produtos a vencer", MessageBoxButton.OK, MessageBoxImage.Warning);
                foreach (var produto in produtos)
                {
                    produtoAVencer.NomeProduto = produto.NomeProduto;
                    produtoAVencer.ValorUnitarioVenda = produto.ValorUnitarioVenda;
                    produtoAVencer.QtdeEstoque = produto.QtdeEstoque;
                    produtoAVencer.Vencimento = produto.DataValidade.CalculaTempoFaltante();

                    produtosAVencer.Add(produtoAVencer);
                }
                ListProdutos.ItemsSource = produtosAVencer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VoltarAoMenu(object sender, RoutedEventArgs e)
        {
            var menuAdmin = new MenuAdmin(infoLogin, funcionario, false);
            menuAdmin.Show();
            Close();
        }
    }
}
