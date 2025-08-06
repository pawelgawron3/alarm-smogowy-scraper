namespace ScraperConsole.Models;
public class ArticleElement
{
    public ArticleElementType ElementType { get; set; }
    public string? Text { get; set; }
    public List<string>? ListItems { get; set; }
    public List<List<string>>? TableData { get; set; }
}

public enum ArticleElementType
{
    Paragraph,
    Header,
    List,
    Table
}
