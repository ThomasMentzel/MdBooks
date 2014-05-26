#Pattern of the Week #7: Command Pattern
Diese Woche steht unter dem Stern des Command Pattern. Das Command Pattern ist ebenso wie das Adapter Pattern eines der häufiger verwendeten Pattern.

##Klassische Implementierung
Das naheliegendste Beispiel für ein Command Pattern ist eine Schildkröte (Turtle), die über eine Methode (AddCommand) Kommandos erhält und diese anschließend ausführt (Go).

![Klasse Implementierung Diagramm](images\007a_1_classicImplementation.png)

	public class Turtle
	{
	    private readonly IList<ICommand> _moveCommands = new List<ICommand>();
	
	    public void AddCommand(ICommand command)
	    {
	        this._moveCommands.Add(command);
	    }
	
	    public void Go()
	    {
	        foreach (var command in _moveCommands)
	            command.Execute();
	    }
	}

Die Klasse `Turtle` kennt nur `ICommand`. Die konkrete Implementierung und besonders die Initialisierung bleibt der Klasse `Turtle` verborgen.

	var turtle = new Turtle();
	
	ICommand cmd1 = new TurnLeft(90);
	ICommand cmd2 = new Forward100();
	
	turtle.AddCommand(cmd1);
	turtle.AddCommand(cmd2);
	
	turtle.Go();

Die `Execute()` Methoden der Commands werden nacheinander ausgeführt und sind sogar mehrfach ausführbar. Damit führt die Klasse `Turtle` die Commands wie eine Art Makro aus. Das Command Pattern kann also auch zum Scripten verwendet werden.

##Undo Commands

Eine ebenso klassische Erweiterung ist ein Undo-Mechanismus. Dazu erhält die Schnittstelle ICommand eine weitere Undo() Methode. So wie der Script-Ablauf zum `Go` kann ein `Undo` die komplementären Funktionen umgekehrter Reihenfolge ausführen.

![Undo Implementierung Diagramm](images\007a_2_undoCommands.png)


	public class UndoTurtle
	{
	    private readonly IList<IUndoCommand> _moveCommands
	        = new List<IUndoCommand>();
	
	    public void AddCommand(IUndoCommand command)
	    {
	        this._moveCommands.Add(command);
	    }
	
	    public void Go()
	    {
	        foreach (var command in _moveCommands)
	            command.Execute();
	    }
	
	    public void Rewind()
	    {
	        foreach (var command in _moveCommands.Reverse())
	            command.Undo();
	    }
	}

##Command Pattern & WPF 4

Mit der ersten Version von WPF wurde als essentieller Bestandteil die erweiterte komplexe Datenbindung eingeführt. Damit können Daten flexibler als mit Windows Forms an die GUI gebunden werden. Das MVVM Pattern war damit der Standard für die Datenpräsentation.

Mit WPF 4 wurde die Datenbindung um die Command-Bindung erweitert. Damit können nicht nur Daten sondern auch Methoden gebunden werden. Ein Modell enthält damit auch Logik, die gebunden werden kann.

	public class MainUserControlModel
	{
	    // property for binding
	    public string ButtonText { get; set; }
	
	    // command for bindung
	    public ICommand MessageBox { get; set; }
	}

Es fällt wieder –oh Überraschung- die Schnittstelle ICommand auf. Die Schnittstelle ICommand aus dem WPF Namespace ist etwas aufwändiger um zusätzlich kontextbezogen Steuerelemente aktivieren und deaktivieren zu können.

	public class MessageBoxCommand : ICommand
	{
	    #region Implementation of ICommand
	
	    public void Execute(object parameter)
	    {
	        MessageBox.Show("Hello World");
	    }
	
	    public bool CanExecute(object parameter)
	    {
	        return true;
	    }
	
	    public event EventHandler CanExecuteChanged = (sender, e) => { };
	
	    #endregion
	}

Anschließend kann das Model an die View mittels XAML gebunden werden.

	<Button
	    Command="{Binding Path=MessageBox}"
	    Content="{Binding Path=ButtonText}" 
	    Margin="16,11,0,0" Name="button1" [...] />