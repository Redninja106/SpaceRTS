# SpaceRTS

A data driven space rts game.

## Features

- combat with missiles and chain guns
- base building (defenses, shipyards, power, etc)
- multiplayer

## Prototypes

Inspired by factorio, every actor in the world defines a prototype class. Prototype instances are then deserialized from [The game's assets](https://github.com/Redninja106/SpaceRTS/tree/sf3/SpaceGameAgain/Assets/Prototypes), and used to define the games content. 

This brings a huge amount of reusability to the game's codebase. For example, there are three kinds of ships: 
- small
- medium
- large

Each ship type have a corresponding assembly bay building to create them. The game's coding only defines a `Ship` class and a `AssemblyBay` class. The variation comes from the prototypes: 
- small ship comes from [small_ship.json](https://github.com/Redninja106/SpaceRTS/blob/sf3/SpaceGameAgain/Assets/Prototypes/small_ship.json) and [small_assembly_bay.json](https://github.com/Redninja106/SpaceRTS/blob/sf3/SpaceGameAgain/Assets/Prototypes/Structures/Shipyards/small_assembly_bay.json)
- medium ship comes from [medium_ship.json](https://github.com/Redninja106/SpaceRTS/blob/sf3/SpaceGameAgain/Assets/Prototypes/medium_ship.json) and [medium_assembly_bay.json](https://github.com/Redninja106/SpaceRTS/blob/sf3/SpaceGameAgain/Assets/Prototypes/Structures/Shipyards/medium_assembly_bay.json)
- large ship comes from [large_ship.json](https://github.com/Redninja106/SpaceRTS/blob/sf3/SpaceGameAgain/Assets/Prototypes/large_ship.json) and [large_assembly_bay.json](https://github.com/Redninja106/SpaceRTS/blob/sf3/SpaceGameAgain/Assets/Prototypes/Structures/Shipyards/large_assembly_bay.json)

Additionally, prototypes are used for serialization. each class that inherits `Prototype` must override the `Deserialize(BinaryReader)` method, and when the game world is serializated, objects are sorted by their prototype.

## Serialization

The game has an extensible serialization system. Each actor overrides a `Serialize(BinaryWriter)` method to support serialization, and its prototype overrides `Deserialize(BinaryReader)`. Object types are preserved using the prototype ID.

# Multiplayer

The game uses a deterministic lockstep network model to support multiplayer. The player commands are buffered in 100ms turns, which are executed 200ms after they are submitted. After a turn ends, each client sends its commands to the server, which broadcasts them to every other client.  

## Large worlds

Every actor uses 64-bit floats for its position, allowing for huge worlds spanning multiple star systems. 

## Screenshots

![image](https://github.com/user-attachments/assets/d7ef8cb4-52d5-4194-bfd7-2e0891d551d2)
![image](https://github.com/user-attachments/assets/a5a6620b-5b02-4d7e-a33d-8375cf174305)
![image](https://github.com/user-attachments/assets/b16c02de-c013-43c0-84cc-be6eaa5cfe7d)
![image](https://github.com/user-attachments/assets/17784b81-7164-4d93-bdcd-d612b101d4f6)
![image](https://github.com/user-attachments/assets/9e441ff4-2099-4b67-a430-e1b400abb4b0)


