using OpenQA.Selenium.Chrome;
using ScraperConsole.Models;

namespace ScraperConsole.WebScrapers;

public abstract class ScraperBaseClass : IScraper, IDisposable
{
    protected readonly string jsonPath = Path.Combine("Resources", "scrapingTargets.json");
    protected ChromeDriver _driver;
    private bool _disposed = false;

    public ScraperBaseClass()
    {
        var chromeOptions = new ChromeOptions();

        chromeOptions.AddArguments("--headless"); //without browser GUI
        chromeOptions.AddArguments("--disable-gpu"); //for stability on Windows/Linux
        chromeOptions.AddArguments("--disk-cache-size=0"); //always load the new page
        chromeOptions.AddArguments("--no-sandbox"); //for Linux env
        chromeOptions.AddArguments("--window-size=1920,1080"); //set the tab resolution

        _driver = new ChromeDriver(chromeOptions);
    }

    public abstract List<Article> StartScraping(string url);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        _disposed = true;
    }

    ~ScraperBaseClass()
    {
        Dispose(false);
    }
}