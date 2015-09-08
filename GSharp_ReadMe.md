# GSharp Readme: PostSharp aspects for Gibraltar #

If you are new to GSharp, you might start by watching a quick video tour of how
PostSharp and Gibraltar work together to provide painless logging and log analysis.

> http://bit.ly/gsharp

If you already have PostSharp and Gibraltar installed, all you need to do
is add a reference to Gibraltar.GSharp.dll to your project.  This will define
the following attributes:

<table cellspacing='12px'><tr><td>GTrace</td><td>logs entry and exit from tagged methods including appropriate message indentation.</td></tr>
<tr><td>GTraceField</td><td>logs every change in value of a tagged field.</td></tr>
<tr>><td>GException</td><td>logs exceptions at the point they are raised. This is a handy safe guard to ensure that both handled and unhandled exceptions are logged by Gibraltar.</td></tr>
<tr><td>GTimer</td><td>lets you graph method execution time in Gibraltar providing invaluable information to identify bottlenecks and optimize performance.</td></tr>
</table>

## Prerequisites ##
  * [PostSharp 1.5](http://www.postsharp.org/download)
  * [Gibraltar 2.0](http://www.gibraltarsoftware.com/Try)
  * Microsoft .NET Framework 2.0 SP1 (or greater)

## Folder Structure ##
```
    Dependencies
	- PostSharp.Laos.dll	- PostSharp 1.5
	- PostSharp.Public.dll	- PostSharp 1.5
	- Gibraltar.Agent.dll	- Gibraltar 2.0 Agent for collecting log data
	- Gibraltar.GSharp.dll	- GSharp aspects the integrate PostSharp with Gibraltar

    Source
	- GSharp.sln		- Source to rebuild Gibraltar.GSharp.dll

    Demo
	- GSharpDemo.sln	- Source for a demo program
```

## Notes ##

  1. PostSharp.Laos.dll and PostSharp.Public.dll are part of the PostSharp 1.5.6.626 distribution.
    * These files are provided for convenience but you should have PostSharp 1.5 installed on your computer.
  1. Gibraltar.Agent.dll is part of the Gibraltar 2.0.478.0 distribution.
    * You will need to install Gibraltar to view the log files created by Gibraltar.Agent.dll

## Getting Started ##

  * After installing PostSharp and Gibraltar, try building and running !GSharpDemo.sln. New log sessions should appear automatically in the Gibraltar Analyst.

  * To integrate GSharp with your program, just add references to the four assemblies from the Dependencies folder.

  * Then, apply the GSharp attributes as you wish to methods, classes or whole assemblies in your project.

  * Review logs in Gibraltar Analyst and tune which methods you log as deisred using standard PostSharp attribute multicasting techniques.

## Technical Support ##

> [mailto:support@gibraltarsoftware.com](mailto:support@gibraltarsoftware.com)

> http://www.gibraltarsoftware.com/About/Contact-Us.aspx