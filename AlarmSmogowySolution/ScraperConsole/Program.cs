using QuestPDF.Infrastructure;
using ScraperConsole.Controllers;

namespace ScraperConsole
{
    public class Program
    {
        private static void Main(string[] args)
        {
            /*
            * QuestPDF requires explicitly selecting a license type.
            * This line sets the license to "Community", which is free to use
            * for individuals, open-source projects, and organizations with
            * annual gross revenue below $1M USD.
            *
            * For more details, see: https://www.questpdf.com/license/
            */
            QuestPDF.Settings.License = LicenseType.Community;

            const string url = "https://iee.org.pl";
            const string url2 = "https://smogopedia.pl";
            ScraperController controller = new ScraperController();

            var articles = controller.Scrape(url2);

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