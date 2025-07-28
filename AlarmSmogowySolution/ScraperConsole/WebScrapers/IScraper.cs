using ScraperConsole.Models;

namespace ScraperConsole.WebScrapers;
public interface IScraper
{
    public List<Article> StartScraping(string url);
}
