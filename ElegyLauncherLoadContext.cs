﻿// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
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
		public ElegyLauncherLoadContext()
		{
			var directories = Directory.GetDirectories( Directory.GetCurrentDirectory(), "data_*", SearchOption.TopDirectoryOnly );
			mDataDirectory = directories[0];
			Godot.GD.Print( $"[EngineLoader] Assembly directory is: '{mDataDirectory}'" );
		}

		protected override Assembly Load( AssemblyName assemblyName )
		{
			// Built-in assemblies first
			switch ( assemblyName.Name )
			{
				case "GodotSharp": return typeof( Godot.GD ).Assembly;
			}

			// Root directory second
			string workingDirectory = Directory.GetCurrentDirectory();
			string assemblyPath = $"{workingDirectory}/{assemblyName.Name}.dll";
			if ( File.Exists( assemblyPath ) )
			{
				return Assembly.LoadFrom( assemblyPath );
			}

			// Data directory third
			assemblyPath = $"{mDataDirectory}/{assemblyName.Name}.dll";
			if ( File.Exists( assemblyPath ) )
			{
				return Assembly.LoadFrom( assemblyPath );
			}

			// Use data_Elegy.Launcher_x86_x64 folder
			return null;
		}

		private string mDataDirectory;
	}
}
