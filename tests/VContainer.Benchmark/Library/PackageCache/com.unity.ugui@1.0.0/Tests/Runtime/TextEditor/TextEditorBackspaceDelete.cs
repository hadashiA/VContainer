using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TextEditorBackspaceDelete
{
    private const string kFailedToRemoveCharacterMessage = "Backspace or Delete Failed To Remove The Expected Character";
    private const string kFailedToChangeCursor = "Backspace or Delete Failed To Move The Cursor To The Expected Index";
    private const string kFailedToChangeSelect = "Backspace or Delete Failed To Move The Selection Index To The Expected Index";

    [Test]
    public void TextEditorWithUTF16_MiddleCursorOnBackspace_RemovesCharacter()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy",
            cursorIndex = 4,
            selectIndex = 4,
        };
        textBox.UpdateTextHandle();

        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(3, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(3, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_MiddleCursorOnDelete_RemovesCharacter()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy",
            cursorIndex = 3,
            selectIndex = 3,
        };
        textBox.UpdateTextHandle();

        textBox.UpdateTextHandle();

        textBox.Delete();

        Assert.AreEqual("MikDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(3, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(3, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_MiddleCursorOnBackspaceAndLeftSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "Mike🗘DeRoy",
            cursorIndex = 5,
            selectIndex = 5,
        };
        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(4, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(4, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_MiddleCursorOnDeleteAndLeftSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "Mike🗘DeRoy",
            cursorIndex = 4,
            selectIndex = 4,
        };
        textBox.UpdateTextHandle();

        textBox.Delete();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(4, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(4, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_MiddleCursorOnBackspaceAndRightSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "Mike🗘DeRoy",
            cursorIndex = 5,
            selectIndex = 5,
        };
        textBox.UpdateTextHandle();;

        textBox.Backspace();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(4, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(4, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_LeftCursorOnBackspace_DoesNotRemoveCharacter()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy",
            cursorIndex = 0,
            selectIndex = 0,
        };
        textBox.UpdateTextHandle();

        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(0, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(0, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_LeftCursorOnDelete_RemovesCharacter()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy",
            cursorIndex = 0,
            selectIndex = 0,
        };
        textBox.UpdateTextHandle();

        textBox.UpdateTextHandle();

        textBox.Delete();

        Assert.AreEqual("ikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(0, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(0, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_LeftCursorOnBackspaceAndLeftSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "🗘MikeDeRoy",
            cursorIndex = 1,
            selectIndex = 1,
        };
        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(0, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(0, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_LeftCursorOnDeleteAndLeftSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "🗘MikeDeRoy",
            cursorIndex = 0,
            selectIndex = 0,
        };
        textBox.UpdateTextHandle();

        textBox.Delete();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(0, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(0, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_LeftCursorOnBackspaceAndRightSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "🗘MikeDeRoy",
            cursorIndex = 1,
            selectIndex = 1,
        };
        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(0, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(0, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_RightCursorOnBackspace_RemovesCharacters()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy",
            cursorIndex = 9,
            selectIndex = 9,
        };
        textBox.UpdateTextHandle();

        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikeDeRo", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(8, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(8, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_RightCursorOnDelete_DoesNotRemoveCharacter()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy",
            cursorIndex = 9,
            selectIndex = 9,
        };
        textBox.UpdateTextHandle();

        textBox.UpdateTextHandle();

        textBox.Delete();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(9, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(9, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_RightCursorOnBackspaceAndLeftSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy🗘",
            cursorIndex = 10,
            selectIndex = 10,
        };
        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(9, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(9, textBox.selectIndex, kFailedToChangeSelect);
    }


    [Test]
    public void TextEditorWithUTF16_RightCursorOnDeleteAndLeftSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy🗘",
            cursorIndex = 9,
            selectIndex = 9,
        };
        textBox.UpdateTextHandle();

        textBox.Delete();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(9, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(9, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_RightCursorOnBackspaceAndRightSurrogate_RemovesBothSurrogates()
    {
        var textBox = new TextEditor()
        {
            text = "MikeDeRoy🗘",
            cursorIndex = 11,
            selectIndex = 11,
        };
        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("MikeDeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(9, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(9, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_MiddleCursorOnBackspace_RemovesBothSurrogatesInSuccession()
    {
        var textBox = new TextEditor()
        {
            text = "Mike🗘🗘🗘DeRoy",
            cursorIndex = 6,
            selectIndex = 6,
        };
        textBox.UpdateTextHandle();

        textBox.Backspace();

        Assert.AreEqual("Mike🗘🗘DeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(5, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(5, textBox.selectIndex, kFailedToChangeSelect);

        textBox.Backspace();

        Assert.AreEqual("Mike🗘DeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(4, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(4, textBox.selectIndex, kFailedToChangeSelect);

        textBox.Backspace();
        Assert.AreEqual("Mik🗘DeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(3, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(3, textBox.selectIndex, kFailedToChangeSelect);
    }

    [Test]
    public void TextEditorWithUTF16_MiddleCursorOnDelete_RemovesBothSurrogatesInSuccession()
    {
        var textBox = new TextEditor()
        {
            text = "Mike🗘🗘🗘DeRoy",
            cursorIndex = 5,
            selectIndex = 5,
        };
        textBox.UpdateTextHandle();

        textBox.Delete();
        textBox.UpdateTextHandle();

        Assert.AreEqual("Mike🗘🗘DeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(5, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(5, textBox.selectIndex, kFailedToChangeSelect);

        textBox.Delete();
        textBox.UpdateTextHandle();

        Assert.AreEqual("Mike🗘DeRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(5, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(5, textBox.selectIndex, kFailedToChangeSelect);

        textBox.Delete();
        textBox.UpdateTextHandle();
        Assert.AreEqual("Mike🗘eRoy", textBox.text, kFailedToRemoveCharacterMessage);
        Assert.AreEqual(5, textBox.cursorIndex, kFailedToChangeCursor);
        Assert.AreEqual(5, textBox.selectIndex, kFailedToChangeSelect);
    }
}
