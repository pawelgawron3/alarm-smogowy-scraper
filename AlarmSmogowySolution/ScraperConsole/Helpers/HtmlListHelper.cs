using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace ScraperConsole.Helpers;

public static class HtmlListHelper
{
    public static List<string> ExtractListItems(IWebElement listElement, int level = 0)
    {
        List<string> items = new List<string>();
        bool isOrdered = false;
        int index = 1;

        string tagName = listElement.TagName.ToLower();
        if (tagName == "ol")
        {
            isOrdered = true;
        }

        var liElements = listElement.FindElements(By.XPath("./li"));


        foreach (var li in liElements)
        {
            string liText = li.Text.Trim();

            var nestedList = li.FindElements(By.XPath("./ul | ./ol")).FirstOrDefault();
            if (nestedList != null)
            {
                string nestedText = nestedList.Text.Trim();
                if (!string.IsNullOrEmpty(nestedText))
                {
                    liText = liText.Replace(nestedText, "").Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(liText))
            {
                string prefix;
                if (isOrdered)
                {
                    prefix = new string(' ', level * 5) + $"{index}. ";
                }
                else
                {
                    prefix = new string(' ', level * 5) + "- ";
                }

                items.Add(prefix + Regex.Replace(liText, @"\[\d+\]", ""));
            }

            if (nestedList != null)
            {
                items.AddRange(ExtractListItems(nestedList, level + 1));
            }

            index++;
        }

        return items;
    }
}