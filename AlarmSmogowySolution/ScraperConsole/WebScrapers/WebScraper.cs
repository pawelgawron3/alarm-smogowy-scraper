using OpenQA.Selenium.Chrome;

namespace ScraperConsole.WebScrapers;
public class WebScraper : IDisposable
{
    private ChromeDriver _driver;
    private bool _disposed = false;

    public WebScraper()
    {
        var chromeOptions = new ChromeOptions();

        chromeOptions.AddArguments("--headless"); //without browser GUI
        chromeOptions.AddArguments("--disable-gpu"); //for stability on Windows/Linux
        chromeOptions.AddArguments("--disk-cache-size=0"); //always load the new page
        chromeOptions.AddArguments("--no-sandbox"); //for Linux env
        chromeOptions.AddArguments("--window-size=1920,1080"); //set the tab resolution

        _driver = new ChromeDriver(chromeOptions);
    }

    public void Dispose()
    {
        Quit();
        GC.SuppressFinalize(this);
    }
    public void Quit()
    {
        if (!_disposed)
        {
            _driver?.Quit();
            _disposed = true;   
        }
    }
    ~WebScraper() 
    {
        Quit();
    }
}
