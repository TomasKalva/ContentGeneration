## Introduction

OurFramework is a framework for fast prototyping of 3D roguelite action RPG games. The gameplay can be created by using C# scripts which make use of already existing libraries for creation of bulidings, enemies, items and much more. 

Creation of 3D action RPGs is a complex and time consuming process. OurFramework helps the designer to focus on the important tasks while minimizing the need to focus on repetitive actions and to write boilerplate code. OurFramework also provides user with existing assets such as models for environment and enemies and visual effects. 

This tutorial covers all the steps needed to create a game using OurFramework.  The reader should be familiar with programming in C#. Most of the C# concepts needed to understand this tutorial can be found [here](https://learnxinyminutes.com/docs/csharp/).

To see how the framework is implemented internally, please refer to the [documentation][documentation].

[documentation]: ???

We are in Visual Studio 2022.
Navigating to declaration of a symbol in visual studio can be done using `Ctrl` + `Left mouse button` click on the symbol.
Moving to previous position of cursor using 
We are in main scene in Unity.

## Installing framework

>Install Unity

>Install Visual Studio 2022

>Install the package to be created

>Our framework uses Noesis so we need a trial license

## A simple level

In this part of the tutorial we will create a simple game with a few buildings, enemies and items.

### Creating a new game

As you probably noticed, the framework contains a lot of scripts and directories. Don't worry about it, for now we will only focus on the core of the framework that is necessary to create a simple game and later on gradually explore more options how to extend the game using more advanced features.

The philosophy of OurFramework is to define most of the game logic using C# scripts. Because the logic can get complicated quite quickly, the scripts are split into modules each of which focuses on a smaller task. The modules are then used to declare what the game should contain.

Let's create a simple module to see how modules work in practice. The module will allow us to start a game with a single building. 

First let's navigate to the folder `AssetsFolderName/LevelDesignLanguage/Modules`. You should see multiple already created modules each put into its own `.cs` file. We will put our new module into a new file too so let's create a new class file called `TutorialModule.cs`. Copy and paste the following code to the file:

```
using OurFramework.Environment.ShapeGrammar;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule : LDLanguage
    {
        public TutorialModule(LanguageParams parameters) : base(parameters) { }
        
        public void Main()
        {
            LevelStart();
        }

        void LevelStart()
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out var area);
            area.Get.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }
    }
}
```


>Create new module

>Put the module to modules
 
Before the game is started the method `DeclareGame` of the module `MainModule` is called. The method contains declaration of events that will get called once the game starts. The method currently contains call to `StartDebugGame` which starts a new game using the already existing modules. We will create the entire game from scratch so to comment out the line with `StartDebugGame();` and add declaration of event from our module the following way:

```
...
public void DeclareGame()
{
    //StartDebugGame();
    State.LC.AddNecessaryEvent($"Tutorial module", 100, level => M.TutorialModule.Main());
}
...
```

At this point the compiler should complain about `TutorialModule`. `M` is an object that should contain references to all existing modules. We need to add our module to it as a property called `TutorialModule`. Navigate to the C# file `AssetsFolderName/LevelDesignLanguage/Modules.cs` and add the following property to the class `Modules`:

```
...
public TutorialModule TutorialModule { get; private set; }
...
```

We need have to initialize it before being able to use it so add call constructor of `TutorialModule` in the method `Initialize`:

```
public void Initialize(LanguageParams languageParams)
{
    ...
    TutorialModule = new TutorialModule(languageParams);
}
```

Now we can go back to Unity and press play. If you've done everything correctly, you should see the following result:



>Call the module from main


>Look at what level start method does

Let's not focus on the parameters, just run the game.

The `Gr.Sym.LevelStartMarker` is used by the framework to find a place where to put the player's character.

### Defining new environment

We already saw the method `One` of the `Env` object. 

Its first argument is of the type `ProductionList`. OurFramework uses shape grammars to generate the environment. It is not necessary to know what a shape grammar is, the important part is that it takes a list of production rules which tell it how to create new buildings and place them to already existing environment. Previously in `LevelStart` we used `Gr.PrL.CreateNewHouse()` to get the `ProductionList`. To see what this method does `Ctrl` + `Left mouse button` click on `CreateNewHouse`. 

You should see the following code:

```
public ProductionList CreateNewHouse()
{
    return new ProductionList
    (
        pr.CreateNewHouse(2)
    );
}
```

The production list has only 1 rule which adds a new house to the level. The rule is implemented using a special language which we will be covered in chapter [missing link]. 

The second argument of `Env.One` is of type `NodesQuery`. The grammar uses nodes of the type `Node` to represent the created buildings. The environment gets created gradually by sequential application of the rules which create `Node`s.  `NodesQuery` is used to find a set of already created nodes. 

A rule typically requires a previous node, which it uses when adding a new node. The second argument of `Env.One` restricts which nodes can be used by application of the defined production rules. In the case of the production list `Gr.PrL.CreateNewHouse()` the one rule it defines doesn't require any existing nodes. We pass in a `NodesQuery` anyway since it's the part of the API and it might become useful if we change the production list in the future.


>Define what the third argument means

`Node` is the type used by the grammars. For usage in the modules which define gameplay it is wrapped into another type `Area`. This type also remembers the objects which are placed into it.

>Now look at the line environment type and use it to create a line 

>Then add branches to it

### Adding enemies

>First place a single enemy (single type, same weapon, no equipment) into each area

>The replace it with placer = strategy to add >objects to multiple areas

>We can introduce more enemies

>We can give enemies different weapons

>We can give enemies different equipment

>The enemies have their strength measured in stats - put enemies to different areas with different stats

Change starting equipment of player

Defining properties of equipment

### Adding loot

>It works the same way as enemies

>Define own item

>We can also place an object - ascension kiln

## A roguelite game

In previous chapters we created a procedural level created by a single event call from the main module. In this chapter we will extend it to a full multilevel roguelite game and add a couple of advanced mechanics.

### Level constructor events

When a new level is started, a bunch of level constructor events are called. These events are typically declared in the method `StartWorld` of `MainModule`.

>Use it to split the game into multiple events - comment on its declarative nature

>Use LevelModule to place end

>Also use LevelModule to replace start

>Add a couple of possible events - npcs with different names

### Interaction

>Interaction with the npcs defined in the previous chapter

>Let the enemies say something and then give the player something - an item

### Items

>Define different types of items - stackable, consumable, wearable...

>Let the npc give these items

>Tell about existing items, spells in particular

### References between objects

>Weapon that deals extra damage to specific enemy

### Locking areas

>Gradually create method BranchWithKey

>Replace it with the actual call

### Existing modules

>OurFramework already contains some modules - describe what they do and how to use them

* Death module
  >how death works
* Environment module
  >say that it creates another skybox and mention how it uses unity
* Faction module
  >the idea behind this module
* Ascending module
  >used for leveling up
* Details module
* Out of depth encounters module

## Shape grammars

In this part of the tutorial we will implement a new grammar using our own production rules.

## Samples



Node
Area
NodeQuery
Areas
ProductionList
Production
Environment
Symbol