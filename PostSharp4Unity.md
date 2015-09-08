PostSharp4Unity modifies assemblies after compilation to make objects self-configurable: you don't need to call a factory method; you can use the default constructor. All you have to do is to decorate your class with the custom attribute "Configurable". It smells
like Spring, isn't it ;-).

# Installing PostSharp4Unity #
Install PostSharp, then build the project PostSharp4Unity and add it to the references of your project. If you don't want to install PostSharp globally, you will need to modify the 'csproj' text file. See the documentation in the 'binary - no installer' documentation for details.

# Using PostSharp4Unity #

I have updated the StopLight sample. The first change is on the StoplightForm class:

```
[Configurable]
public partial class StoplightForm : Form, IStoplightView
  {
```

Then, in Program.Main, you can use the default constructor:

```
Application.Run(new StoplightForm());
```

Unfortunately, that's not all. Since Unity has no notion of context registry (i.e. no notion of "current container"), you have to build a basic one:

```
public sealed class UnityContainerProvider : IUnityContainerProvider
 {
   private readonly IUnityContainer container;

   public UnityContainerProvider()
   {
     this.container = new UnityContainer()
       .Register<ILogger, TraceLogger>()
       .Register<IStoplightTimer, RealTimeTimer>();
   }

   public IUnityContainer CurrentContainer
   {
       get { return this.container; }
   }
 }

```

Then tell PostSharp4Unity to build your container:

```
[assembly: DefaultUnityContainerProvider(typeof(UnityContainerProvider))]
```

As you can see, there is a little of set up to do, but it's only once per assembly. (And would be useless if there were some Unity-wide notion of context registry or default container.)

Pay attention that your 'configurable' objects are now configured **before** the constructor is executed, and not **after**. So, in the class StoplightForm, we have to move the view initialization at the end of the constructor:

```
[Configurable]
public partial class StoplightForm : Form, IStoplightView
{
  private StoplightPresenter presenter;

  public StoplightForm()
  {
    InitializeComponent();
    presenter.SetView(this);
  }

  [Dependency]
  public StoplightPresenter Presenter
  {
    get { return presenter; }
    set { presenter = value; }
  }

  ...
```