#Command Pattern trifft CD-Sammlung
##Hintergründe

Bereits in einem vorherigen Blog-Artikel hatte ich erwähnt, dass mit Metadaten von MP3 Dateien wichtig sind. Nach einem Daten-Crash-Super-GAU hatte ich mich entschieden, alle meine CDs noch einmal mit iTunes zu digitalisieren und zu 100% zu "taggen". Da mit iTunes nur ein Teil der Metadaten gefüllt werden, müssen die anderen Daten irgendwie anders her. Die Seite Musik-Sammler hat eine sehr ausführliche Liste an CDs und deren zugehörige EANs (Der CD-Code auf der Rückseite der CD). Mit den EANs lassen sich die entsprechenden CDs sehr schnell finden.

##Divide et impera (Teile und Herrsche)

Kurz gesagt gibt es viele Schritte, die neben dem reinen Digitalisieren mit iTunes noch erledigt werden müssen. Dazu zählen:

* Erstellen einer Liste aller EANs meiner CDs
* Lesen von erweiterten Metadaten aus dem Internet (Musik-Sammler.de)
* Lesen des Covers aus dem Internet
* Linken des MP3 Verzeichnisses mit dem Album/EAN
* Verifizieren von beispielsweise 
	* Sind alle Songs digitalisiert
	* Hat jedes Album ein MP3-Verzeichnis

Alle diese Punkte lassen sich bei fast 400 CDs nicht mehr händisch pflegen. * Also muss ein Skript (PowerShell) oder ein Programm her. Ich habe mich für eine Konsolenapplikation entschieden. Um einzelne Aufgaben getrennt ausführen zu können, habe ich ein Konzept wie bsp. `stsadm` gewählt, bei dem der erste Parameter die `Aktion` darstellt. Aufrufbeispiel:

	Analyser.exe <Aktion> <Parameter für die Aktion>
	Analyser.exe EanCdInfoReader c:\eans\ c:\cdinfo
	Analyser.exe DownloadCdCover c:\eans

##Command Pattern

Das -wir erinnern uns- hilft hierbei. Alle unterschiedlichen/möglichen Aktionen werden in separate Klassen ausgelagert und über die Schnittstelle ICommand erreichbar gemacht.

	public interface ICommand
	{
	    // args are the command line parameter (all of them)
	    void Execute(string[] args);
	}

Die Command-Line Parameter werden 1:1 and die Commands durchgereicht (natürlich könnte auch der erste Parameter gelöscht werden). Es gibt also in der Assembly für alle Aktionen eine separate Klasse mit der Implementierung der Schnittstelle `ICommand`.

##Find and Execute

Das unspannendste aber auch effizienteste ist das Instanziieren und Ausführen der Aktionen. Die `Main`-Methode ermittelt alle Implementierungen der Command-Schnittstelle, instanziiert die Implementierung mit dem korrespondierendem Namen  und ruft die `Execute` Methode auf.

	static void Main(string[] args)
	{
	    // show actions of no parameter is given
	    if (args.Length == 0)
	    {
	        Console.WriteLine("you need at least one operation. available actions:");
	        Assembly.GetEntryAssembly().GetTypes()
	            .Where(t => t.GetInterface(typeof(ICommand).Name) != null && t.IsClass)
	            .ToList()
	            .ForEach(t => Console.WriteLine("    " + t.Name));
	        return;
	    }
	
	    // find class for action and create instance
	    var cmdType = Assembly.GetEntryAssembly().GetTypes().FirstOrDefault(f => f.Name.EndsWith(args[0]));
	    if (cmdType == null)
	    {
	        Console.WriteLine("cannot find type for given action. check for typos and upper/lower case");
	        return;
	    }
	
	    var cmd = (ICommand)Activator.CreateInstance(cmdType);
	    cmd.Execute(args); // pass cmdline parameter to execute methode
	}

Das bedeutet also, dass der Aufruf von `Analyser.exe DownloadCdCover <...>` eine Klasse `DownloadCdCover` mit der Implementierung der Schnittstelle `ICommand` erwartet. Ist die Klasse vorhanden, dann wird die Aktion ausgeführt. Neue Aktionen müssen sich nicht registrieren, sondern werden automatisch erkannt. Erweitern könnte man den Code noch soweit, dass er die Klassen aus allen Assemblys in dem Verzeichnis verwendet.