
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using System.Net;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        //Proměnné
        List<Subject> predmety = new List<Subject>();
        string[] userData = { "", "" };
        string sem = "";
        string uBrow = "";
        int numero = 0;
        bool developer = false;
        bool allView = false;
        bool skip = false;
        bool nextTry = false;
        List<string> vFakulty = new List<string>();
        IWebDriver cd;
        List<char> alphabeth = new List<char>(("aábcčdďeéěfghiíjklnňmoópqrřsštťuúůvwxyýzž").ToCharArray());
        IList<IWebElement> faculties = new List<IWebElement>();


        Console.WriteLine("Start!");
        Console.WriteLine("Zadejte argumenty nebo zmáčkněte enter: (? pro info)");

        //Argumenty
        try
        {
            string argumenty = Console.ReadLine();
            if (argumenty is null)
                argumenty = "";
            if (argumenty.Contains("?"))
            {
                Console.WriteLine("-u [přihlašovací jméno]\n" +
                    "-p [přihlašovací heslo]\n" +
                    "-b (testovací verze programu)\n" +
                    "-s [semestr (typ: B-rok-pololetí; př: B221)]\n" +
                    "-v [1-chrome; 2-Edge; 3-Firefox]\n" +
                    "-count [počet předmětů k vylistování]\n" +
                    "-skip (program přeskočí přihlašování z konzole a počká na přihlášení v prohlížeči)\n" +
                    "-from [aábcčdďeéěfghiíjklnňmoópqrřsštťuúůvwxyýzž] (výběr počátečního písmena)\n" +
                    "-to [aábcčdďeéěfghiíjklnňmoópqrřsštťuúůvwxyýzž] (výběr závěrečného písmena)");
                argumenty = Console.ReadLine();
                if (argumenty is null)
                    argumenty = "";
            }
            var argument = argumenty.Split("-");
            bool[] finish = { false, false };
            foreach (string arg in argument)
            {
                List<char> tempAlph = new List<char>();
                var cmm = arg.Split(" ");
                switch (cmm[0])
                {
                    case "u": //uzivatel
                        userData[0] = cmm[1];
                        break;
                    case "p": //heslo
                        userData[1] = cmm[1];
                        break;
                    case "b": //test
                        numero = 30;
                        developer = true;
                        Console.WriteLine("Vstupujete do testovacího módu");
                        break;
                    case "d": //developer - odemiká rozšířený pohled
                        developer = true;
                        allView = true;
                        Console.WriteLine("Vstupujete do vývojářského módu");
                        break;
                    case "from" or "f": //Od tohoto písmene začne počítání předmětů
                        char fromSel = new char();
                        char.TryParse(cmm[1], out fromSel);
                        alphabeth.Reverse();
                        foreach (char c in alphabeth)
                        {
                            if (!finish[0])
                            {
                                finish[0] = c.Equals(fromSel);
                                tempAlph.Add(c);
                            }
                        }
                        alphabeth = tempAlph;
                        alphabeth.Reverse();
                        break;
                    case "to" or "t": //Do tohoto písmene se počítají předměty
                        char toSel = new char();
                        char.TryParse(cmm[1], out toSel);
                        foreach (char c in alphabeth)
                        {
                            if (!finish[1])
                            {
                                finish[1] = c.Equals(toSel);
                                tempAlph.Add(c);
                            }
                        }
                        alphabeth = tempAlph;
                        break;
                    case "s": //semestr
                        sem = cmm[1];
                        break;
                    case "count": //počet předmětů
                        int.TryParse(cmm[1], out numero);
                        break;
                    case "skip":
                        skip = true;
                        break;
                    case "v":
                        uBrow = cmm[1];
                        break;
                }
            }
        }
        catch (Exception ex) { Debug.WriteLine(ex, "arguments"); }

        //Otevření okna prohlížeče
        if (uBrow.Length == 0)
        {
            Console.WriteLine("Vyberte prohlížeč: (výchozí: Chrome)\n" +
                "1. Google Chrome\n" +
                "2. MS Edge\n" +
                "3. Firefox");
            uBrow = Console.ReadLine();
        }
        try
        { //Pokus o spuštění prohlížeče
            switch (uBrow)
            {
                case "2":
                    EdgeDriverService eDService = EdgeDriverService.CreateDefaultService();
                    EdgeOptions eOptions = new EdgeOptions();
                    eOptions.AcceptInsecureCertificates = false;
                    eOptions.PageLoadStrategy = PageLoadStrategy.Normal;
                    if (!allView)
                    {
                        eDService.EnableVerboseLogging = false;
                        eDService.SuppressInitialDiagnosticInformation = true;
                        eDService.HideCommandPromptWindow = true;
                        if (!skip)
                            eOptions.AddArgument("--headless");
                        eOptions.AddArgument("--no-sandbox");
                        eOptions.AddArgument("--disable-gpu");
                        eOptions.AddArgument("--disable-crash-reporter");
                        eOptions.AddArgument("--disable-extensions");
                        eOptions.AddArgument("--disable-in-process-stack-traces");
                        eOptions.AddArgument("--disable-logging");
                        eOptions.AddArgument("--disable-dev-shm-usage");
                        eOptions.AddArgument("--log-level=3");
                        eOptions.AddArgument("--output=/dev/null");
                    }
                    cd = new EdgeDriver(eDService, eOptions);
                    break;
                case "3":
                    FirefoxDriverService fDService = FirefoxDriverService.CreateDefaultService();
                    FirefoxOptions fOptions = new FirefoxOptions();
                    fOptions.AcceptInsecureCertificates = false;
                    fOptions.PageLoadStrategy = PageLoadStrategy.Normal;
                    if (!allView)
                    {
                        fDService.SuppressInitialDiagnosticInformation = true;
                        fDService.HideCommandPromptWindow = true;
                        if (!skip)
                            fOptions.AddArgument("--headless");
                        fOptions.AddArgument("--no-sandbox");
                        fOptions.AddArgument("--disable-gpu");
                        fOptions.AddArgument("--disable-crash-reporter");
                        fOptions.AddArgument("--disable-extensions");
                        fOptions.AddArgument("--disable-in-process-stack-traces");
                        fOptions.AddArgument("--disable-logging");
                        fOptions.AddArgument("--disable-dev-shm-usage");
                        fOptions.AddArgument("--log-level=3");
                        fOptions.AddArgument("--output=/dev/null");
                    }
                    cd = new FirefoxDriver(fDService, fOptions);
                    break;
                default:
                    ChromeDriverService cDService = ChromeDriverService.CreateDefaultService();
                    ChromeOptions cOptions = new ChromeOptions();
                    cOptions.AcceptInsecureCertificates = false;
                    cOptions.PageLoadStrategy = PageLoadStrategy.Normal;
                    if (!allView)
                    {
                        cDService.EnableVerboseLogging = false;
                        cDService.SuppressInitialDiagnosticInformation = true;
                        cDService.HideCommandPromptWindow = true;
                        if (!skip)
                            cOptions.AddArgument("--headless");
                        cOptions.AddArgument("--no-sandbox");
                        cOptions.AddArgument("--disable-gpu");
                        cOptions.AddArgument("--disable-crash-reporter");
                        cOptions.AddArgument("--disable-extensions");
                        cOptions.AddArgument("--disable-in-process-stack-traces");
                        cOptions.AddArgument("--disable-logging");
                        cOptions.AddArgument("--disable-dev-shm-usage");
                        cOptions.AddArgument("--log-level=3");
                        cOptions.AddArgument("--output=/dev/null");
                    }
                    cd = new ChromeDriver(cDService, cOptions);
                    break;
            }

        }
        catch (Exception ex) { Console.WriteLine("\nChybná verze webdriveru k prohlížeči (nebo prohlížeče). Stáhněte nebo vyberte správnou\n\n" + ex); return; }

        //Přihlášení do aplikace
        cd.Navigate().GoToUrl(@"https://new.kos.cvut.cz/login");
        string nameS = "";
        try
        {
            while (cd.Url == @"https://new.kos.cvut.cz/login")
            {
                if (skip)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Thread.Sleep(1000);
                    IWebElement e = cd.FindElement(By.Id("username"));
                    if (!developer)
                        Console.Clear();
                    if (!nextTry)
                    {
                        nextTry = true;
                        if (userData[0].Length < 3)
                        {
                            for (int i = 0; i < 50; i++)
                                e.SendKeys("\b");
                            Console.WriteLine("Zadejte přihlašovají jméno:");
                            nameS = Console.ReadLine();
                            if (nameS is null)
                                nameS = "";
                            e.SendKeys(nameS);
                        }
                        else { nameS = userData[0]; e.SendKeys(userData[0]); }
                    }
                    else
                    {
                        Console.WriteLine("Špatně zadané údaje!\nUživatel: " + nameS);
                    }
                    e = cd.FindElement(By.Id("password"));
                    if (userData[1].Length < 8)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        Console.WriteLine("Heslo: ");
                        for (int i = 0; i < 50; i++)
                            e.SendKeys("\b");
                        while (true)
                        {
                            ConsoleKeyInfo newKey = Console.ReadKey(true);
                            char passwordKey = newKey.KeyChar;
                            if (newKey.Key == ConsoleKey.Backspace)
                            { nextTry = false; }
                            if (newKey.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else
                            {
                                if ((newKey.Key == ConsoleKey.Backspace) || (newKey.Key == ConsoleKey.Delete))  //podmínka pro zobrazování "*" místo znaků
                                { Console.Write("\b"); }
                                else { Console.Write("*"); }
                                stringBuilder.Append(passwordKey.ToString());
                            }
                        }
                        e.SendKeys(stringBuilder.ToString());
                    }
                    else { e.SendKeys(userData[1]); }
                    if (!developer)
                        Console.Clear();
                    e = cd.FindElement(By.CssSelector("button[data-testid='button-login']"));
                    e.Click();
                    Thread.Sleep(1500);
                }
            }
        }
        catch (Exception ex) { Debug.WriteLine("Chyba: " + ex, "Login"); }

        //Nedosažitelná výjimka
        if (cd.Url == @"https://new.kos.cvut.cz/login")
        {
            cd.Close();
            Console.WriteLine("Přihlášení neproběhlo úspěšně, spusťe program znovu (zkuste argument -skip pro neomezený počet pokusů přihlášení v prohlížeči)");
            return;
        }
        string UserNAME = cd.FindElement(By.CssSelector("span[data-testid='fullname']")).Text;
        Console.WriteLine("\nPřihlášen jako uživatel: " + UserNAME);

        //Cookies
        CookieContainer cc = new CookieContainer();
        foreach (OpenQA.Selenium.Cookie c in cd.Manage().Cookies.AllCookies)
        {
            string name = c.Name;
            string value = c.Value;
            cc.Add(new System.Net.Cookie(name, value, c.Path, c.Domain));
        }
        cd.Url = @"https://new.kos.cvut.cz/course-register";
        cd.Navigate();
        Thread.Sleep(1000);

        //Zápis semestru (pokud není předdefinováno)
        try
        {
            IWebElement semesterSel = cd.FindElement(By.CssSelector("div[data-testid='select-semester']"));
            IWebElement clickSem = semesterSel.FindElement(By.CssSelector("svg[class='svg-inline--fa fa-caret-down fa-lg multiselect__caret-icon']"));
            clickSem.Click();
            IList<IWebElement> semesterList = semesterSel.FindElement(By.CssSelector("ul[class='multiselect__content']")).FindElements(By.CssSelector("li[class='multiselect__element']"));
            if (sem == "")
            {
                string[] dropSemesters = new string[semesterList.Count];
                Console.WriteLine("Vyber semestr:");
                for (int i = 0; i < semesterList.Count; i++)
                {
                    Console.WriteLine($"{i}. {semesterList[i].Text}");
                    dropSemesters[i] = semesterList[i].Text.Replace("&nbsp", "").Replace(" ", "");
                }
                int idSem = 0;
                int.TryParse(Console.ReadLine(), out idSem);
                if (idSem < dropSemesters.Length)
                {
                    var fText = dropSemesters[idSem].Split("-");
                    foreach (string s in fText)
                        if (s.StartsWith("B"))
                        {
                            sem = s;
                            break;
                        }
                    Console.WriteLine("Vybráno: " + sem);
                }
                if (sem is null)
                    sem = "";
            }
            clickSem.Click();
            foreach (IWebElement drop in semesterList)
            {
                if (drop.Text.Contains(sem))
                    drop.Click();
            }
            Thread.Sleep(1500);
        }
        catch (Exception) { }

        //Část pro výběr fakulty
        Console.WriteLine("Přejete si vybrat fakultu? [Y/N]");
        var con = Console.ReadLine();
        if (con == null) con = "";
        bool empty = false;
        IWebElement facultySel = cd.FindElement(By.CssSelector("div[data-testid='select-faculty']"));
        IWebElement clickFac = facultySel.FindElement(By.CssSelector("svg[class='svg-inline--fa fa-caret-down fa-lg multiselect__caret-icon']"));
        if (con.ToLower() == "y")
        {
            try
            {
                //IWebElement facultySel = cd.FindElement(By.CssSelector("div[data-testid='select-faculty']"));
                //IWebElement clickFac = facultySel.FindElement(By.CssSelector("svg[class='svg-inline--fa fa-caret-down fa-lg multiselect__caret-icon']"));
                clickFac.Click();
                IList<IWebElement> facultyList = facultySel.FindElement(By.CssSelector("ul[class='multiselect__content']")).FindElements(By.CssSelector("li[class='multiselect__element']"));

                string[] facultyFL = new string[facultyList.Count];
                Console.WriteLine("Vyberte fakultu: (pro vybrání více použijte čárky mezi čísly)");
                for (int i = 0; i < facultyList.Count; i++)
                {
                    Console.WriteLine($"{i}. {facultyList[i].Text}");
                    facultyFL[i] = facultyList[i].Text.Replace("&nbsp", "").Replace(" ", "");
                }
                string selFacults = Console.ReadLine();
                if ((selFacults is null) || (selFacults.Length <= 0))
                    throw new Exception("Fakulta nevybrána");
                string[] sFakulty = selFacults.Split(",");
                foreach (string faculty in sFakulty)
                {
                    int numm = 1000000;
                    int.TryParse(faculty, out numm);
                    try
                    {
                        faculties.Add(facultyList[numm]);
                    }
                    catch (IndexOutOfRangeException) { }
                }

            }
            catch (Exception) { }
        }
        else
        {
            faculties.Add(cd.FindElement(By.TagName("html")));
            empty = true;
        }

        //Zapisování ID předmětů
        List<string> ids = new List<string>();
        Console.WriteLine("Načítám předměty...");
        for (int i = 0; i < faculties.Count; i++)
        {
            if (!empty)
            {
                clickFac.Click();
                faculties[i].Click();
            }

            IWebElement element = cd.FindElement(By.TagName("html"));
            IList<IWebElement> nums = cd.FindElement(By.ClassName("pagination")).FindElements(By.ClassName("page-item"));
            foreach (IWebElement num in nums)//přepnutí na num 100
            {
                if (num.Text.Equals("100"))
                    element = num;
            }
            element.Click();
            Thread.Sleep(500);
            List<IWebElement> pages = new List<IWebElement>();
            IList<IWebElement> alphabethL = cd.FindElement(By.CssSelector("div[data-testid='alphabet']")).FindElements(By.TagName("span"));
            foreach (IWebElement tempEl in alphabethL)
            {
                if (ids.Count >= 30)
                    break;
                pages.Clear();
                Debug.WriteLine(tempEl.Text);
                char tester = char.Parse(tempEl.Text.ToLower());
                if (tester != null)
                    if (alphabeth.Contains(tester))
                    {
                        tempEl.Click();
                        try //vylistování tlačítek na přepnutí stránky (pokud jsou)
                        {
                            IList<IWebElement> dummy = cd.FindElements(By.CssSelector("li[class='page-item align-self-center']"));
                            Thread.Sleep(2000);
                            foreach (IWebElement page in dummy)
                                try
                                {
                                    pages.Add(page.FindElement(By.TagName("button")));
                                }
                                catch (Exception) { }
                        }
                        catch (NoSuchElementException) { }

                        pages.Add(cd.FindElement(By.TagName("html")));
                        foreach (var page in pages)
                        {
                            IList<IWebElement> subjects = cd.FindElement(By.TagName("tbody")).FindElements(By.CssSelector("a[class='link-room text-nowrap']"));
                            foreach (IWebElement subject in subjects)
                                ids.Add(subject.Text);
                            Console.WriteLine("Zapsáno IDs: {0}", ids.Count);
                            if (page.TagName == "html")
                                continue;
                            page.Click();
                            Thread.Sleep(1500);
                        }
                    }
            }
            if ((numero == 0) && (numero >= ids.Count))
                numero = ids.Count;
        }

        //AutoReading
        if (numero > 0)
        {
            for (int i = 0; i < numero; i++)
            {
                cd.Url = @$"https://new.kos.cvut.cz/course-syllabus/{ids[i]}/{sem}";
                cd.Navigate();
                Thread.Sleep(500);
                try
                {
                    string nameE = cd.FindElement(By.CssSelector("div[data-testid='name']")).FindElement(By.CssSelector("span[class='attribute-value']")).Text;
                    IWebElement facultyName = cd.FindElement(By.CssSelector("div[data-testid='faculty']")).FindElement(By.CssSelector("span[class='attribute-value']"));
                    IWebElement credits = cd.FindElement(By.CssSelector("div[data-testid='credits']")).FindElement(By.CssSelector("span[class='attribute-value']"));
                    int kredity = 0;
                    int.TryParse(credits.Text, out kredity);
                    Console.WriteLine($"\n\n{i + 1}/{numero} - {nameE}");

                    bool pred = cd.FindElement(By.CssSelector("div[data-testid='extent']")).Text.Contains("P"); ;
                    IList<IWebElement> elements = cd.FindElement(By.CssSelector("div[data-testid='parallels']")).FindElements(By.CssSelector("tr[class='row-headline collapsed']"));
                    if (elements.Count > 0)
                    {
                        int pocet = 0;
                        int par = 0;
                        foreach (IWebElement webElement in elements)
                        {
                            string[] items = new string[5];
                            IWebElement tempElement0 = webElement.FindElement(By.CssSelector("td[data-testid='parallel-number']")).FindElement(By.CssSelector("div[class='cell-content']"));    //číslo paralelky
                            IWebElement tempElement1 = webElement.FindElement(By.CssSelector("td[data-testid='parallel-type']")).FindElement(By.CssSelector("div[class='cell-content']"));      //typ paralelky
                            IWebElement tempElement2 = webElement.FindElement(By.CssSelector("td[data-testid='occupied-capacity']")).FindElement(By.CssSelector("div[class='cell-content']"));  //capacita
                            IWebElement tempElement3 = webElement.FindElement(By.CssSelector("td[data-testid='timetables']")).FindElement(By.CssSelector("div[data-testid='parallel-time']"));  //čas
                            var tempElements = webElement.FindElement(By.CssSelector("td[data-testid='timetables']")).FindElements(By.CssSelector("a[data-testid='parallel-teacher']"));        //Učitelé

                            try
                            {
                                IWebElement tempElement4 = webElement.FindElement(By.CssSelector("td[data-testid='timetables']")).FindElement(By.CssSelector("div[data-testid='parallel-week']"));//týden SU/Li
                                items[1] += " " + tempElement4.Text;
                            }
                            catch (NoSuchElementException ex) { Debug.WriteLine(ex.Message, "Week"); }

                            int.TryParse(tempElement0.Text, out par);
                            if (pred && !tempElement1.Text.StartsWith("P"))
                                items[0] = "*";
                            items[0] += tempElement1.Text;
                            if (items[0].StartsWith("P"))
                                pred = true;
                            items[3] = tempElement2.Text;
                            items[1] = tempElement3.Text;
                            if (tempElements.Count > 0)
                            {
                                foreach (IWebElement we in tempElements)
                                    items[2] += we.Text + ",";
                                items[2] = items[2].Remove(items[2].Length - 1);
                            }
                            else { items[2] = "-"; }
                            items[4] = cd.FindElement(By.CssSelector("div[data-testid='code']")).FindElement(By.CssSelector("span[class='attribute-value']")).Text;
                            predmety.Add(new Subject(nameE, items[1], items[4], items[0], items[2], par, items[3], facultyName.Text, kredity));
                            pocet++;
                            string printP = $"Paralelek: {pocet}/{elements.Count}";
                            for (int j = 0; j < printP.Length; j++)
                                Console.Write("\b");
                            Console.Write(printP);

                        }
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message, "WebElement"); }
            }
        }
        cd.Url = "https://new.kos.cvut.cz/logout";
        cd.Navigate();
        Thread.Sleep(2000);
        cd.Close();
        Debug.WriteLine(predmety.Count);
        predmety.Sort((s1, s2) => s1.jmeno.CompareTo(s2.jmeno));
        predmety.Sort((s1, s2) => s1.jmeno.CompareTo(s2.jmeno));


        //Konec programu - uživatelské funkce
        bool end = false;
        string path = "";
        Console.WriteLine("\n\nKonec programu, zadej příkaz:" +
            "\nlist [po/út/st/čt/pá] - vypíše předměty v určitém dni" +
            "\nprint - vyexportuje do souboru" +
            "\nteacher [list/jméno učitele] - vypíše učitele/najde předměty, kde učí" +
            "\nend");

        while (!end)

        {
            var uInput = Console.ReadLine();
            if (uInput is null) continue;
            var command = uInput.Split(" ");
            switch (command[0])
            {
                case "list":
                    string den = "po";
                    if (command.Length == 1)
                    {
                        Console.WriteLine("Dny:\t PO\t ÚT\t ST\t ČT\t PÁ");
                        den = Console.ReadLine();
                    }
                    else
                    {
                        den = command[1];
                    }
                    foreach (var predmet in predmety)
                    {
                        if (den == null)
                            break;
                        if (den == "all")
                        {
                            Console.WriteLine(predmet.ToString());
                            continue;
                        }
                        if (predmet.cas.Contains(den.ToLower()))
                            Console.WriteLine(predmet.ToString());
                    }

                    break;
                case "print":
                    Console.WriteLine("Zapiš cestu k uložení:");
                    List<string> list = new List<string>();
                    predmety.ForEach(item => list.Add(item.ToPrint()));
                    path = Console.ReadLine();
                    if (path == null)
                        path = "";
                    File.WriteAllLines(path + "\\Rozvrh.csv", list, Encoding.UTF8);
                    break;
                case "teacher":
                    string uci = "";
                    HashSet<string> ucitele = new HashSet<string>();
                    if (command.Length == 1)
                    {
                        Console.WriteLine("list / jmeno učitele");
                        uci = Console.ReadLine();
                    }
                    if (uci.Length < 1)
                        uci = command[1];
                    if (command.Length == 3)
                        uci += command[2];

                    foreach (Subject predmet in predmety)
                    {
                        if (uci.Equals("list") || uci.Equals("teacher list"))
                        {
                            var temp = predmet.uci.Split(",");
                            foreach (string s in temp)
                                ucitele.Add(s);
                        }
                        else if (predmet.uci.ToLower().Contains(uci.ToLower()))
                        {
                            Console.WriteLine(predmet.ToString());
                        }
                    }
                    foreach (string u in ucitele)
                        Console.WriteLine(u);
                    break;
                case "count":
                    Console.WriteLine(predmety.Count);
                    break;
                case "end":
                    end = true;
                    break;
            }
        }
        Console.Clear();
        Console.WriteLine("Můžete zavřít okno");
    }

}

class Subject
{
    public string jmeno, cas, ID, typ, uci, kapacita, fakulta;
    public int parallel, kredity;
    public Subject(string name, string time, string id, string type, string teacher, int parallel, string kapacita, string fakulta, int kredity)
    {
        jmeno = name.Replace("&nbsp", "");
        cas = time.Replace("&nbsp", "");
        ID = id.Replace("&nbsp", "");
        typ = type.Replace("&nbsp", "");
        uci = teacher.Replace("&nbsp", "");
        this.parallel = parallel;
        this.kapacita = kapacita.Replace("&nbsp", "");
        this.fakulta = fakulta.Replace("&nbsp", "");
        this.kredity = kredity;
    }

    public string ToPrint()
    {
        return ID + ";" + jmeno + ";" + kredity + ";" + cas + ";" + typ + ";" + parallel.ToString() + ";" + uci + ";" + kapacita + ";" + fakulta;
    }

    override public string ToString()
    {
        return ID + "\n" + jmeno + "\n" + cas + "\n" + typ + "\n" + parallel + "\n" + uci + "\n" + kapacita + "\n" + fakulta + "\n";
    }
}