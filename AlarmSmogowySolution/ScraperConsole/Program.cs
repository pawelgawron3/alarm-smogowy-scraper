using ScraperConsole.Models;
using ScraperConsole.Controllers;

namespace ScraperConsole
{
    public class Program
    {
        private static void Main(string[] args)
        {
            const string url = "https://iee.org.pl";
            ScraperController controller = new ScraperController();

            var articles = controller.Scrape(url);

            //Testing
            Console.WriteLine();
            Console.WriteLine($"Znaleziono {articles.Count} artykułów.\n");

            foreach (var art in articles)
            {
                Console.WriteLine($"Tytuł: {art.Title}");
                Console.WriteLine($"Tekst: {art.Content}");
                Console.WriteLine();
            }
        }
    }
}