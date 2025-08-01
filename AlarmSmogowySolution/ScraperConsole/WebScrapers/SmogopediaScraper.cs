using System.Text.Json;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class SmogopediaScraper : ScraperBaseClass
{
    private string jsonString;

    public SmogopediaScraper()
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

                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("h1#firstHeading")));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div.mw-parser-output")));

                string title = _driver.FindElement(By.CssSelector("h1#firstHeading")).Text.Trim();

                var articleContainer = _driver.FindElement(By.CssSelector("div.mw-parser-output"));
                var childElements = articleContainer.FindElements(By.XPath("./*"));

                List<string> contentBuilder = new List<string>();

                foreach (var element in childElements)
                {
                    switch (element.TagName.ToLower())
                    {
                        case "p":
                            string cleanText = Regex.Replace(element.Text.Trim(), @"\[\d+\]", "");
                            contentBuilder.Add(cleanText);
                            break;

                        case "h1":
                            contentBuilder.Add(element.FindElement(By.CssSelector("span.mw-headline")).Text.Trim());
                            break;

                        case "ul":
                            var listItems = element.FindElements(By.TagName("li"))
                                .Select(li => $"- {Regex.Replace(li.Text.Trim(), @"\[\d+\]", "")}");
                            contentBuilder.Add(string.Join("\n", listItems));
                            break;
                    }
                }

                string content = string.Join("\n", contentBuilder.ToArray());

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
}