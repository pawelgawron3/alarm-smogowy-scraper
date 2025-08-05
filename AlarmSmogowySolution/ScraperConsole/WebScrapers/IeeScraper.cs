using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class IeeScraper : ScraperBaseClass
{
    private string jsonString;

    public IeeScraper()
    {
        jsonString = File.ReadAllText(jsonPath);
    }

    public override List<Article> StartScraping(string url)
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

                var articleContainer = _driver.FindElement(By.XPath("//div[contains(@class, 'wpb_wrapper') and " +
                    "not(.//div[contains(@class, 'wpb_wrapper')]) and not(.//h3)]"));
                var childElements = articleContainer.FindElements(By.XPath("./*"));

                List<string> contentBuilder = new List<string>();

                foreach (var element in childElements)
                {
                    switch (element.TagName.ToLower())
                    {
                        case "p":
                            contentBuilder.Add(element.Text.Trim());
                            break;

                        case "ul":
                            var listItems = element.FindElements(By.TagName("li"))
                                .Select(li => $"- {li.Text.Trim()}");
                            contentBuilder.Add(string.Join("\n", listItems));
                            break;
                        case "div":
                            contentBuilder.Add(element.Text.Trim());
                            break;
                    }
                }

                string content = string.Join("\n", contentBuilder.ToArray());

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
}