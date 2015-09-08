## Introduction ##

GSharp is a set of aspects that integrate with Gibraltar, a simple, yet powerful application logging, monitoring and profiling tool.

**[Check out this short video tour of using PostSharp and Gibraltar together](http://bit.ly/gharp)**

<a href='http://www.youtube.com/watch?feature=player_embedded&v=UEEO9x1XczU' target='_blank'><img src='http://img.youtube.com/vi/UEEO9x1XczU/0.jpg' width='425' height=344 /></a>

With these aspects you can add powerful logging to your applications without the effort, complexity or clutter of writing procedural logging code.  Plus, Gibraltar makes it incredibly easy to manage and analyze your logs.

<table>
<tr><td><b>Attribute</b></td><td><b>Description</b></td></tr>
<tr><td><code>[GTrace]</code></td><td>logs entry and exit from tagged methods including appropriate message indentation.</td></tr>
<tr><td><code>[GTraceField]</code></td><td>logs every change in value of a tagged field.</td></tr>
<tr><td><code>[GException]</code></td><td>logs exceptions at the point they are raised. This is a handy safe guard to ensure that both handled and unhandled exceptions are logged by Gibraltar.</td></tr>
<tr><td><code>[GTimer]</code></td><td>lets you graph method execution time in Gibraltar providing invaluable information to identify bottlenecks and optimize performance.</td></tr>
</table>

## Requirements ##
[Gibraltar](http://www.gibraltarsoftware.com/Register_Now.aspx) and [PostSharp](http://www.postsharp.org/download) on a computer running .NET Framework 2.0 or newer.

## Installing GSharp ##
[Download GSharp.zip](http://postsharp-user-plugins.googlecode.com/files/GSharp.zip) and follow the simple instructions in the [GSharp\_ReadMe](GSharp_ReadMe.md) file.

GSharp.zip includes source code for the GSharp aspects as well as source code for a demo program that illustruates GSharp usage.  When you build and run the demo program, Gibraltar logs will be created.  You can view these logs in the Gibraltar Analyst.

## Remarks ##
_[Contact us](http://www.gibraltarsoftware.com/About/Contact.aspx) with any questions or suggestions. We're here to help!_