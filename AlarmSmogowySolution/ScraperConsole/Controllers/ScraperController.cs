using ScraperConsole.Models;
using ScraperConsole.WebScrapers;
using ScraperConsole.Helpers;

namespace ScraperConsole.Controllers;
public class ScraperController
{
    private readonly Dictionary<string, Func<IScraper>> _scraperFactory; 

    public ScraperController()
    {
        _scraperFactory = new Dictionary<string, Func<IScraper>>()
        {
            { "https://iee.org.pl", () => new IeeScraper()},
            { "https://smogopedia.pl", () => new SmogopediaScraper()},
            { "https://smoglab.pl", () => new SmoglabScraper() },
            { "https://czystepowietrze.gov.pl/", () => new CzystePowietrzeScraper() }
        };
    }

    public void Scrape(string url)
    {
        if (!_scraperFactory.TryGetValue(url, out var scraperFactory))
        {
            Console.WriteLine($"No scraper available for domain: {url}");
        }
        else
        {
            using (var scraper = scraperFactory())
            {
                var domain = new Uri(url).Host;
                var articles = scraper.StartScraping(url);
                PdfHelper.SaveArticlesToPdfs(articles, domain);
            }
        }
    } 
}
