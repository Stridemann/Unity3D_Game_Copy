# Unity3D_Game_Copy
Sends all the data from current loaded level in any Unity3D game to editor (replicate it) using TCP protocol. Gameobjects, models, textures (only main texture from shader), some basic components (BoxCollider, Light)

![Image](https://raw.githubusercontent.com/Stridemann/Unity3D_Game_Copy/master/Screenshots/1.png)

![Image](https://raw.githubusercontent.com/Stridemann/Unity3D_Game_Copy/master/Screenshots/2.png)
 
![Image](https://raw.githubusercontent.com/Stridemann/Unity3D_Game_Copy/master/Screenshots/4.gif)

I'm using Unity 2018.3.0b9, but should works on older (201X) versions.

For game side:
Compile the dll version of this project ~~(create "DLL/Library" project in visual studio (NET 3.5!!!), add all sources in this project, add reference to GameFolder/%MyGame%_Data\Managed/UnityEngine.dll)~~ Use GameCopy_Game project to build it.

Inject/Call function GameCopy_Game._LoadModule in game (I'm using .NET Reflector + Reflexil plugin. Open Assembly-CSharp.dll add CALL IL instruction in some Awake/Start function in some main script). If injection was successfull you will see Connect button on game screen)

In editor just start the GameCopy scene. 

Then press Connect in game, then Send The World, Then Send Components.

"Save Tex" in GameCopy_Editor: check it ON for first export the game data. Textures will be saved to Resources folder. Uncheck it for all the next exports. 

Use "Send Used Textures" button in editor (when the game is connected) to send the texture names that you have already cached in Resource folder, it will not send it's data, it will only use cached. After this you can press Export Components and it will use cached textures.

How everything other works - define urself/read code, etc. 
No support to this code.
