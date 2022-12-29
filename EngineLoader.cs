// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Launcher;
using Godot;
using System;
using System.IO;
using System.Reflection;

public partial class EngineLoader : Node3D
{
	public override void _Ready()
	{
		Assembly engineAssembly = null;

		try
		{
			ElegyLauncherLoadContext launcherLoadContext = new();
			engineAssembly = launcherLoadContext
				.LoadFromAssemblyPath( $"{Directory.GetCurrentDirectory()}/Elegy.Engine.dll" );
		}
		catch ( FileNotFoundException ex )
		{
			// Todo: we could maybe have a messagebox popping up here
			GD.PrintErr( "Cannot find Elegy.Engine.dll" );
			GD.PrintErr( "Error message:" );
			GD.PrintErr( ex.Message );
		}
		catch ( Exception ex )
		{
			GD.PrintErr( $"Unknown error: {ex.Message}" );
		}

		if ( engineAssembly == null )
		{
			Exit( "Shutting down, I cannot work without my engine DLL...", 1 );
			return;
		}

		// This is quite a nasty way of doing this, but there are some complications with
		// using types between Godot project assemblies and external ones, which make it
		// unnecessarily tricky to just use an interface here.
		// I preferred to keep it simple and just use delegates
		EngineInterface engine = new();
		
		Type? entry = engineAssembly.GetType( "Elegy.EntryPoint" );
		if ( entry == null )
		{
			GD.PrintErr( "Elegy.Engine.dll does not contain EntryPoint" );
			Exit( "Without an entry point to the engine, I cannot load it" );
			return;
		}

		if ( !CheckMethod( entry, ref engine.Init, "Init" ) ||
			 !CheckMethod( entry, ref engine.Update, "Update" ) ||
			 !CheckMethod( entry, ref engine.PhysicsUpdate, "PhysicsUpdate" ) ||
			 !CheckMethod( entry, ref engine.HandleInput, "HandleInput" ) )
		{
			Exit( "Failed to load one of the methods from Elegy.Engine.dll", 2 );
			return;
		}

		if ( !engine.Init( GetParent() as Node3D ) )
		{
			Exit( "Engine failed to initialise" );
			return;
		}

		// Kick off the engine host with the loaded interface
		EngineHost engineHost = new( engine );
		engineHost.Name = "EngineHost";
		engineHost.TopLevel = true;
		GetParent().AddChild( engineHost );

		// Delete self, no longer needed
		QueueFree();
	}

	private static bool CheckMethod<T>( Type type, ref T method, string name ) where T : Delegate
	{
		method = type.GetMethod( name ).CreateDelegate<T>();

		if ( method == null )
		{
			GD.PrintErr( $"Cannot find method EntryPoint.{name} in Elegy.Engine.dll" );
			return false;
		}

		return true;
	}

	private void Exit( string message, int errorCode = 0 )
	{
		GD.Print( message );
		QueueFree();
		GetTree().Quit( errorCode );
	}
}
