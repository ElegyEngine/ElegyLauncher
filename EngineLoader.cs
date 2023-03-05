// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Launcher;
using Godot;
using System;
using System.IO;
using System.Reflection;

public partial class EngineLoader : Node3D
{
	public override void _Process( double delta )
	{
		if ( mInitialised )
		{
			return;
		}

		GD.Print( "[EngineLoader] Init" );
		GD.Print( "[EngineLoader] Loading 'Elegy.Engine.dll'..." );

		try
		{
			mEngineAssembly = mLoadContext
				.LoadFromAssemblyPath( $"{Directory.GetCurrentDirectory()}/Elegy.Engine.dll" );
		}
		catch ( FileNotFoundException ex )
		{
			// Todo: we could maybe have a messagebox popping up here
			GD.PrintErr( "[EngineLoader] Cannot find Elegy.Engine.dll" );
			GD.PrintErr( "[OS] Message:" );
			GD.PrintErr( ex.Message );
		}
		catch ( Exception ex )
		{
			GD.PrintErr( $"[OS] Unknown error: {ex.Message}" );
		}

		if ( mEngineAssembly == null )
		{
			Exit( "Shutting down, I cannot work without my engine DLL...", 1 );
			return;
		}

		// This is quite a nasty way of doing this, but there are some complications with
		// using types between Godot project assemblies and external ones, which make it
		// unnecessarily tricky to just use an interface here.
		// I preferred to keep it simple and just use delegates
		EngineInterface engine = new();
		
		Type? entry = mEngineAssembly.GetType( "Elegy.EntryPoint" );
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

		try
		{
			engine.Init( GetParent() as Node3D );
		}
		catch ( Exception ex )
		{
			GD.PrintErr( "[EngineLoader] Engine failed to initialise" );
			GD.Print( "[OS] Message:" );
			GD.Print( $"{ex.Message}" );
			Exit( "Engine failed to initialise", 3 );
			return;
		}
		finally
		{
			// Kick off the engine host with the loaded interface
			EngineHost engineHost = new( engine );
			engineHost.Name = "EngineHost";
			engineHost.TopLevel = true;
			GetParent().AddChild( engineHost );
		}

		// Delete self, no longer needed
		QueueFree();
		mInitialised = true;
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

	public override void _ExitTree()
	{
		mEngineAssembly = null;
		mLoadContext = null;
	}

	ElegyLauncherLoadContext mLoadContext = new();
	Assembly mEngineAssembly = null;
	bool mInitialised = false;
}
