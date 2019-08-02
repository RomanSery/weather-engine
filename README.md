# weather-engine


### Directory structure
Brief description of all directories in the project.

| Directory | Description |
| ------ | ------ |
| BuildContent | Tool to convert 3D assets from 3DMax into .X format usable by the engine. |
| BuildTextures | Tool used by world editor to process/convert textures into the correct format and create normal maps |
| ColorPickerControl | Color picking control, frequently used in WorldEditor |
| Content | Contains all assets such as meshes, textures, sounds, shaders, compiled scripts, animation data |
| Framework | The main engine code. Handles user input, physics, rendering, saving/loading world state |
| Main | Entry point for launching the "game" built with the world editor |
| RenderLoop | Device manager, chooses rendering settings, resolution, manages render targets, tells the engine when to render the frame and how often |
| WorldEditor | World editor for creating areas, cells, meshes, lights, weather effects, scripts |
