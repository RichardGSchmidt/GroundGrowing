# GroundGrowing
**An Open Source Unity3d Planetary Terrain Generator and Editor Extension**

This project is built as an extension to the editor, it is meant as a build tool for designers looking to generate content rapidly on the fly.  The generation functions can also be exported and called inside other applications.  It is multithreaded in both the inspector and during runtime so it can update and generate without slowing down either the player's or the designer's user experience.  It updates in incrementally increasing resolutions start with low res in order to provide modes rapidly to the user while higher resolution models are processed in the background.

**Click ⬇️ for in Browser Demo Hosted on a React Front End!**

[![An Example Planet](https://i.imgur.com/8zgkg4y.png?2)](https://make-planet.com)]




**To Start**
1.  Open the NoiseGenerator scene inside the assets/scripts folder.
2.  Double-Click the planet in the scene hierachy to center your view on the planet object.  Adjust your view as desired.
3.  Click the MapGenerator object in the scene hierarcy.  The controls will be located in the inspector window under the MapGenerator script.
4.  The noise stack's behavior can be modified in the **Noise Stack** dropdown in the editor.  Coloring based on height maps is under the **Regions** dropdown in the editor.
5.  The generator will reprocess every time you change the noise stack or hit the generate button automatically.

**Noise Stack**
* The noise stack is made of noise functions that can be either added or subtracted from each other in order to blend them.
* Supported noise types are: Perlin, Billow, Ridged Multifractal, and Voronoi.  I recommend experimenting with all of them independently before you start blending, so you have some idea of what they look like.
* You can load or save presets to file for later use.

**Regions**
*  Height relative to the approximate floor value and ceiling values is used to determine the map color at a given location.
*  Presets can be loaded or saved to file.


You can export the mesh's and the textures manually.

Currently updated to version 2021.3.6f1 of Unity3d.

Enjoy.


Feel free to ask me any questions if you have them.  
I'm easiest to reach via email:  [Richard.G.Schmidt.Jr@gmail.com]. If you do happen to
use this in a project, please let me know and credit me.

Thanks and have fun!
-Richard
