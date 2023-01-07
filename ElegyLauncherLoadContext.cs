// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Elegy.Launcher
{
	/// <summary>
	/// Handles DLL loading for Elegy.Engine.
	/// </summary>
	internal class ElegyLauncherLoadContext : AssemblyLoadContext
	{
		protected override Assembly Load( AssemblyName assemblyName )
		{
			switch ( assemblyName.Name )
			{
				case "GodotSharp": return typeof( Godot.GD ).Assembly;
			}

			// Check for already loaded assemblies
			foreach ( var assembly in AssemblyLoadContext.Default.Assemblies )
			{
				// Dynamic assemblies cannot be loaded so easily
				if ( assembly.IsDynamic )
				{
					continue;
				}

				AssemblyName loadedAssemblyName = new( assembly.FullName );
				if ( loadedAssemblyName.Name == assemblyName.Name )
				{
					return assembly;
				}
			}

			// Fallback to root directory
			string workingDirectory = Directory.GetCurrentDirectory();
			string assemblyPath = $"{workingDirectory}/{assemblyName.Name}.dll";
			if ( File.Exists( assemblyPath ) )
			{
				return Assembly.LoadFrom( assemblyPath );
			}

			// Use data_Elegy.Launcher_x86_x64 folder
			return null;
		}
	}
}
