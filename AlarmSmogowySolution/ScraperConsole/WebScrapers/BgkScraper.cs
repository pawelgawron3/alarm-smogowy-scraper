using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperConsole.Helpers;
using ScraperConsole.Models;
using SeleniumExtras.WaitHelpers;

namespace ScraperConsole.WebScrapers;

public class BgkScraper : ScraperBaseClass
{
    private string _jsonString;

    public BgkScraper()
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
                try
                {
                    var cookieBtn = wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("button#onetrust-accept-btn-handler, button.js-cookie-accept, a.cc-btn.cc-dismiss")));
                    cookieBtn.Click();
                }
                catch (WebDriverTimeoutException) { }

                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div.container h1")));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("nav.tabs__nav")));

                string title = _driver.FindElement(By.CssSelector("div.container h1")).Text.Trim();

                Article article = new Article()
                {
                    Title = title
                };

                var tabsNav = _driver.FindElement(By.CssSelector("nav.tabs__nav"));
                var articlesContainer = tabsNav.FindElement(By.CssSelector("ul.d-lg-flex"));
                var tabs = articlesContainer.FindElements(By.TagName("li"));

                foreach (var li in tabs)
                {
                    var a = li.FindElement(By.TagName("a"));
                    try
                    {
                        a.Click();
                    }
                    catch (ElementClickInterceptedException)
                    {
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", a);
                    }

                    var tab = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div.tabs__item:not(.d-none)")));

                    wait.Until(d => tab.FindElements(By.CssSelector("h2, div")).Count > 0);

                    var childElements = tab.FindElements(By.XPath("./*"));

                    foreach (var element in childElements)
                    {
                        switch (element.TagName.ToLower())
                        {
                            case "h2":
                                article.Elements.Add(new ArticleElement
                                {
                                    ElementType = ArticleElementType.Header,
                                    Text = element.Text.Trim()
                                });
                                break;

                            case "div":
                                var divChildElements = element.FindElements(By.XPath("./*"));

                                foreach (var divElement in divChildElements)
                                {
                                    switch (divElement.TagName.ToLower())
                                    {
                                        case "p":
                                            article.Elements.Add(new ArticleElement
                                            {
                                                ElementType = ArticleElementType.Paragraph,
                                                Text = divElement.Text.Trim()
                                            });
                                            break;
                                        case "h2":
                                        case "h3":
                                        case "h4":
                                            article.Elements.Add(new ArticleElement
                                            {
                                                ElementType = ArticleElementType.Header,
                                                Text = divElement.Text.Trim()
                                            });
                                            break;

                                        case "ul":
                                        case "ol":
                                            var listItems = HtmlListHelper.ExtractListItems(divElement);
                                            article.Elements.Add(new ArticleElement
                                            {
                                                ElementType = ArticleElementType.List,
                                                ListItems = listItems
                                            });
                                            break;
                                    }
                                }
                                break;
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