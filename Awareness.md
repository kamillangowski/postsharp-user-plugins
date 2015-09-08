# Introduction #

'Awareness' is a new extension point of PostSharp Laos 1.5. It enables
a custom plug-in to add transformations to enhanced classes after
PostSharp Laos has done its job.

Thanks to this, PostSharp Laos can be make 'aware' of other frameworks
or class libraries that require custom enhancements.

Awarenesses are implemented as plug-ins of PostSharp Core.

For instance, both BinaryFormatter and DataContractSerializer skip the constructors
when deserializing objects. Therefore, aspects are not initialized properly. For
an object to be compatible with the BinaryFormatter or DataContractSerializer,
aspects have to be initialized from a method annotated with the [OnDeserializing](OnDeserializing.md)
custom attribute.

This is precisely the purpose of the Serialization awareness (PostSharp.Awareness.Serialization), the only awareness available so far.

# Compilation and Unit Tests #

1. Download the source code using an SVN client and open the solution in Visual Studio 2008.

2. If PostSharp is not installed globally, you may need to enter the location of PostSharp files in both projects using the Project Properties dialog box, Reference Path tab.

3. Edit the Project Properties of the project Serialization.Test  and, in the Reference Path tab, specify the output folder of the project Serialization (otherwise PostSharp will not find the plug-in).

4. Build the solution and run unit tests using NUnit.

# Installation #

Output files (**.dll,**.psplugin) should be present in PostSharp search path. One of the ways to install the plug-in is to copy these files to the plug-in directory of PostSharp. Alternatively, it is possible to add the location of these files to the list of reference path of all projects using this plug-in. More about search path on http://doc.postsharp.org/1.5/UserGuide/Platform/Advanced/SearchPath.html.


# Serialization awareness #

## Purpose ##

This awareness causes aspect initialization (InitializeAspects method, see PostSharp documentation) to be invoked during deserialization:

  * If the class already contains a method annotated with the attribute [OnDeserializing](OnDeserializing.md),  this plug-in injects instruction to initialize aspects before any other instruction of this method.

  * Of the class does not contain a an [OnDeserializing](OnDeserializing.md) method, this plug-in creates a new method that initializes aspects.

## Enabling the serialization awareness ##

There are two ways to enable the serialization awareness.

1. Annotate every aspect class that will need this awareness with the following custom attribute:

```
[EnableLaosAwareness( "PostSharp.Awareness.Serialization", "PostSharp.Awareness.Serialization" )]
```

As a result, the awareness will be enabled in all assemblies using this aspect.

2. Use the same custom attribute at assembly level (typically in AssemblyInfo.cs), in each assembly requiring this awareness:

```
[assembly: PostSharp.Laos.EnableLaosAwareness( "PostSharp.Awareness.Serialization", "PostSharp.Awareness.Serialization" )]
```