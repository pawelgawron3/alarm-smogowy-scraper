using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace ScraperConsole.Helpers;

public static class HtmlListHelper
{
    public static List<string> ExtractListItems(IWebElement ulElement, int level = 0)
    {
        List<string> items = new List<string>();
        var liElements = ulElement.FindElements(By.XPath("./li"));

        foreach (var li in liElements)
        {
            string liText = li.Text.Trim();

            var nestedUl = li.FindElements(By.XPath("./ul")).FirstOrDefault();
            if (nestedUl != null)
            {
                string nestedText = nestedUl.Text.Trim();
                if (!string.IsNullOrEmpty(nestedText))
                {
                    liText = liText.Replace(nestedText, "").Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(liText))
            {
                string prefix = new string(' ', level * 5) + "- ";
                items.Add(prefix + Regex.Replace(liText, @"\[\d+\]", ""));
            }

            if (nestedUl != null)
            {
                items.AddRange(ExtractListItems(nestedUl, level + 1));
            }
        }

        return items;
    }
}