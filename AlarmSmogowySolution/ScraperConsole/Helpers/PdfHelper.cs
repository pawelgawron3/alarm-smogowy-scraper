using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using ScraperConsole.Models;

namespace ScraperConsole.Helpers;

public static class PdfHelper
{
    public static void SaveArticlesToPdfs(List<Article> articles, string outputDirectory = "PDFs")
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
                                    .FontSize(18)
                                    .FontFamily("Times New Roman")
                                    .SemiBold();

                                col.Item().PaddingBottom(10);

                                var lines = article.Content.Split('\n');

                                foreach (var line in lines)
                                {
                                    col.Item().Text(line)
                                        .FontSize(12)
                                        .FontFamily("Times New Roman")
                                        .LineHeight(1.5f);
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