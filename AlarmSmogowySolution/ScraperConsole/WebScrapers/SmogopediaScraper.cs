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
                                Text = Regex.Replace(element.Text.Trim(), @"\[\d+\]", "")
                            });
                            break;

                        case "h1":
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.Header,
                                Text = element.FindElement(By.CssSelector("span.mw-headline")).Text.Trim()
                            });
                            break;

                        case "ul":
                            var listItems = element.FindElements(By.TagName("li"))
                                .Select(li => $"- {Regex.Replace(li.Text.Trim(), @"\[\d+\]", "")}").ToList();
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.List,
                                ListItems = listItems
                            });
                            break;

                        case "table":
                            var tbody = element.FindElement(By.TagName("tbody"));
                            var rows = tbody.FindElements(By.TagName("tr"));

                            var table = new List<List<string>>();

                            foreach(var row in rows)
                            {
                                var cells = row.FindElements(By.XPath("./th | ./td"))
                                    .Select(el => el.Text.Trim())
                                    .ToList();
                                table.Add(cells);
                            }

                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.Table,
                                TableData = table
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