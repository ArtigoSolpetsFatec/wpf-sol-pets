using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using wpf_sol_pets._2TelaAdministrativa;
using wpf_sol_pets.Extensions;
using wpf_sol_pets.Models;
using wpf_sol_pets.Models.ViewModels;

namespace wpf_sol_pets._1___Tela_Login
{
    /// <summary>
    /// Lógica interna para TelaLogin.xaml
    /// </summary>
    public partial class TelaLogin : Window
    {
        public TelaLogin()
        {
            InitializeComponent();
            LoadImage();
        }

        private void LoadImage()
        {
            Image image = new();
            image.Width = 200;
            BitmapImage myBitmapImage = new();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(@"C:\imagensWpf/logo.jpg");
            myBitmapImage.DecodePixelWidth = 200;
            myBitmapImage.EndInit();
            imageLogin.Source = myBitmapImage;
        }

        private void ButtonMouseEnter(object sender, MouseEventArgs e)
        {
            GeneralExtensions.ButtonMouseEnter(sender, e);
        }

        private void ButtonMouseLeave(object sender, MouseEventArgs e)
        {
            GeneralExtensions.ButtonMouseLeave(sender, e);
        }

        /// <summary>
        /// Botão de login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetLogin(object sender, RoutedEventArgs e)
        {
            GetLogin();
        }

        /// <summary>
        /// Evento de mudança para textbox email
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtEmail.Focus();
        }

        /// <summary>
        /// Evento de enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoggin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLogin();
            }
        }

        /// <summary>
        /// Executa a chamada da api e valida dados de login
        /// </summary>
        private async void GetLogin()
        {
            var result = new LoginViewModel();
            try
            {
                HttpClient client = GeneralExtensions.GetClient();
                Loading.Visibility = Visibility.Visible;
                buttonLoggin.Visibility = Visibility.Hidden;
                Loading.Spin = true;
                string email = !string.IsNullOrEmpty(txtEmail.Text.ToString()) ? txtEmail.Text.ToString() :
                    throw new Exception("Obrigatório informar o e-mail!");
                email = email.Contains("@") ? email :
                    throw new Exception("E-mail informado é inválido");
                string senha = !string.IsNullOrEmpty(txtSenha.Password.ToString()) ? txtSenha.Password.ToString() :
                    throw new Exception("Obrigatório informar a senha!");
                ;
                Login loginUser = new()
                {
                    Email = txtEmail.Text.ToString().Trim(),
                    Senha = txtSenha.Password.ToString().Trim()
                };

                var token = await GeneralExtensions.GetToken("admin", "admin");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }
                else
                {
                    throw new Exception("Erro ao buscar token de autenticação!");
                }

                string url = $"/login/email/{loginUser.Email}/senha/{loginUser.Senha}";
                HttpRequestMessage request = new(HttpMethod.Get, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<LoginViewModel>(responseJson);

                    if (result.LoginIsValid)
                    {
                        var funcionario = new FuncionarioViewModel();
                        var avancaTela = new MenuAdmin(result, funcionario, true);
                        avancaTela.Show();
                        Close();
                    }
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    MessageBox.Show($"Email e senha não encontrados!", "Dados não encontrados", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    string messageError = await response.Content.ReadAsStringAsync();
                    throw new Exception(messageError);
                }

                Loading.Spin = false;
                Loading.Visibility = Visibility.Hidden;
                buttonLoggin.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Loading.Spin = false;
                Loading.Visibility = Visibility.Hidden;
                buttonLoggin.Visibility = Visibility.Visible;
            }
        }
    }
}
