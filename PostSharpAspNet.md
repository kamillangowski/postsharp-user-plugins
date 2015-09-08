The PostSharp.AspNet.AssemblyPostProcessor class hooks into the ASP.NET compilation process by post-processing assemblies using PostSharp (implementation of IAssemblyPostProcessor).

In order to use PostSharp in a web project, specify this class as an assembly post-processor in web.config:


```
<configuration>
     <system.web>
       <compilation debug="true" assemblyPostProcessorType="PostSharp.AspNet.AssemblyPostProcessor, PostSharp.AspNet"/>
     </system.web>
</configuration>
```

Additionally, you have to add the <postsharp ... /> section in the configuration file:

```
<?xml version="1.0"?>
<configuration>
	<!-- Add a configuration handler for PostSharp. -->
	<configSections>
		<section name="postsharp" type="PostSharp.AspNet.Configuration.PostSharpConfiguration, PostSharp.AspNet"/>
	</configSections>
	<!-- PostSharp configuration -->
	<postsharp directory="P:\open\branches\1.0\Core\PostSharp.MSBuild\bin\Debug" trace="true">
		<parameters>
			<!--<add name="parameter-name" value="parameter-value"/>-->
		</parameters>
		<searchPath>
			<!-- Always add the binary folder to the search path. -->
			<add name="bin" value="~\bin"/>
			<!-- Then add the location of plug-ins that are not installed in standard locations. -->
			<add name="laos-weaver" value="P:\open\branches\1.0\Laos\PostSharp.Laos.Weaver\bin\Debug"/>
		</searchPath>
	</postsharp>
	<appSettings/>
	<connectionStrings/>
	<system.web>
		<!-- Note the 'assemblyPostProcessorType' attribute. -->
		<compilation debug="true" assemblyPostProcessorType="PostSharp.AspNet.AssemblyPostProcessor, PostSharp.AspNet">
		<authentication mode="None"/>
		<trace enabled="true" pageOutput="true"/>
	</system.web>
</configuration>
```

In all configuration parameters and in search path elements, the tilde character (~) is replaced by the physical path of the application.