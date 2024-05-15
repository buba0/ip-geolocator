using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeZoneConverter;

namespace geolocator
{
    public class Data
    {
        public string ip { get; set; }
        public string hostname { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string loc { get; set; }
        public string org { get; set; }
        public string postal { get; set; }
        public string timezone { get; set; }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Lokalizator IP";
            Console.Write("Wprowadź adres IP lub nazwę hosta: ");
            string input = Console.ReadLine();
            string ip = "";

            // Check if input is IP address or hostname
            if (IPAddress.TryParse(input, out IPAddress address))
            {
                ip = input;
            }
            else
            {
                try
                {
                    // Resolve hostname to IP address
                    IPAddress[] addresses = await Dns.GetHostAddressesAsync(input);
                    if (addresses.Length > 0)
                    {
                        ip = addresses[0].ToString();
                    }
                    else
                    {
                        Console.WriteLine("Nie można zarejestrować adresu IP dla podanej nazwy hosta.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas rozwiązywania nazwy hosta: {ex.Message}");
                    return;
                }
            }

            string url = $"https://ipinfo.io/{ip}/json";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine("[+] Zapytanie wysłane prawidłowo!");
                    string responseData = await response.Content.ReadAsStringAsync();
                    Data ipInfo = JsonConvert.DeserializeObject<Data>(responseData);
                    Console.Clear();
                    Console.WriteLine($"IP: {ipInfo.ip}");
                    Console.WriteLine($"Hostname: {ipInfo.hostname}");
                    Console.WriteLine($"Miasto: {ipInfo.city}");
                    Console.WriteLine($"Region: {ipInfo.region}");
                    Console.WriteLine($"Kraj: {ipInfo.country}");
                    Console.WriteLine($"Lokalizacja: {ipInfo.loc}");
                    Console.WriteLine($"Organizacja/ISP: {ipInfo.org}");
                    Console.WriteLine($"Kod pocztowy: {ipInfo.postal}");
                    Console.WriteLine($"Strefa czasowa: {ipInfo.timezone}");

                    // Display current time in the timezone
                    try
                    {
                        string windowsTimeZoneId = TZConvert.IanaToWindows(ipInfo.timezone);
                        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
                        DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);
                        Console.WriteLine($"Aktualny czas w tej strefie czasowej: {currentTime.ToString()}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd podczas pobierania aktualnego czasu: {ex.Message}");
                    }


                    //Open Google Maps with location coordinates
                    try
                    {
                        string googleMapsUrl = $"https://www.google.com/maps?q={ipInfo.loc}";
                        //Console.WriteLine($"Street view: {googleMapsUrl}");
                        Console.WriteLine("Otworzyć street view? [y/n]");
                        string choice = Console.ReadLine();
                        if (choice == "y" || choice == "Y")
                        {
                            Process.Start(new ProcessStartInfo { FileName = googleMapsUrl, UseShellExecute = true });
                        }
                        if (choice == "n" || choice == "N")
                        {
                            Environment.Exit(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd podczas otwierania Google Maps: {ex.Message}");
                    }

                    

                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Błąd: {ex.Message}");
                }
            }
        }
    }
}
