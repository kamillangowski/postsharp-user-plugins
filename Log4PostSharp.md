_For questions, please visit the [support forum](http://www.postsharp.org/forum/postsharp4entlib.html)._

# Introduction #

Log4PostSharp contains an aspect which uses PostSharp to inject MSIL code that writes messages to log4net log.
This page describes shortly how to get started using it.

# Requirements #

  * [PostSharp 1.0](http://www.postsharp.org/download/)
  * [Log4Net](http://logging.apache.org/log4net/)
  * .NET Framework 2.0

# Installing Log4PostSharp #

  * Download Log4PostSharp [source code](http://postsharp-user-plugins.googlecode.com/svn/trunk/Log4PostSharp/) using an SVN client.
  * Build the solution using Visual Studio or MSBuild.
  * Optionally: copy files `Log4PostSharp.psplugin` and `Log4PostSharp.Weaver.dll` into `C:\Program Files\PostSharp 1.0\PlugIns` (for global installation) or `C:\Documents and Settings\userName\Application Data\PostSharp 1.0` (user-only installation).

Log4PostSharp does not require any specific log4net configuration - configure it your usual way.

# Using Log4PostSharp #

## Adding Log4PostSharp to your project ##

Add to your project a reference to assemblies `PostSharp.Public.dll` and `Log4PostSharp.dll`.

If you did not install the plug-in as described here above, add the directory containing `Log4PostSharp.psplugin` in the in _Reference Paths_, in Visual Studio's Project Properties.

## Logging methods ##

Code injected by Log4PostSharp can log three kinds of messages:
  * when a method is entered,
  * when a method is exited without throwing exception,
  * when a method throws an exception.

### Logging a single method ###

The only thing that developer has to do is to decorate a method with `LogAttribute` and provide values for few attribute properties, like in the following example:

```
[Log(LogLevel.Info, "Counting characters.")]
int CountCharacters(string arg) {
   return arg.Length;
}
```

This would cause that when the method is entered, "Counting characters." message is logged with severity level of `Info`. Also, if exception occurs in the method, it would be logged with level of `Error`.

### Logging multiple methods (`Multicast`) ###

In most cases it is desirable to add the following line to project's `AssemblyInfo.cs` file:

```
[assembly: Log(AttributeTargetTypes = "*", EntryLevel = LogLevel.Debug, ExitLevel = LogLevel.Debug, ExceptionLevel = LogLevel.Error)]
```

This causes that all three kinds of events are logged and method signatures are appended to log messages.

### Remarks ###

Use `Multicast` feature of the PostSharp to apply the attribute to all methods in order to log messages with `Debug` level. Besides of this you may additionally decorate chosen methods with `Info` level to log more important messages.

_For questions, please visit the [support forum](http://www.postsharp.org/forum/postsharp4entlib.html)._