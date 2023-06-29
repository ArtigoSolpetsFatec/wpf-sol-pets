using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using wpf_sol_pets.Models;

namespace wpf_sol_pets.Extensions
{
    public static class GeneralExtensions
    {
        public static string TokenView { get; set; }

        public static async Task<string> GetToken(string user, string password)
        {
            var httpClient = GetClient();
            TokenViewModel result = new();
            try
            {
                Token content = new()
                {
                    Username = user,
                    Password = password
                };
                string jsonContent = JsonConvert.SerializeObject(content);
                string url = "/token";
                HttpRequestMessage request = new(HttpMethod.Post, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.SendAsync(request, CancellationToken.None);
                response.EnsureSuccessStatusCode(); //lança um código de erro
                result = await response.Content.ReadFromJsonAsync<TokenViewModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            TokenView = result.Token;
            return result.Token;
        }

        public static HttpClient GetClient()
        {
            HttpClient client = new();
            client.BaseAddress = new Uri("http://localhost:64967");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        public static void ButtonMouseEnter(object sender, MouseEventArgs e)
        {
            Button btnLogin = sender as Button;
            btnLogin.BorderBrush = Brushes.LightBlue;
        }

        public static void ButtonMouseLeave(object sender, MouseEventArgs e)
        {
            Button btnLogin = sender as Button;
            btnLogin.Background = Brushes.DarkBlue;
        }

        public static bool ValidarCPF(this string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;

            int num1 = int.Parse(cpf.Substring(0, 1));
            int num2 = int.Parse(cpf.Substring(1, 1));
            int num3 = int.Parse(cpf.Substring(2, 1));
            int num4 = int.Parse(cpf.Substring(3, 1));
            int num5 = int.Parse(cpf.Substring(4, 1));
            int num6 = int.Parse(cpf.Substring(5, 1));
            int num7 = int.Parse(cpf.Substring(6, 1));
            int num8 = int.Parse(cpf.Substring(7, 1));
            int num9 = int.Parse(cpf.Substring(8, 1));
            int num10 = int.Parse(cpf.Substring(9, 1));
            int num11 = int.Parse(cpf.Substring(10, 1));

            if (num1.Equals(num2) && num2.Equals(num3) && num3.Equals(num4) && num4.Equals(num5) &&
                num5.Equals(num6) && num7.Equals(num8) && num8.Equals(num9) && num9.Equals(num10) &&
                num10.Equals(num11))
                return false;

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf += digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito += resto.ToString();
            return cpf.EndsWith(digito);
        }

        public static bool ValidarCNPJ(this string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;
            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
                return false;

            int num1 = int.Parse(cnpj.Substring(0, 1));
            int num2 = int.Parse(cnpj.Substring(1, 1));
            int num3 = int.Parse(cnpj.Substring(2, 1));
            int num4 = int.Parse(cnpj.Substring(3, 1));
            int num5 = int.Parse(cnpj.Substring(4, 1));
            int num6 = int.Parse(cnpj.Substring(5, 1));
            int num7 = int.Parse(cnpj.Substring(6, 1));
            int num8 = int.Parse(cnpj.Substring(7, 1));
            int num9 = int.Parse(cnpj.Substring(8, 1));
            int num10 = int.Parse(cnpj.Substring(9, 1));
            int num11 = int.Parse(cnpj.Substring(10, 1));
            int num12 = int.Parse(cnpj.Substring(11, 1));
            int num13 = int.Parse(cnpj.Substring(12, 1));
            int num14 = int.Parse(cnpj.Substring(13, 1));

            if (num1.Equals(num2) && num2.Equals(num3) && num3.Equals(num4) && num4.Equals(num5) &&
                num5.Equals(num6) && num7.Equals(num8) && num8.Equals(num9) && num9.Equals(num10) &&
                num10.Equals(num11) && num11.Equals(num12) && num12.Equals(num13) && num13.Equals(num14))
                return false;

            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito += resto.ToString();

            return cnpj.EndsWith(digito);
        }

        public static T ConvertToType<T>(object value) where T : class
        {
            var jsonData = JsonConvert.SerializeObject(value);
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string CalculaIdade(DateTime dNascimento, DateTime dAtual)
        {
            try
            {
                int idDias = 0, idMeses = 0, idAnos = 0;
                DateTime dNascimentoCorrente = DateTime.Parse(dNascimento.Day.ToString() + "/" +
                dNascimento.Month.ToString() + "/" + (dAtual.Year - 1).ToString());
                string ta = "", tm = "", td = "";
                if (dAtual < dNascimento)
                {
                    return "Data de nascimento inválida ";
                }
                idAnos = dAtual.Year - dNascimento.Year;
                if (dAtual.Month < dNascimento.Month || (dAtual.Month ==
                dNascimento.Month && dAtual.Day < dNascimento.Day))
                {
                    idAnos--;
                }
                idMeses = CalculaMeses(dAtual, dNascimento);
                idDias = CalculaDias(dAtual, dNascimento);
                if (idAnos > 1)
                    ta = idAnos + " anos ";
                else if (idAnos == 1)
                    ta = idAnos + "ano";
                if (idMeses > 1)
                    tm = idMeses + " meses ";
                else if (idMeses == 1)
                    tm = idMeses + " mês ";
                if (idDias > 1)
                    td = idDias + " dias ";
                else if (idDias == 1)
                    td = idDias + " dia ";
                return ta + tm + td;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static int CalculaDias(DateTime dataAtual, DateTime dataOriginal)
        {
            try
            {
                int numeroDias = 0;
                if ((dataAtual.Month > dataOriginal.Month || dataAtual.Month <
                dataOriginal.Month) && (dataAtual.Day > dataOriginal.Day))
                {
                    DateTime dUltima = DateTime.Parse(dataOriginal.Day.ToString() + "/" +
                    (dataAtual.Month).ToString() + "/" + (dataAtual.Year).ToString());
                    numeroDias = (dataAtual - dUltima).Days;
                }
                else if ((dataAtual.Month > dataOriginal.Month || dataAtual.Month <
                dataOriginal.Month) && (dataAtual.Day < dataOriginal.Day))
                {
                    DateTime dUltima = DateTime.Parse(dataOriginal.Day.ToString() + "/" +
                    (dataAtual.Month - 1).ToString() + "/" + (dataAtual.Year).ToString());
                    numeroDias = (dataAtual - dUltima).Days;
                }
                else if (dataOriginal.Month == dataAtual.Month)
                {
                    DateTime dUltima = DateTime.Parse(dataOriginal.Day.ToString() + "/" +
                    (dataAtual.Month).ToString() + "/" + (dataAtual.Year).ToString());
                    numeroDias = (dataAtual - dUltima).Days;
                }
                return numeroDias;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static int CalculaMeses(DateTime dataAtual, DateTime dataOriginal)
        {
            try
            {
                int numeroMeses = 0;
                if (dataAtual.Month > dataOriginal.Month)
                {
                    numeroMeses = dataAtual.Month - dataOriginal.Month;
                }
                else if (dataAtual.Month < dataOriginal.Month)
                {
                    if (dataAtual.Day > dataOriginal.Day)
                    {
                        numeroMeses = (12 - dataOriginal.Month) + (dataAtual.Month);
                    }
                    else if (dataAtual.Day < dataOriginal.Day)
                    {
                        numeroMeses = (12 - dataOriginal.Month) + (dataAtual.Month - 1);
                    }
                }
                return numeroMeses;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string CalculaTempoFaltante(this DateTime dataParametro)
        {
            DateTime dataAtual = DateTime.Now;
            TimeSpan diferenca = dataParametro - dataAtual;

            int mesesFaltantes = (int)diferenca.TotalDays / 30;
            int diasFaltantes = (int)diferenca.TotalDays % 30;

            return $"{mesesFaltantes} meses e {diasFaltantes} dias.";

        }

        public static async Task<dynamic> GetToken()
        {
            try
            {
                HttpClient client = GetClient();
                string token;
                if (string.IsNullOrEmpty(TokenView))
                {
                    token = await GetToken("admin", "admin");
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    }
                    else
                    {
                        throw new Exception("Erro ao buscar token de autenticação!");
                    }
                }
                else
                {
                    token = TokenView;
                }
                return new { token, client };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
