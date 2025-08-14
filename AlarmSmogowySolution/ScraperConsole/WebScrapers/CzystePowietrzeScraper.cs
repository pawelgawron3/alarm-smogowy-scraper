using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Helpers;
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

                string title = _driver.FindElement(By.CssSelector("h1#articleTitle")).Text.Trim();

                Article article = new Article()
                {
                    Title = title
                };

                IWebElement articleContainer;

                try
                {
                    wait.Until(ExpectedConditions.ElementExists(By.CssSelector("article#main-content")));
                    articleContainer = _driver.FindElement(By.CssSelector("article#main-content"));
                }
                catch (WebDriverTimeoutException)
                {
                    wait.Until(ExpectedConditions.ElementExists(By.CssSelector("main#main div.content")));
                    articleContainer = _driver.FindElement(By.CssSelector("main#main div.content"));
                    ScrapeDynamicContent(article, wait);
                }

                var childElements = articleContainer.FindElements(By.XPath("./*"));

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
                            var classAttribute = element.GetAttribute("class") ?? string.Empty;
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
                            }
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

    private void ScrapeDynamicContent(Article article, WebDriverWait wait)
    {
        var rowDom = _driver.FindElement(By.CssSelector("main#main div.row"));
        var faq_accordion = rowDom.FindElement(By.CssSelector("div#faq-accordion"));
        var cardsContainer = faq_accordion.FindElements(By.CssSelector("div.card"));

        foreach (var card in cardsContainer)
        {
            var button = card.FindElement(By.TagName("button"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", button);
            wait.Until(ExpectedConditions.ElementToBeClickable(button));

            try
            {
                button.Click();
            }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", button);
            }

            article.Elements.Add(new ArticleElement
            {
                ElementType = ArticleElementType.Paragraph,
                Text = button.Text.Trim()
            });

            var content = card.FindElement(By.CssSelector("div.card-body div.content"));

            var cardChildElements = content.FindElements(By.XPath("./*"));

            foreach (var element in cardChildElements)
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
                        var listItems = element.FindElements(By.TagName("li"))
                            .Select(li => $"- {li.Text.Trim()}").ToList();
                        article.Elements.Add(new ArticleElement
                        {
                            ElementType = ArticleElementType.List,
                            ListItems = listItems
                        });
                        break;

                    case "table":
                        var thead = element.FindElement(By.TagName("thead"));
                        var tbody = element.FindElement(By.TagName("tbody"));

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
            }
        }
    }
}