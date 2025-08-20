using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using ScraperConsole.Models;

namespace ScraperConsole.Helpers;

public static class PdfHelper
{
    public static void SaveArticlesToPdfs(List<Article> articles, string outputDirectory)
    {
        string downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads"
        );

        string pdfsFolder = Path.Combine(downloadsPath, outputDirectory);

        if (Directory.Exists(pdfsFolder))
        {
            foreach (var file in Directory.GetFiles(pdfsFolder))
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(pdfsFolder);
        }

        try
        {
            foreach (var article in articles)
            {
                string title = MakeFileNameSafe(article.Title);
                string filePath = Path.Combine(pdfsFolder, $"{title}.pdf");

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(40);

                        page.Content()
                            .Column(col =>
                            {
                                col.Item().Text(article.Title)
                                    .FontSize(24)
                                    .FontFamily("Times New Roman")
                                    .SemiBold();

                                col.Item().PaddingBottom(10);

                                foreach (var el in article.Elements)
                                {
                                    switch (el.ElementType)
                                    {
                                        case ArticleElementType.Paragraph:
                                            col.Item().Text(el.Text)
                                            .FontSize(12)
                                            .LineHeight(1.5f);
                                            col.Item().PaddingBottom(5);
                                            break;

                                        case ArticleElementType.Header:
                                            col.Item().Text(el.Text)
                                            .FontSize(16)
                                            .Bold();
                                            col.Item().PaddingBottom(5);
                                            break;

                                        case ArticleElementType.List:
                                            foreach (var item in el.ListItems!)
                                            {
                                                col.Item().Text($"{item}")
                                                .FontSize(12)
                                                .LineHeight(1.5f);
                                            }
                                            col.Item().PaddingBottom(5);
                                            break;

                                        case ArticleElementType.Table:
                                            if (el.TableData != null)
                                            {
                                                col.Item().Table(table =>
                                                {
                                                    int colCount = el.TableData.Max(r => r.Count);

                                                    table.ColumnsDefinition(columns =>
                                                    {
                                                        for (int i = 0; i < colCount; i++)
                                                        {
                                                            columns.RelativeColumn();
                                                        }
                                                    });

                                                    foreach (var row in el.TableData)
                                                    {
                                                        foreach (var cell in row)
                                                        {
                                                            table.Cell()
                                                            .Border(0.5f)
                                                            .BorderColor(Colors.Grey.Lighten1)
                                                            .AlignCenter()
                                                            .AlignMiddle()
                                                            .Padding(0)
                                                            .Text(cell)
                                                            .FontSize(10);
                                                        }
                                                    }
                                                });
                                                col.Item().PaddingBottom(10);
                                            }
                                            break;
                                    }
                                }
                            });
                    });
                });

                document.GeneratePdf(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static string MakeFileNameSafe(string title)
    {
        string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string regexPattern = $"[{invalidChars}]+";
        string safe = Regex.Replace(title, regexPattern, "").Trim();
        return safe.Replace(" ", "_");
    }
}