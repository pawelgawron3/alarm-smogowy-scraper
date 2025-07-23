using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class WebScraper : IDisposable
{
    private ChromeDriver _driver;
    private bool _disposed = false;

    public WebScraper()
    {
        var chromeOptions = new ChromeOptions();

        //disabled for tests
        //chromeOptions.AddArguments("--headless"); //without browser GUI
        chromeOptions.AddArguments("--disable-gpu"); //for stability on Windows/Linux
        chromeOptions.AddArguments("--disk-cache-size=0"); //always load the new page
        chromeOptions.AddArguments("--no-sandbox"); //for Linux env
        chromeOptions.AddArguments("--window-size=1920,1080"); //set the tab resolution

        _driver = new ChromeDriver(chromeOptions);
    }

    public List<Article> StartScraping(string url)
    {
        const int timeoutInterval = 5;
        var articles = new List<Article>();

        _driver.Navigate().GoToUrl(url);
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInterval));

        var link = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='https://iee.org.pl/index.php/aktualnosci/']")));
        link.Click();

        var articlecontainers = _driver.FindElements(By.CssSelector("div.wpb_wrapper"));

        foreach (var container in articlecontainers)
        {
            try
            {
                string title = container.FindElement(By.TagName("h4")).Text.Trim();
                string content = container.FindElement(By.TagName("p")).Text.Trim();

                if (articles.Any(a => a.Title == title))
                {
                    continue;
                }

                Article article = new Article()
                {
                    Title = title,
                    Content = content
                };
                articles.Add(article);
            }
            catch (NoSuchElementException)
            {
                continue;
            }
        }
        return articles;
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