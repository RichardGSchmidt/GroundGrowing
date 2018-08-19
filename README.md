# GroundGrowing
Open Source Unity3d Planetary Terrain Generator

This project is not completed, but is still usable.

Most interfaces are on the Editor panel in Unity,
but I recommend going through the code regardless
for now if you are seriously going to use it.

Current features include multi-threaded proceedural
terrain generation at various resolutions, using multiple
noise types that can be stacked and layered courtesy of a
C# implementation of the LibNoise Library.

Things that need to be added:

Terrain group serialization storage and file IO.  
Partial planet renderer that scales 
(the flat mesh generator which is currently broken).
Autosea Level.
