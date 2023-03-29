# Spoons Source
This is the in-development source code for a game called _Would You Like To Buy A Spoon?_ (WYLT-BAS). This project is in active development and I intend to release it on Steam when it is complete. However, I plan to keep the source code open, accessible, and free.

![spoons](https://user-images.githubusercontent.com/3848374/228410797-9b1236f1-0aae-4509-854e-c06647d38dd7.gif)


## Overview
WYLT-BAS is a game about the future of work and human creativity in the dawn of the AI age. In the game, the player assumes the role of a sales-person for a fake company, _Meta-American_, and must take on the job of selling spoons in a door to door virtual world. The inhabitants of the virtual houses are powered by [OpenAI's _text-davinci-003_ model](https://platform.openai.com/docs/api-reference/completions/create). The houses themselves are generated via [Scenario's](https://www.scenario.com/) image generator (trained on a limited model of my own design). The game's architecture is built on [Beamable](https://docs.beamable.com/docs).

## Getting Started
This project is in active development, so the following section for _Getting Started_ may fall out of date quickly. This section is for anyone who wants to learn about how Beamable, OpenAI, and ScenarioGG can all be sitched together. Feel free to fork this repository and do whatever you will with it. With that minor disclaimer out of the way, there are some prerequisites.

You will need...
1. an [API key for OpenAI](https://platform.openai.com/account/api-keys).
2. an API key for [Scenario](https://www.scenario.com/). (At the time of this writing, the API is in early access, and you will need to request a spot in line to get a key.)
3. a trained model in Scenario from which to acquire house images. 
4. a [Beamable](https://docs.beamable.com/docs/beamable-overview) organization and admin sign in credential.

The general structure of the repository has 2 main projects, the Unity game, and a standalone Microservice. Traditionally in Beamable, Microservices are bound to the Unity game's .NET development environment. However, there is a new experimental feature that allows Microservices to be developed freely outside of Unity. This [documentation](https://docs.beamable.com/docs/standalone-microservices) explains the basics, but it is still in heavy development by the Beamable team.

```
/
    /Spoons # this is the Microservice code
        /services
            /Spoons # this is where the server code is
            /SpoonsCommon # this is where shared code between Unity and the server live
    /SpoonsUnity # this is the Unity game folder
```

You'll need to use the Beamable credentials to sign into the Beamable plugin in Unity once you've forked the repo.

Then, you'll need to enter the OpenAI API key and the Scenario API key into Beamable realm config. Realm config is controlled in the Beamable portal in the _Operate/Config_ section. 
* You need a namespace called _game_ 
    * put the OpenAI key under the _game_ namespace in a property called _openapi_
    * put the Scenario key under the _game_ namespace in a property called _scenariogg_
You can go to the [`Config`](https://github.com/cdhanna/spoons/blob/main/Spoons/services/Spoons/Services/Config.cs) class to see how the values are read from realm config.

Finally, (and this is likely to change quickly), you need to change the [`modelId`](https://github.com/cdhanna/spoons/blob/main/Spoons/services/Spoons/Services/ScenarioGG.cs#L34) in the `ScenarioGG` (Scenario used to be called ScenarioGG) class to point to a trained model available in your Scenario account. 

Once all of these steps have been taken, you are ready to start running the program! 
1. Start the standalone Microservice!
2. Run the _Assets/Spoons/Scenes/GameScene.unity_ in the Unity project. 

## Credits

* [Shapeforms Audio Free Sound Effects](https://assetstore.unity.com/packages/audio/sound-fx/shapeforms-audio-free-sound-effects-183649) - This is a free asset with some great sound effects including the tape effects. 
* [Procedural Music Generator](https://assetstore.unity.com/packages/audio/music/procedural-music-generator-192532) - This is a free asset that I took the instrumental sound effects from.
* [officina27](https://pixabay.com/music/pulses-00-officina-zanchi-synt-bells-4697/) - The artist who created the menacing background music.
