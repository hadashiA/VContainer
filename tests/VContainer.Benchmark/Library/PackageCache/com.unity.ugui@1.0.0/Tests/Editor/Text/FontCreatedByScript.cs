using System;
using NUnit.Framework;
using UnityEngine;

[Category("Text")]
public class FontCreatedByScript
{
    static Font CreateDefaultFontWithOneCharacter(int character)
    {
        var font = new Font();
        CharacterInfo[] characterInfo = new CharacterInfo[1];
        characterInfo[0].index = character;
        font.characterInfo = characterInfo;
        return font;
    }

    [Test]
    public static void GetCharacterInfo_FindsCharacterInfoThatIsInSet()
    {
        char character = 'A';
        int charIndex = Convert.ToInt32(character);

        var font = CreateDefaultFontWithOneCharacter(charIndex);
        CharacterInfo result = new CharacterInfo();
        Assert.IsTrue(font.GetCharacterInfo(character, out result), "Could not find character info for '" + character + "' even though the Font contains it.");
        Assert.AreEqual(charIndex, result.index, "Incorrect character info was returned for " + character);
    }

    [Test]
    public static void GetCharacterInfo_DoesNotFindCharacterInfoThatIsNotInSet()
    {
        char character = 'A';
        char characterNotInSet = 'X';
        int charIndex = Convert.ToInt32(character);

        var font = CreateDefaultFontWithOneCharacter(charIndex);
        CharacterInfo result;
        Assert.IsFalse(font.GetCharacterInfo(characterNotInSet, out result), "Found character info for '" + characterNotInSet + "' even though the Font does not contain it.");
    }

    [Test]
    public static void HasCharacterReturns8BitChars()
    {
        char character = 'A';
        int charIndex = Convert.ToInt32(character);

        var font = CreateDefaultFontWithOneCharacter(charIndex);
        Assert.IsTrue(font.HasCharacter(character), "HasCharacter returned false even though it should have " + character);
    }

    [Test]
    public static void HasCharacterReturns16BitChars()
    {
        char character = '\u03A9';
        int charIndex = Convert.ToInt32(character);

        var font = CreateDefaultFontWithOneCharacter(charIndex);
        Assert.IsTrue(font.HasCharacter(character), "HasCharacter returned false even though it should have " + character);
    }
}
