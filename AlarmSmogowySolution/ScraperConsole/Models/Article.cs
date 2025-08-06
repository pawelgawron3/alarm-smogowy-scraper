namespace ScraperConsole.Models;
public class Article
{
    public string Title { get; set; }
    public List<ArticleElement> Elements { get; set; } = new List<ArticleElement>();

}
