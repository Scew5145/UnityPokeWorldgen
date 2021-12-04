# UnityPokeWorldgen

Why are you reading this right now?

This repo is still under construction. Come back in a few months.

...

I guess, since you're here, I can give you some random information that will eventually be useful to... someone. Probably me. Future me.

## Requirements and Versions

* if you want to edit PKU source code, the verified working  Pokemon Unity fork for this unity project is here: https://github.com/Scew5145/PokemonUnity
  * That fork of PKU requires:
    * .Net framework 4.7.2
    * System.Data.SQLite 1.0.114.0
* This project uses Unity Version 2020.3.15f2 as of the writing of this readme.

## Other info

1. This project uses compiled Binaries from a [Pokemon Unity Fork](https://github.com/herbertmilhomme/PokemonUnity/tree/TestProject) that decouples a large amount of the code.
    * You can see [my version of the Pokemon Unity](https://github.com/Scew5145/PokemonUnity) repo that I use for compilation of the framework. If you want that.
    * to compile, you may need to edit the output path to somewhere sensible (aka, the Assets/Libraries folder in this repo on your local machine)
    * Also, so that unity doesn't complain, delete the x86 folder and its .Interop.dll file that are generated. Unity HATES duplicately named dlls. Like, so much.
2. There is a known issue with Unity 2019+ where the version control plugin depends on a `Newtonsoft.Json.dll` and will conflict with imported .dll files brought from an external `.csproj.`
    * The "best" fix is to not have the version control plugin enabled, but that's also kind of garbage. There's apparently other workarounds, but I honestly don't really care right now.
    * This might be fixed later, but for now, make sure to disable the VC plugin. 

## Contributing

dont
