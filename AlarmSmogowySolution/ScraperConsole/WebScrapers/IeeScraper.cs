using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Helpers;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class IeeScraper : ScraperBaseClass
{
    private string _jsonString;

    public IeeScraper()
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

                wait.Until(ExpectedConditions.ElementExists(By.TagName("h1")));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div.wpb_wrapper")));

                string title = _driver.FindElement(By.TagName("h1")).Text.Trim();

                var articleContainer = _driver.FindElement(By.XPath("//div[contains(@class, 'wpb_wrapper') and " +
                    "not(.//div[contains(@class, 'wpb_wrapper')]) and not(.//h3)]"));
                var childElements = articleContainer.FindElements(By.XPath("./*"));

                Article article = new Article()
                {
                    Title = title,
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

                        case "ul":
                            var listItems = HtmlListHelper.ExtractListItems(element);
                            //var listItems = element.FindElements(By.TagName("li"))
                            //    .Select(li => $"- {li.Text.Trim()}").ToList();
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.List,
                                ListItems = listItems
                            });
                            break;

                        case "div":
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.Paragraph,
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