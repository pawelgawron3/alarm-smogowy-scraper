using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class SmoglabScraper : ScraperBaseClass
{
    private string _jsonString;

    public SmoglabScraper()
    {
        _jsonString = File.ReadAllText(jsonPath);
    }

    public override List<Article> StartScraping(string url)
    {
        const int timeoutInterval = 5;
        var articles = new List<Article>();

        var targets = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(_jsonString);
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

                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("h1.elementor-heading-title")));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div.elementor-widget-container")));

                string title = _driver.FindElement(By.CssSelector("h1.elementor-heading-title")).Text.Trim();

                var articleContainer = _driver.FindElement(
                    By.XPath("//div[contains(@class, 'elementor-widget-container')]" +
                    "[.//p]" +
                    "[.//h2]"));
                var childElements = articleContainer.FindElements(By.XPath("./*"));

                Article article = new Article()
                {
                    Title = title
                };

                foreach (var element in childElements)
                {
                    switch (element.TagName.ToLower())
                    {
                        case "p":
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.Paragraph,
                                Text = element.Text.Trim()
                            });
                            break;

                        case "h2":
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.Header,
                                Text = element.Text.Trim()
                            });
                            break;
                    }
                }

                articles.Add(article);
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"[WARN] Expected elements not found on page: {subpageUrl}");
                Console.WriteLine($"       Details: {ex.Message}");
                continue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Unexpected error occurred while scraping: {subpageUrl}");
                Console.WriteLine($"        Details: {ex.Message}");
            }
        }

        return articles;
    }
}