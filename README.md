# OracleHack [![Build Status](https://travis-ci.org/kabili207/oracle-hack.svg?branch=master)](https://travis-ci.org/kabili207/oracle-hack)

A library for working with the password system used in the Legend of Zelda Oracle of Ages and Oracle of Seasons games.
Inspired by the [original password generator](http://home.earthlink.net/~paul3/zeldagbc.html) written by
Paul D. Shoener III a.k.a. Paulygon back in 2001.

### User Interfaces
OracleHack is only a library for manipulating the codes from the Oracle series games, meant for use by developers.
General users should instead use of the the following user interfaces:
 * Windows WPF - https://github.com/kabili207/oracle-hack-win
 * Linux GTK - https://github.com/kabili207/oracle-hack-gtk

### Features
 * Decodes game and ring secrets
 * Generates game, ring, and memory secrets
 * Allows import and export of game data using a json-based `.zora` file
 
## The .zora save file
The `.zora` file contains all relevent information to recreate a player's game and ring secrets. Data is saved
as a JSON object, with the intention that it can be used with other implementations of the password system.
The file gets it's name from a contraction of **Z**elda **Ora**cle and from the Zora, one of the games'
races and enemies.

```json
{
    "Hero": "Kabi",
    "GameID": 21353,
    "Game": "Ages",
    "Child": "Derp",
    "Animal": "Moosh",
    "Behavior": "Infant",
    "IsLinkedGame": true,
    "IsHeroQuest": false,
    "Rings": -5188093993887465139
}
```

Valid values for `Behavior` are: `Infant`,
`BouncyA`, `BouncyB`, `BouncyC`, `BouncyD`, `BouncyE`,
`ShyA`, `ShyB`, `ShyC`, `ShyD`, `ShyE`,
`HyperA`, `HyperB`, `HyperC`, `HyperD`, `HyperE`.

Valid values for `Game` are `Ages` or `Seasons`. This value refers to the _target_ game.

Valid values for `Animal` are `Ricky`, `Dimitri`, or `Moosh`.

The rings are saved as a 64 bit signed integer. A signed integer was chosen to maintain compatibility with
languages that don't support unsigned integers.

None of the fields are required; the OracleHack library will load whatever is present, however the same
cannot be guaranteed for other libraries that implement the `.zora` save file.

## Using the library

### Getting the raw secret
OracleHack uses byte arrays for most operations with the secrets. Most people don't go passing byte values
around, however, opting for a more readable text representation. These secret strings can be parsed like so:
```c#
string gameSecret = "#3GGr q59s right qY)*? =G* left (";
byte[] rawGameSecret = GameInfo.SecretStringToByteArray(gameSecret);
```
The `SecretStringToByteArray` method is fairly flexible in what it will accept. Possible formats include 
`→N♥Nh`, `right N heart N h`, and `{right}N{heart}Nh`. You can even combine the formats like so: 
`RigHtN ♥N h`. Brackets (`{` and `}`) also ignored, so long as they are next to a symbol word such as
`circle` or `left`.

Whitespace is also ignored. This does not cause any issues with the symbol words because the list of valid
characters does not include any vowels.

### Getting a secret string
It's also possible to take the raw bytes and convert them back into a readable string value.
```c#
byte[] rawSecret = new byte[]
    {
        12, 52,  3,  3, 40,
        39, 54, 58, 41, 62,
        39, 19, 50, 34, 45,
        49,  3, 34, 61, 48
    };
string secret = GameInfo.ByteArrayToSecretString(rawSecret);
// #3GGr q59s→ qY)*? =G*←(
```

The `ByteArrayToSecretString` method is far less flexible than it's counter-part, and will only return
a UTF-8 string, as shown above.

### Loading a game secret
```c#
byte[] rawGameSecret = new byte[] { 12, 52, 3, 3, 40, ... };
GameInfo info = new GameInfo();
info.LoadGameData(rawGameSecret);
```

### Loading a ring secret
```c#
bool appendRings = false;
byte[] rawRingSecret = new byte[]
    {
      14, 52,  3,  8, 58,
      35, 25, 56, 45, 12,
      54, 55,  9, 41, 61
    };
info.LoadRings(rawRingSecret, appendRings);
```

### Creating a game secret
```c#
byte[] rawGameSecret = info.CreateGameSecret();
string gameSecret = GameInfo.ByteArrayToSecretString(rawGameSecret);
// #3GGr q59s→ qY)*? =G*←(
```

### Creating a ring secret
```c#
byte[] rawRingSecret = info.CreateGameSecret();
string ringSecret = GameInfo.ByteArrayToSecretString(rawRingSecret);
// Q3G♠9 /-7?# 56♥s←
```

### Creating a memory secret
```c#
bool isReturnSecret = true;
byte[] memorySecret = info.CreateMemorySecret(Memory.SmithLibrary, isReturnSecret);
string secret = GameInfo.ByteArrayToSecretString(memorySecret);
// →BYq↑
```

## Special Thanks
 * Paulygon - Created the [original secret generator](http://home.earthlink.net/~paul3/zeldagbc.html) way back in 2001
 * 39ster - Rediscovered [how to decode game secrets](http://www.gamefaqs.com/boards/472313-the-legend-of-zelda-oracle-of-ages/66934363) using paulygon's program
 * [LunarCookies](https://github.com/LunarCookies) - Discovered the correct cipher and checksum logic used to generate secrets
