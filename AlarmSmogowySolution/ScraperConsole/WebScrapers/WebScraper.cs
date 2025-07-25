using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class WebScraper : IDisposable
{
    private readonly string jsonPath = Path.Combine("Resources", "scrapingTargets.json");
    private string jsonString;
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
        jsonString = File.ReadAllText(jsonPath);
    }

    public List<Article> StartScraping(string url)
    {
        const int timeoutInterval = 5;
        var articles = new List<Article>();

        var targets = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);
        if (targets == null || !targets.ContainsKey(url))
        {
            Console.WriteLine($"There is no such page in .json file: {url}");
            return articles;
        }

        var subpages = targets[url];
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInterval));

        foreach (var subpageUrl in subpages)
        {
            try
            {
                _driver.Navigate().GoToUrl(subpageUrl);

                wait.Until(ExpectedConditions.ElementExists(By.TagName("h1")));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div.wpb_wrapper")));

                string title = _driver.FindElement(By.TagName("h1")).Text.Trim();

                var articleContainer = _driver.FindElement(By.CssSelector("div.wpb_wrapper"));
                var pContainer = articleContainer.FindElements(By.TagName("p"));

                string content = string.Join("\n", pContainer.Select(p => p.Text.Trim()));

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
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"The expected elements were not found on the page: {subpageUrl}\n{ex.Message}");
                continue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
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