using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Net;
using UglyToad.PdfPig;
using System.Globalization;

class Program
{

    //has to be async to await the url
    public static async System.Threading.Tasks.Task Main()
    {
        string DiskLink = "E:/moties/";

        //cleanData(DiskLink);
        await getData(DiskLink);


        Console.WriteLine("Scraping has succeeded");
        Environment.Exit(0);
    }

    public static void cleanData(string DiskLink)
    {
        Database db = new Database();
        //Clean map moties
        try
        {
            DirectoryInfo di = new DirectoryInfo(DiskLink);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            if (!Directory.EnumerateFileSystemEntries(DiskLink).Any())
            {
                Console.WriteLine("All files removed");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }


        //Cleaning database
        try
        {
            db.TruncateDatabase();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    //visiting all the motie pages and getting all the data from them
    public static async System.Threading.Tasks.Task getData(string urlToLocalPDF)
    {

        // get html page source
        var httpClient = new HttpClient();


        //For all 730 moties, loop:
        //OPTIONAL: MAKE 730 RESPONSIVE
        var urlOverviewMoties = "";
        var urlOverviewMotiesBase = "https://www.tweedekamer.nl/kamerstukken/moties?qry=klimaat&fld_prl_kamerstuk=Moties&fld_tk_categorie=kamerstukken&sta=";

        for (int i = 1; i < 730; i += 15)
        {
            //increment of 15, because 15 moties per page

            //Create url with increment
            urlOverviewMoties = urlOverviewMotiesBase;
            urlOverviewMoties += i;

            // get html page source
            await httpClient.GetStringAsync(urlOverviewMoties);
            var pageOverviewMoetiesHTML = await httpClient.GetStringAsync(urlOverviewMoties);
            var pageOverviewMotiesDoc = new HtmlDocument();
            pageOverviewMotiesDoc.LoadHtml(pageOverviewMoetiesHTML);


            //Each element into list
            var allNodes = pageOverviewMotiesDoc.DocumentNode.SelectNodes("//html/body/div[1]/div/div[2]/div/div[3]/div[1]/div").ToList();

            foreach (var singleNode in allNodes)
            {
                if (singleNode.Attributes[0].Value.Equals("card"))
                {
                    //get part of link, to find the motie
                    var urlSpecificMotieID = singleNode.ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[0].Attributes[0].Value;

                    //get ID and DID from link
                    int fromText = urlSpecificMotieID.IndexOf("id=") + "id=".Length;
                    int toText = urlSpecificMotieID.LastIndexOf("&");

                    string motieID = urlSpecificMotieID.Substring(fromText, toText - fromText);
                    string motieDID = urlSpecificMotieID.Substring(Math.Max(0, urlSpecificMotieID.Length - 10));


                    //create new link
                    var urlSpecificMotieBase = "https://www.tweedekamer.nl";
                    var urlSpecificMotie = urlSpecificMotieBase + urlSpecificMotieID;

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
                        string urlToLocalPDF2 = urlToLocalPDF + "" + motieID + "" + motieDID + ".pdf";


                        //If file is not downloaded yet
                        if (!File.Exists(urlToLocalPDF2))
                        {
                            //Get specific data of each motie

                            //get motietitle
                            string UnConcattedtitle = titleSpecificMotie[0].InnerText;
                            string cleanTitle = UnConcattedtitle.Replace("\n", "").Trim();


                            //If file is not downloaded yet, so that we dont redo work
                            if (!File.Exists(urlToLocalPDF2))
                            {
                                //Get votes of motie
                                int mensenVoor = int.Parse(mainPageSpecificMotie[0].ChildNodes[5].Attributes[1].Value);



                                //get who voted positive
                                string votedPositive = getVotedPositive(docSpecificMotie.DocumentNode.SelectNodes("/html/body/div[1]/div/section[2]/div/section/div/div[2]/div/div/div/div[3]/div/div[1]/table/tbody"));

                                //get who voted negative
                                string votedNegative = getVotedNegative(docSpecificMotie.DocumentNode.SelectNodes("/html/body/div[1]/div/section[2]/div/section/div/div[2]/div/div/div/div[3]/div/div[2]/table/tbody"));



                                //get date of motie
                                var dateMotieString = docSpecificMotie.DocumentNode.SelectNodes("/html/body/div[1]/div/section[1]/div/div/div[2]/div/div[1]/div[2]");
                                DateTime dateMotieReversed = DateTime.Parse(dateMotieString[0].ChildNodes[0].InnerText);
                                string dateMotie = dateMotieReversed.ToString("yyyy-MM-dd");

                                //Go download pdf of motie
                                var urlDownloadPDFButton = docSpecificMotie.DocumentNode.SelectSingleNode("/html/body/div[1]/div/section[1]/div/div/div[1]/div/a");
                                string urlToDownloadPDF = urlSpecificMotieBase + urlDownloadPDFButton.Attributes[1].Value;


                                if (DownloadPDF(urlToDownloadPDF, urlToLocalPDF2))
                                {

                                    //Get omschrijving from PDF
                                    string omschrijving = getOmschrijvingFromPDF(urlToLocalPDF2);
                                    omschrijving = omschrijving.Substring(omschrijving.IndexOf("De Kamer, gehoord de beraadslaging,"));
                                    omschrijving = omschrijving.Remove(omschrijving.IndexOf("en gaat over tot de orde van de dag.") + 36);
                                    omschrijving = omschrijving.Replace("'", "''");

                                    //Write data to db
                                    WriteData(motieID, motieDID, cleanTitle, omschrijving, mensenVoor, dateMotie, votedPositive, votedNegative);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Motie is al gedownload");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Geen stemmen voor bekend!");
                        }
                        Console.WriteLine("\n");

                    }
                }
            }

        }

        //Downloading the PDF to server and C:/Users/ivar/source/repos/ConsoleApp2/ConsoleApp2/Moties/ map
        static bool DownloadPDF(string urlToDownloadPDF, string DiskUrl)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(
                        new System.Uri(urlToDownloadPDF),
                        DiskUrl
                    );
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(urlToDownloadPDF);
                Console.WriteLine("Unable to Download : " + ex.ToString());
            }
            return false;
        }

        //getting the motie from the pdf
        static string getOmschrijvingFromPDF(string urlToPDF)
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
    ; return otherText.Replace("'", "''");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "Failed";
                }

            }
        }

        //Getting the date of the motie from the pdf
        static DateTime getDateFromPDF(string urlToPDF)
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
                            Console.WriteLine("PROBLEEM MET PARSEN: " + ex);
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

        //Writing data to database
        static bool WriteData(string id, string did, string title, string description, int stemmenVoor, string motieDatum, string lijstVoor, string lijstTegen)
        {

            Database db = new Database();
            string query = "INSERT INTO motie (id, did, title, omschrijving, stemmenVoor, motieDatum, partijVoor, partijTegen) " +
                            "VALUES('" + id + "', '" + did + "', '" + title + "', '" + description + "', " + stemmenVoor + ", '" + motieDatum + "', '" + lijstVoor + "', '" + lijstTegen + "');";
            db.InsertInto(query);

            return true;
        }









        static string getVotedPositive(HtmlNodeCollection voteTableVoor)
        {
            string final = "";
            if (voteTableVoor != null)
            {
                var tableRow = voteTableVoor[0].ChildNodes;
                foreach (var partyVotedPositive in tableRow)
                {
                    if (!partyVotedPositive.HasAttributes)
                    {
                        if (partyVotedPositive.Name.Equals("tr"))
                        {
                            foreach (var ta in partyVotedPositive.ChildNodes)
                            {
                                if (ta.Name.Equals("td") & ta.ChildNodes.Count > 2)
                                {

                                    if (ta.ChildNodes[1].Name.Equals("span"))
                                    {
                                        if (!ta.ChildNodes[1].HasAttributes)
                                        {
                                            final += ta.ChildNodes[1].InnerHtml + ", ";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return final;
        }


        static string getVotedNegative(HtmlNodeCollection voteTableTegen)
        {
            string final = "";
            if (voteTableTegen != null)
            {
                var tableRow = voteTableTegen[0].ChildNodes;
                foreach (var partyVotedNegative in tableRow)
                {
                    if (!partyVotedNegative.HasAttributes)
                    {
                        if (partyVotedNegative.Name.Equals("tr"))
                        {
                            foreach (var ta in partyVotedNegative.ChildNodes)
                            {
                                if (ta.Name.Equals("td") & ta.ChildNodes.Count > 2)
                                {

                                    if (ta.ChildNodes[1].Name.Equals("span"))
                                    {
                                        if (!ta.ChildNodes[1].HasAttributes)
                                        {
                                            final += ta.ChildNodes[1].InnerHtml + ", ";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return final;
        }

    }
}
