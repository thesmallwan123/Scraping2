using HtmlAgilityPack;
using System;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Web;
using System.Net;
using UglyToad.PdfPig;

class Program
{
    public string dowloadToDiskURL = "C:/Users/ivar/source/repos/ConsoleApp2/ConsoleApp2/Moties/";


    //has to be async to await the url
    public static async System.Threading.Tasks.Task Main()
    {

        await getData();
        Environment.Exit(0);
    }


    public static async System.Threading.Tasks.Task getData()
    {

        

        // get html page source
        var httpClient = new HttpClient();

        //store the html of the page in a variable
        var pageForAmount = await httpClient.GetStringAsync("https://www.tweedekamer.nl/kamerstukken/moties?qry=klimaat&fld_prl_kamerstuk=Moties&fld_tk_categorie=kamerstukken&sta=");
        var pageForAmount2 = new HtmlDocument();
        pageForAmount2.LoadHtml(pageForAmount);
        var nodes2 = pageForAmount2.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div[2]/div/div[2]/div[1]/h2/span");
        int amountMoties = int.Parse(nodes2.ToString());

        //var maxMotiesRaw = docOverviewMoties.DocumentNode.SelectNodes("/html/body/div[1]/div/div[2]/div/div[2]/div[1]/h2/span").ToList()[0].InnerHtml;

        for (int i = 1; i < amountMoties; i += 15)
        {


            //Set url
            var urlOverviewMoties = "https://www.tweedekamer.nl/kamerstukken/moties?qry=klimaat&fld_prl_kamerstuk=Moties&fld_tk_categorie=kamerstukken&sta=";
            var baseUrlOverviewMoties = "https://www.tweedekamer.nl/kamerstukken/moties?qry=klimaat&fld_prl_kamerstuk=Moties&fld_tk_categorie=kamerstukken&sta=";

            //Create url
            urlOverviewMoties += i;

            // get html page source
            await httpClient.GetStringAsync(urlOverviewMoties);



            //increment of 15, because website shows 15 moties.
            //max 730, because there are 730 total moties. NEEDS TO BE RESPONSIVE!!!
            //store the html of the page in a variable
            var htmlPagesOverviewMoties = await httpClient.GetStringAsync(urlOverviewMoties);
            var docOverviewMoties = new HtmlDocument();
            docOverviewMoties.LoadHtml(htmlPagesOverviewMoties);






            //Each element into list
            var nodes = docOverviewMoties.DocumentNode.SelectNodes("//html/body/div[1]/div/div[2]/div/div[3]/div[1]/div").ToList();
            foreach (var element in nodes)
            {
                if (element.Attributes[0].Value.Equals("card"))
                {
                    //get part of link, to find the motie
                    var urlSpecificMotieID = element.ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[0].Attributes[0].Value;

                    //get ID and DID from link
                    int pFrom = urlSpecificMotieID.IndexOf("id=") + "id=".Length;
                    int pTo = urlSpecificMotieID.LastIndexOf("&");

                    string motieID = urlSpecificMotieID.Substring(pFrom, pTo - pFrom);
                    string motieDID = urlSpecificMotieID.Substring(Math.Max(0, urlSpecificMotieID.Length - 10));



                    //create new link
                    var urlSpecificMotieBase = "https://www.tweedekamer.nl";
                    var urlSpecificMotie = urlSpecificMotieBase + urlSpecificMotieID;

                    Console.WriteLine(urlSpecificMotieID);
                    Console.WriteLine(i);
                    //setup new connection
                    var htmlSpecificMotie = await httpClient.GetStringAsync(urlSpecificMotie);

                    var docSpecificMotie = new HtmlDocument();
                    docSpecificMotie.LoadHtml(htmlSpecificMotie);

                    // get global data tabel
                    var mainPageSpecificMotie = docSpecificMotie.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/section[2]/div[1]/section[1]/div[1]/div[2]/div[1]/div[1]/div[1]");
                    var titleSpecificMotie = docSpecificMotie.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/section[1]/div[1]/div[1]/div[1]/div[1]/h1");


                    // check if votes are in
                    if (mainPageSpecificMotie != null)
                    {
                        //Get specific data of each motie
                        //get motietitle
                        string UnConcattedtitle = titleSpecificMotie[0].InnerText;
                        string cleanTitle = UnConcattedtitle.Replace("\n", "").Trim();

                        //Get votes of motie
                        int mensenVoor = int.Parse(mainPageSpecificMotie[0].ChildNodes[5].Attributes[1].Value);

                        //get date of motie
                        var dateMotieString = docSpecificMotie.DocumentNode.SelectNodes("/html/body/div[1]/div/section[1]/div/div/div[2]/div/div[1]/div[2]");
                        DateTime dateMotie = DateTime.Parse(dateMotieString[0].ChildNodes[0].InnerText);

                        //Go download pdf of motie
                        var urlDownloadPDFButton = docSpecificMotie.DocumentNode.SelectSingleNode("/html/body/div[1]/div/section[1]/div/div/div[1]/div/a");
                        string urlToDownloadPDF = urlSpecificMotieBase + urlDownloadPDFButton.Attributes[1].Value;


                        string urlToLocalPDF = "C:/Users/ivar/source/repos/ConsoleApp2/ConsoleApp2/Moties/" + motieID + "" + motieDID + ".pdf";

                        //downloadpdf
                        if (!File.Exists(urlToLocalPDF))
                        {
                            if (DownloadPDF(urlToDownloadPDF, motieID, motieDID))
                            {

                                //Get omschrijving from PDF
                                string omschrijving = getOmschrijvingFromPDF(urlToLocalPDF);
                                omschrijving = omschrijving.Replace("'", "''");
                                //Write data to db
                                WriteData(motieID, motieDID, cleanTitle, omschrijving, mensenVoor, dateMotie);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Motie bestaat al in dictionary");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Geen stemmen voor bekend!");
                    }
                    Console.WriteLine("\n");

                }
            }
            urlOverviewMoties = baseUrlOverviewMoties;
        }

    }

    public static bool DownloadPDF(string urlToDownloadPDF, string id, string did)
    {
        try
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(
                    new System.Uri(urlToDownloadPDF),
                    "C:/Users/ivar/source/repos/ConsoleApp2/ConsoleApp2/Moties/" + id + "" + did + ".pdf"
                );
            }
            Console.WriteLine("Succes downloaded PDF!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(urlToDownloadPDF);
            Console.WriteLine("Unable to Download : " + ex.ToString());
        }
        return false;
    }






    public static DateTime getDateFromPDF(string urlToPDF)
    {
        using (var pdf = PdfDocument.Open(urlToPDF))
        {
            foreach (var page in pdf.GetPages())
            {
                // Or the raw text of the page's content stream.
                var otherText = string.Join(" ", page.GetWords());

                
                int pFrom = otherText.IndexOf("Voorgesteld ") + "Voorgesteld ".Length;
                int pTo = otherText.LastIndexOf(" De Kamer, ");

                string motieDate = otherText.Substring(pFrom, pTo - pFrom);

                int number;
                if (int.TryParse(motieDate.First().ToString(), out number))
                {
                    try
                    {
                        return DateTime.Parse(motieDate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("PROBLEEM MET PARSEN: "+ex);
                    }
                }
                else
                {
                    motieDate = motieDate.Substring(motieDate.IndexOf(" van ") + " van ".Length);

                    int number2;
                    if (int.TryParse(motieDate.First().ToString(), out number2))
                    {
                        try
                        {
                            return DateTime.Parse(motieDate);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("PROBLEEM MET PARSEN: " + ex);
                        }
                    }
                    else
                    {
                        Console.WriteLine("BEGINT NOG STEEDS FOUT: " + motieDate);
                        Console.WriteLine();
                    }
                }
                               
            }
        }
        return DateTime.Parse("01 jan 0001");
    }





    public static string getOmschrijvingFromPDF(string urlToPDF)
    {
        using (var pdf = PdfDocument.Open(urlToPDF))
        {
            try
            {
                var otherText = "";
                foreach (var page in pdf.GetPages())
                {
                    // Or the raw text of the page's content stream.
                    otherText += string.Join(" ", page.GetWords());
                }
                int pFrom = otherText.IndexOf("De Kamer, gehoord de beraadslaging, ") + "De kamer, gehoord de beraadslaging, ".Length;
                int pTo = otherText.LastIndexOf(" en gaat over tot de orde van de dag.");

                string motieDescription = otherText.Substring(pFrom, pTo - pFrom);
                return motieDescription;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Failed";
            }

        }
    }


    public static bool WriteData(string id, string did, string title, string description, int stemmenVoor, DateTime motieDatum)
    {

        Database db = new Database();
        string query = "INSERT INTO motie (id, did, title, omschrijving, stemmenVoor, motieDatum) VALUES('" + id + "', '" + did + "', '" + title + "', '" + description + "', " + stemmenVoor + ", '" + motieDatum.ToShortDateString() + "');";
        db.InsertInto(query);

        return true;
    }

}
