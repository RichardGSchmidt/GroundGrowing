# GroundGrowing
Open Source Unity3d Planetary Terrain Generator

This project is not completed, but is still usable.

It currently uses the #region style organization,
so the code looks best in Visual Studio; where
the regions expand like folders.

Most interfaces are on the Editor panel in Unity,
but I recommend going through the code regardless
for now if you are seriously going to use it.

Current features include multi-threaded proceedural
terrain generation at various resolutions, using multiple
noise types that can be stacked and layered courtesy of a
C# implementation of the LibNoise Library.

Things that need to be added:

Conversion from 2d arrays to 1d arrays for textures
to avoid the hiccup on texture updates.
Terrain group serialization storage and file IO.  
Partial planet renderer that scales for rendering
ground / close up scenes (the flat mesh generator which is currently broken).
Autosea Level.
Ice cap noise inserts.
Rivers.
Terrain group selection mapping.
And a sun generator (really easy, I'm just slacking).
