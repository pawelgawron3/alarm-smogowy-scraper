using QuestPDF.Infrastructure;
using ScraperConsole.Controllers;

namespace ScraperConsole
{
    public class Program
    {
        private static void Main(string[] args)
        {
            /*
            * QuestPDF requires explicitly selecting a license type.
            * This line sets the license to "Community", which is free to use
            * for individuals, open-source projects, and organizations with
            * annual gross revenue below $1M USD.
            *
            * For more details, see: https://www.questpdf.com/license/
            */
            QuestPDF.Settings.License = LicenseType.Community;

            //Testing
            //const string url = "https://iee.org.pl";
            //const string url2 = "https://smogopedia.pl";
            //const string url3 = "https://smoglab.pl";
            const string url4 = "https://czystepowietrze.gov.pl/";

            ScraperController controller = new ScraperController();

            controller.Scrape(url4);
        }
    }
}