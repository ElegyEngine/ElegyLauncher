// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Launcher;
using Godot;

public partial class EngineHost : Node3D
{
	EngineInterface mInterface;

	public EngineHost( EngineInterface @interface )
	{
		mInterface = @interface;
	}

	public override void _Process( double delta )
	{
		mInterface.Update( (float)delta );
	}

	public override void _PhysicsProcess( double delta )
	{
		mInterface.PhysicsUpdate( (float)delta );
	}

	public override void _Input( InputEvent @event )
	{
		mInterface.HandleInput( @event );
	}
}

