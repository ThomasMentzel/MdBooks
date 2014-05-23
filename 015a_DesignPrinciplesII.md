#Pattern of the Week #15: Design Principles II
Es ist mal wieder an der Zeit, ein paar Design Principles vorzustellen. Bei Design Principles handelt es sich nicht Muster für ein Problem sondern um allgemeine Richtlinien. Diese Richtlinien beschreiben Grundgedanken und Regeln für ein gutes Design.

##YAGNI
YAGNI ist ein Akronym für `You Ain’t Gonna Need It` und bedeutet übersetzt so viel wie `Du benötigst das nicht`. Damit ist gemeint, dass nur diejenigen Features implementiert werden sollen, die gewünscht sind und damit auch hinreichend spezifiziert wurden. Oft werden Features implementiert, nur weil man sich sagt "Das kann ich bestimmt irgendwann gebrauchen".

**YAGNI Code hat viele Nachteile**

1. YAGNI Code für nie benötigte Features. Es wird Zeit in etwas investiert, das keinen Nutzen hat.
1. YAGNI Code muss getestet werden. Egal wie klein und trivial der Code ist, muss dafür mindestens ein Test geschrieben werden.
1. Wird ein Features auf "gut Glück" implementiert kann es sein, dass Code und damit die Tests neu implementiert werden müssen, da die Spezifikation anders ist, als man zunächst vermutet hat.


Hier ein kleines Beispiel. Der Kunde hat die folgende Anforderung (der Übersichtlichkeit halber sehr zusammengefasst).

* Implementieren eines LIFO Speichers mit Push und Pop
* LIFO Speicher ist generisch
* Export der Einträge als XML


Ein Ausweg hierfür ist, dass der Ansatz für die Entwicklung geändert wird. Hier helfen Methoden wie Test Driven Development (TDD) und Specification Driven Development. Beide Ansätze verfolgen den Weg, zunächst die Anforderungen in Form von Tests zu definieren. Diese Tests definieren die Erwartungen an das Framework und diese –aber auch nur diese- werden auch implementiert.

	[Test()]
	public void PopPushTest()
	{
	    MyStack<int> sut = new MyStack<int>();
	
	    var expected = 23;
	    sut.Push(expected);
	    var actual = sut.Pop();
	
	    Assert.AreEqual(expected, actual, "stack returned wrong value");
	}

	[Test()]
	public void ToXmlTest()
	{
	    MyStack<int> sut = new MyStack<int>();
	            
	    string xml = sut.ToXml();
	
	    Assert.AreEqual(string.Empty, xml);
	}
 

Ein weiterer Aspekt von YAGNI ist es, nicht mit der sprichwörtlichen Kanone auf Spatzen zu schießen. Aus der Anforderung “den Stack in XML zu exportieren”, können beliebig viele Featuers abgeleitet werden.

* Methode "Export" zum Exportieren
* Methode "Import" um den Export importieren zu können
* Serializer Factory um unterschiedliche Serialisierungsverfahrung unterstützen zu können
* Plugin Mechanismus für zusätzliche Serialisierungsverfahren
* Interceptor Pattern um in die Serialisierung eingreifen zu können

Aus der Anforderung, einen Stack in ein XML zu exportieren, kann man sehr sehr viele Features ableiten. Aber man muss sich immer die Frage stellen, was war der Auftrag. In der Spezifikation (who, what, why) steht vielleicht "Der Entwickler möchte den Stack nach XML exportieren um die Daten in Excel darstellen zu können". Welches der Features aus der obigen Liste werden benötigt?

##Tell, don’t Ask

Wer Kinder hat wird das Problem kennen: Bei langen Autofahrten wird ständig gefragt "Sind wir bald da?". Die Antwort ist dann so etwas wie "noch 150 Kilometer". Steht man im Stau sind die Fragen noch lästiger, da die Entfernung sich nicht ändert. Wie wäre es denn alternativ mit einen Status alle 20 Kilometer? Das Kind muss (müsste) nicht mehr fragen und bekommt regelmäßig einen Status, aber auch nur dann, wenn es sinnvoll ist.

Ebenso verhält es sich in Software. Es hat keinen Nutzen, ein Objekt ständig nach seinem Status zu fragen. Man bekommt auf diesem Weg irgendwann mit, das sich etwas geändert hat, aber nicht wann. Die Fragenden bekommen aber nicht mit, wenn sich wirklich etwas ändert.

Der Ausweg für dieses Problem ist das Observer Pattern oder in der .NET Sprache `Events`. An das Event können sich alle fragenden Objekte (Listener) registrieren und wieder deregistrieren. Das Quellobjekt benachrichtig alle fragenden Objekte –aufrufen der übergebenen Methode. Das beste Beispiel für eine solche Implementierung ist die Schnittstelle aud dem .NET Framework `INotifyPropertyChanged`.

	public class AClassWithSomeState : INotifyPropertyChanged
	{
	    // INotifyPropertyChanged implementation
	    public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
	
	    private string _name;
	
	    public string Name
	    {
	        get { return _name; }
	        set
	        {
	            _name = value;
	            PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));
	        }
	    }
	}

Damit kann man sich von Statusänderungen benachrichtigen lassen, wenn sie wirklich auftreten.

	var acwss = new AClassWithSomeState();

	for (int i = 0; i < 10; i++)
	{
	    int i1 = i; // remember: "i1" is an closure, "i" NOT
	    acwss.PropertyChanged +=
	        (sender, e) => Console.WriteLine("acwss has changed. event listerner #" + i1 +
	                       ((AClassWithSomeState)sender).Name);
	}
	
	acwss.Name = "New Name";
 
Ein weiterer Schritt, um die Aufrufe an die Klasse zu reduzieren ist es, dem Listener bei dem Methodenaufruf den Status mitzugeben. Damit wird zusätzlich die Kopplung zwischen Klasse und Listener entkoppelt. Beide hängen nur noch von den EventArgs als Schnittstelle ab.

	// new event definition
	public event EventHandler<AClassWithStateEvtStateEventArgs>
	    PropertyChanged = (sender, e) => { };
	
	// event args definition
	public class AClassWithStateEvtStateEventArgs : EventArgs
	{
	    private readonly string _property;
	    private readonly object _name;
	
	    public AClassWithStateEvtStateEventArgs(string property, object name)
	    {
	        _property = property;
	        _name = name;
	    }
	
	    public object Name
	    {
	        get { return _name; }
	    }
	
	    public string Property
	    {
	        get { return _property; }
	    }
	}
 
##Mockups

Es wird gepredigt, die Kopplung der Komponenten möglichst gering zu halten und Schnittstellen zu definieren. Das erhöht die Wartbarkeit und reduziert die Fehleranfälligkeit. Ein weiterer wichtiger Vorteil ist allerdings die Testbarkeit.

Das größte Problem beim Testen ist die Kopplung zwischen den Komponenten. Selbst wenn eine Komponente nur von einer Schnittstelle abhängt, hängt sie dennoch davon ab. Befriedigt man diese Abhängigkeit indem man eine andere Komponente des "produktiven" Code übergibt, kann man in Komponente A auf einen Fehler treten, die durch Komponente B entsteht. Der Test ist damit nicht aussagekräftig und falsch.

Mockups und Schnittstellen lösen dieses Problem und bieten einen weiteren Benefit: Aufruftests. Beginnt man nun eine Klasse zu mocken, stellt sich dennoch die Frage, ob eine Methode des Mocks wirklich aufgerufen wurde oder nicht. Hat man eine Schnittstelle `IEmail` und eine Klasse `SmtpSender` wäre es gut, wenn die Property `Body` auch mindestens einmal aufgerufen wurde.

Mocking Frameworks gibt es wie Sand am Meer. Da ich für meine Tests normalerweise NUnit einsetze, benutze ich für mein Mocking gerne die Klasse DynamicMock. Letztlich ist es egal, welches Mocking Framework man nutzt, da die Essenz dieser Frameworks gleich ist: Objekte zu erstellen, die eine Schnittstelle implementieren und als Atrappen für die "produktiven" Objekte stehen.

	// the interface definition
	public interface IWantsToBeMocked
	{
	    string Name { get; set; }
	    string Address { get; set; }
	}
	
	// the interface "productive" implementation
	public class WantsToBeMocked : IWantsToBeMocked
	{
	    public string Name { get; set; }
	    public string Address { get; set; }
	}
	
	// the class which uses the interface
	public class SystemUnderTest
	{
	    public void WriteNameWithoutAddress(IWantsToBeMocked instance)
	    {
	        Console.WriteLine("Name: " + instance.Name);
	    }
	}

Der Test erstell ein Mock-Objekt (mittels DynamicMock) und reicht dieses an das System unter Test (sut) weiter. Damit entstehen Implementierungen für eine Schnittstelle, die nur das nowendige implementiert und die Rückgabewerte beim Mocking fest definiert werden. Ein Bonuspunkt ist, dass die Erwartung an die Methodenaufrufe ebenfalls definiert werden.

	// create mock object
	var aMockFactory = new DynamicMock(typeof(IWantsToBeMocked));
	aMockFactory.ExpectAndReturn("get_Name", "Thomas");
	aMockFactory.SetReturnValue("get_Address", "Bramsstr. 8");
	
	IWantsToBeMocked mock = (IWantsToBeMocked)aMockFactory.MockInstance;
	
	// create sud and test
	var sut = new SystemUnderTest();
	sut.WriteNameWithoutAddress(mock);
	
	// check if mock was called correctly
	aMockFactory.Verify();
 
Die Methodennamen `get_*` liegen daran, dass hier Getter von Properties aufgerufen werden, die intern diese Namenskonvention haben. Zwar hängt das SUT weiterhin von der Schnittstelle ab, es ist aber durch das Mocking auszuschließen, dass der Test fehlschlägt, weil die abhängigen Klasse fehlerhaft ist. Man sieht, dass Mocking notwendig ist, um korrekte Tests zu definieren. Tests, in denen nur eine Methode des SUT getestet wird und nichts anderes.