using ScraperConsole.Models;
using ScraperConsole.WebScrapers;

namespace ScraperConsole.Controllers;
public class ScraperController
{
    private readonly Dictionary<string, Func<IScraper>> _scraperFactory; 

    public ScraperController()
    {
        _scraperFactory = new Dictionary<string, Func<IScraper>>()
        {
            { "https://iee.org.pl", () => new WebScraper()},
        };
    }

    public List<Article> Scrape(string url)
    {
        if (!_scraperFactory.TryGetValue(url, out var scraperFactory))
        {
            Console.WriteLine($"No scraper available for domain: {url}");
            return new List<Article>();
        }
        else
        {
            using (var scraper = scraperFactory())
            {
                return scraper.StartScraping(url);
            }
        }
    } 
}
