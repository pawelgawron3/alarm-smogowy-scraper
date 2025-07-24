using System.Runtime.CompilerServices;
using ScraperConsole.WebScrapers;
using ScraperConsole.Models;

namespace ScraperConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string url = "https://iee.org.pl";
            using (var scraper = new WebScraper())
            {
                var articles = scraper.StartScraping(url);

                //Testing purposes
                Console.WriteLine();
                Console.WriteLine($"Znaleziono {articles.Count} artykułów.");

                foreach (var art in articles)
                {
                    Console.WriteLine($"Tytuł: {art.Title}");
                    Console.WriteLine($"Tekst: {art.Content}");
                    Console.WriteLine();
                }
            }
        }
    }
}