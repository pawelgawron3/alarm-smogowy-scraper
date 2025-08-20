using ScraperConsole.Models;

namespace ScraperConsole.WebScrapers;
public interface IScraper : IDisposable
{
    public List<Article> StartScraping(string url);
}
