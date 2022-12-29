// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Godot;

namespace Elegy.Launcher
{
	public struct EngineInterface
	{
		public delegate bool EngineInitMethod();
		public delegate void EngineUpdateMethod( float delta );
		public delegate void EngineInputMethod( InputEvent @event );

		public EngineInitMethod Init;
		public EngineUpdateMethod Update;
		public EngineUpdateMethod PhysicsUpdate;
		public EngineInputMethod HandleInput;
	}
}
