using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wpf_sol_pets._13MenuFinanceiro;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._14TelaIndicadores
{
    /// <summary>
    /// Lógica interna para TelaIndicadores.xaml
    /// </summary>
    public partial class TelaIndicadores : Window
    {
        public readonly LoginViewModel infoLogin = new();
        private readonly FuncionarioViewModel funcionario = new();
        private readonly string tipoIndicador;

        public TelaIndicadores(LoginViewModel infoLogin, FuncionarioViewModel funcionario, string tipoIndicador)
        {
            InitializeComponent();
            this.infoLogin = infoLogin;
            this.funcionario = funcionario;
            this.tipoIndicador = tipoIndicador;
            if (tipoIndicador.ToUpper().Equals("ANO"))
            {
                txtIndicador.Text = "Gráfico apenas de visualização de seus indicadores do ano corrente";
                GetIndicadoresByAno();
            }
            else if (tipoIndicador.ToUpper().Equals("MES"))
            {
                txtIndicador.Text = "Gráfico apenas de visualização de seus indicadores do mês corrente";
                GetIndicadoresByMes();
            }
            else
            {
                txtIndicador.Text = "Gráfico apenas de visualização de seus indicadores do dia corrente";
                GetIndicadoresByDia();
            }
        }

        private async void GetIndicadoresByAno()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/indicadores/meses/" + 12;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResultIndicadores(response, tipoIndicador);
                if (result.Count > 0)
                {
                    grafico.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GetIndicadoresByMes()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = $"/indicadores/meses/" + 1;
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResultIndicadores(response, tipoIndicador);
                if (result.Count > 0)
                {
                    grafico.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GetIndicadoresByDia()
        {
            try
            {
                var objTokenClient = await GeneralExtensions.GetToken();
                var token = objTokenClient.token;
                var client = objTokenClient.client;

                string url = "/indicadores/dia";
                var uri = new Uri("http://localhost:64967" + url);
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                var result = await TratarResultIndicadores(response, tipoIndicador);
                if (result.Count > 0)
                {
                    grafico.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<IndicadorViewModel>> TratarResultIndicadores(HttpResponseMessage response, string tipoIndicador)
        {
            var result = new List<IndicadorViewModel>();
            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<List<IndicadorViewModel>>(responseJson);
                }
                else if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new Exception("No momento, não há indicadores na base de dados! Cadastre suas vendas e custos.");
                }
                else if (response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GeneralExtensions.TokenView = "";
                    if (tipoIndicador.ToUpper().Equals("ANO"))
                        GetIndicadoresByAno();
                    else if (tipoIndicador.ToUpper().Equals("MES"))
                        GetIndicadoresByMes();
                    else
                        GetIndicadoresByDia();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        private void VoltarTelaAnterior(object sender, RoutedEventArgs e)
        {
            var menuFinanceiro = new MenuFinanceiro(infoLogin, funcionario);
            menuFinanceiro.Show();
            Close();
        }
    }
}
