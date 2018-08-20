# GroundGrowing
Open Source Unity3d Planetary Terrain Generator

This project is not completed, but is still usable.
This is also the working copy as well, so things
may break / change without notice as I attempt to
improve it.  There will also be notes to myself 
all throughout the code and documentation.

A video to an earlier build of the program
can be found at:
https://www.youtube.com/watch?v=UYVcR3TiSrw

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

TODO:



*Code reduction / hiearchy seperation.

*Multicore rendering off switch.

*Fix / improve multithreading implementation. <DONE RGS 8/19/18>

*Better fuctions to run the generator instantiated with 
variety of presets settings or files.

*Conversion from 2d arrays to 1d arrays for textures
to avoid the hiccup on texture updates. 

*Move the timestamp checker deeper into the texture draw
method to cause thread halts earlier after changes.  

*Better Shaders / terrain groups.

*Terrain group serialization storage and file IO.  

*Ocean toggle.  <DONE RGS 8/19/18>

*Custom noise inserts(e.g. gas giants, ice caps, stars.).

*Rivers city / glow overlays.

*Terrain group selection mapping. 

*Partial planet renderer that scales for rendering
ground / close up scenes (the flat mesh generator 
which is currently broken).


If there is anything else not implemented or on the TODO list that you want to suggest please let me know.  
Also feel free to ask me any questions if you have them.  
I'm easiest to reach via email:  [Richard.G.Schmidt.Jr@gmail.com]. If you do happen to 
use this in a project, please let me know and credit me.

Thanks and have fun!
-Richard Schmidt
