// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Reflection;
using System.Runtime.Loader;

namespace Elegy.Launcher
{
	internal class ElegyLauncherLoadContext : AssemblyLoadContext
	{
		protected override Assembly Load( AssemblyName assemblyName )
		{
			switch ( assemblyName.Name )
			{
				case "GodotSharp": return typeof( Godot.GD ).Assembly;
			}

			return null;
		}
	}
}
