## Introduction

OurFramework is a framework for fast prototyping of 3D roguelite action RPG games. The gameplay can be created by using C# scripts which make use of already existing libraries for creation of bulidings, enemies, items and much more. 

Creation of 3D action RPGs is a complex and time consuming process. OurFramework helps the designer to focus on the important tasks while minimizing the need to write a boilerplate code. OurFramework also provides user with existing assets such as models for environment and enemies and visual effects. 

This tutorial covers all the steps needed to create a game using OurFramework.  The reader should be familiar with programming in C#. To see how the framework is implemented internally, please refer to the [documentation][documentation].

[documentation]: ???

### Creating a new game




```
class TutorialGameLanguage : LDLanguage
{
    public TutorialGameLanguage(LanguageParams parameters) : base(parameters) { }
    
    public void Main()
    {

    }
}
```