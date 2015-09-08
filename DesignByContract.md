DesignByContract lets you specify constraint and checks to your code.

# Introduction #
You can find an introduction to design by contract [here](http://en.wikipedia.org/wiki/Design_by_contract).

# Details #

Current features:
  * Check for null parameters and return values
  * Check for empty string parameters and return values
  * Define singletons classes

# Usage #
Download source code from [svn](http://postsharp-user-plugins.googlecode.com/svn/trunk/Torch/) repository. Built it and copy Torch.DesignByContract.dll, Torch.DesignByContract.Weaver.dll and Torch.DesignByContract.psplugin to the PlugIns directory under the PostSharp installation location. Reference Torch.DesignByContract.dll from your project.

## NonNull/NonEmpty ##
```
  [return: NonNull] public SomeObject SomeMethod([NonNull] AnotherObject param1)
  {
    ...
  }
```

## Singleton ##
### Declaration ###
```
  [Singleton]
  public class MySingletonCandidate
  {
     // default constructor
     public MySingletonCandidate()
     {
        ...
     }
     ...
  }
```
### Using the class ###
> Just like any other class, if you are compiling against the plain class (not yet enhaced)
```
  MySingletonCandidate obj1 = new MySingletonCandidate();
```
> Or
```
  MySingletonCandidate obj1 = MySingletonCandidate.Instance;
```
> If you are compiling against the enhaced class.

Comments to altobarba [at](at.md) gmail [dot](dot.md) com, or posting in the postsharp's [forum](http://www.postsharp.org/forum/designbycontract-f17.html).

Ignacio Vivona.