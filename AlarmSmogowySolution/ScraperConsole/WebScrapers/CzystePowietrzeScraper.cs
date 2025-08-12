using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class CzystePowietrzeScraper : ScraperBaseClass
{
    private string _jsonString;

    public CzystePowietrzeScraper()
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

                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("h1#articleTitle")));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("article#main-content")));

                string title = _driver.FindElement(By.CssSelector("h1#articleTitle")).Text.Trim();

                var articleContainer = _driver.FindElement(By.CssSelector("article#main-content"));
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
                        case "h3":
                        case "h4":
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.Header,
                                Text = element.Text.Trim()
                            });
                            break;

                        case "ul":
                            var listItems = element.FindElements(By.TagName("li"))
                                .Select(li => $"- {li.Text.Trim()}").ToList();
                            article.Elements.Add(new ArticleElement
                            {
                                ElementType = ArticleElementType.List,
                                ListItems = listItems
                            });
                            break;

                        case "div":
                            var classAttribute = element.GetAttribute("class");
                            if (classAttribute.Contains("table-responsive"))
                            {
                                var tableDom = element.FindElement(By.TagName("table"));

                                var thead = tableDom.FindElement(By.TagName("thead"));
                                var tbody = tableDom.FindElement(By.TagName("tbody"));

                                var theadRows = thead.FindElements(By.TagName("tr"));
                                var tbodyRows = tbody.FindElements(By.TagName("tr"));

                                var table = new List<List<string>>();

                                foreach (var row in theadRows)
                                {
                                    var cells = row.FindElements(By.XPath("./th"))
                                        .Select(el => el.Text.Trim())
                                        .ToList();
                                    table.Add(cells);
                                }

                                foreach (var row in tbodyRows)
                                {
                                    var cells = row.FindElements(By.XPath("./td"))
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
                            else
                            {
                                continue;
                            }
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