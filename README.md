# UnityPokeWorldgen

The final goal of this project is to make a Pokemon game that plays like a rougelike. You should be able to hit "new game" and just start playing a comepletely randomly generated Pokemon game. That's it. I have no idea what the rest of it will be, but the project sounded fun. 

The majority of the work will be done in Unity, and there'll be some attempts to stay on the latest version of it. 

## Install Instructions

1. Clone the repository to your computer, through Github Desktop or otherwise.
2. Download Unity Hub from [here](https://unity3d.com/get-unity/download)
3. Install the current Unity version the project is using (2021.2.5f1) 
4. Add the _WorldgenMain_ folder to the projects list in Unity Hub
5. Launch Unity
6. You can try some of what is currently in the project by going to Scenes -> Sample Scene and running around.
    *  Arrow keys to move, X to run.

## Other info

1. This project MAYBE will use compiled Binaries from a [Pokemon Unity Fork](https://github.com/herbertmilhomme/PokemonUnity/tree/TestProject) that decouples a large amount of the code.
    * That said, right now, it doesn't. but it will. You don't need it right now.
    * You can see [my version of the Pokemon Unity](https://github.com/Scew5145/PokemonUnity) repo that I use for compilation of the framework. If you want that.
    * to compile, you may need to edit the output path to somewhere sensible (aka, the Assets/Libraries folder in this repo on your local machine)
    * Also, so that unity doesn't complain, delete the x86 folder and its .Interop.dll file that are generated. Unity HATES duplicately named dlls. Like, so much.
2. There is a known issue with Unity 2019+ where the version control plugin depends on a `Newtonsoft.Json.dll` and will conflict with imported .dll files brought from an external `.csproj.`
    * The "best" fix is to not have the version control plugin enabled, but that's also kind of garbage. There's apparently other workarounds, but I honestly don't really care right now.
    * This might be fixed later, but for now, make sure to disable the VC plugin. You can do all of your version control straight from git / github desktop.

## Requirements and Versions

* if you want to edit PKU source code, the verified working  Pokemon Unity fork for this unity project is here: https://github.com/Scew5145/PokemonUnity
  * That fork of PKU requires:
    * .Net framework 4.7.2
    * System.Data.SQLite 1.0.114.0
* This project uses Unity Version 2020.3.15f2 as of the last update of this readme.

## Contributing

Submit a pull request or poke me on Discord (Ignus#6182) and I'll get you set up if I know you. If I don't, fork the repo and submit a pull request :D


## Major TODOS (to be claimed):

* BW/BW2/HGSS Texture and asset rips: [See Here](https://www.pokecommunity.com/showthread.php?t=357039)
* Finishing the zone streamer code (I'll do this myself probably)
    * Caves and buildings: build a teleport system for going from an entrance to a smaller area
    * Load zones in and out of memory as you approach them
* Building up zone/world/route generators:
    * A small prototype has been built in python for generating the route layout, [here](https://github.com/Scew5145/RandpokeWorldgen/blob/main/worldgen_main.py)
    * I have some small amounts of notes on what I want to do for some of this, poke Scott if you want to work on these.
    * There's going to be a LOT of generators, probably, so this can be claimed by an infinite number of people
* Battle simulator code
* AI code past movement and basic animation
* Story work: 
    * What does the story of a randomly generated world look like?
    * Multiple antagonists? Different bad guy teams per playthrough?
    * dunno lol haven't thought about this much
* Menuing work
    * Building out the pause menu
        * Party, Pokedex, Badges, Save button, etc
    * PC menu
    * Battles

* Saving
    * Some work has been done with saving the layout of a zone once it's generated, but there's a lot more that needs to be done
    * Scott will probably do this
* Audio work
    * Battle Music
    * Town Music
    * Fanfares
    * Button press audio sfx
    * Battle move sfx
* Add more as I think of stuff that needs to happen
* Route Design
    * Even if everything is random, there needs to be some work done on what a route should look like.
    * How many trainers in a route? What types of biomes are there? what should the general layout for a biome look like?
