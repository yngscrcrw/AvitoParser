using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;

class Driver
{
    IWebDriver driver;

    public void CreateDriver(string url)
    {
        var options = new ChromeOptions();
        //options.AddArgument("--headless");
        options.AddArgument("--start-maximized");
        driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl(url);
    }
    public void deleteDriver()
    {
        driver.Quit();
        driver.Dispose();
    }
    public IWebDriver getDriver()
    { return driver; }
}
class Parser
{
    private string url;
    private int pagesNum;
    public Parser(string url, int pagesNum)
    {
        this.url = url;
        this.pagesNum = pagesNum;
    }
    public void Parse()
    {
        Driver pageDriver = new Driver();
        pageDriver.CreateDriver(url);

        Page page = new Page();
        page.ParsePage(pageDriver.getDriver());
    }
}
class Page
{
    public void ParsePage(IWebDriver driver)
    {
        var adElements = driver.FindElements(By.ClassName("iva-item-list-rfgcH"));

        List<Ad> ads = new List<Ad>();

        foreach (var adElement in adElements)
        {
            Ad ad = new Ad();
            ad.title = adElement.FindElement(By.CssSelector("h3[itemprop='name']")).Text;
            ad.link = adElement.FindElement(By.CssSelector("a[itemprop='url']")).GetAttribute("href");

            try
            {
                ad.sellerName = adElement.FindElement(By.CssSelector(".style-root-uufhX p.styles-module-root-_KFFt")).Text;
            }
            catch
            {
                ad.sellerName = "Нет";
            }
            try
            {
                Actions actions = new Actions(driver);
                var tooltipElement = adElement.FindElement(By.ClassName("styles-arrow-jfRdd"));
                actions.MoveToElement(tooltipElement).Perform();
                var promotions = driver.FindElements(By.ClassName("styles-entry-MuP_G"));

                foreach (var promotion in promotions)
                {
                    var promotionText = promotion.Text;
                    if (promotionText == "Продвинуто")
                    {
                        var imgSrc = promotion.FindElement(By.CssSelector("img")).GetAttribute("src");
                        if (Regex.IsMatch(imgSrc, "https://www.avito.st/s/common/components/monetization/icons/web/x20"))
                        {
                            promotionText = "Продвинуто x20";
                        }
                        else if (Regex.IsMatch(imgSrc, "https://www.avito.st/s/common/components/monetization/icons/web/x5"))
                        {
                            promotionText = "Продвинуто x5";
                        }
                        else if (Regex.IsMatch(imgSrc, "https://www.avito.st/s/common/components/monetization/icons/web/x10"))
                        {
                            promotionText = "Продвинуто x10";
                        }
                        else if (Regex.IsMatch(imgSrc, "https://www.avito.st/s/common/components/monetization/icons/web/x15"))
                        {
                            promotionText = "Продвинуто x15";
                        }
                        else if (Regex.IsMatch(imgSrc, "https://www.avito.st/s/common/components/monetization/icons/web/x2."))
                        {
                            promotionText = "Продвинуто x2";
                        }
                    }
                    ad.promotions += promotionText + ' ';
                }
            }
            catch
            {
                ad.promotions = "Нет";
            }

            AdPage adPage = new AdPage();
            adPage.ParseAdPage(ad.link, ad);

            ads.Append(ad);
        }
        foreach (Ad ad in ads)
        {
            ad.Print();
        }
    }
}
class AdPage
{
    public void ParseAdPage(string url, Ad ad)
    {
        Driver adDriver = new Driver();
        adDriver.CreateDriver(url);

        try
        {
            var viewsElement = adDriver.getDriver().FindElement(By.XPath("//span[@data-marker='item-view/total-views']")).Text;
            ad.views = int.Parse(Regex.Match(viewsElement, @"\d+").Value);

            var todayViewsElement = adDriver.getDriver().FindElement(By.XPath("//span[@data-marker='item-view/today-views']")).Text;
            ad.todayViews = int.Parse(Regex.Match(todayViewsElement, @"\d+").Value);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            adDriver.deleteDriver();
        }
    }
}
class Ad
{
    public string title { get; set; } = " ";
    public string sellerName { get; set; } = " ";
    public string link { get; set; } = " ";
    public string promotions { get; set; } = " ";
    public int views { get; set; } = -1;
    public int todayViews { get; set; } = -1;
    public void Print()
    {
        Console.WriteLine($"{title}|{sellerName}|{promotions} | {views} | {todayViews}");
    }
}
public class Program
{
    public static void Main()
    {
        Parser parser = new Parser("https://www.avito.ru/moskva_i_mo/predlozheniya_uslug/uborka_klining-ASgBAgICAUSYC7L3AQ?cd=1&q=%D0%BA%D0%BB%D0%B8%D0%BD%D0%B8%D0%BD%D0%B3", 5);
        parser.Parse();
    }
}
