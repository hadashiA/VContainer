using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class TextEditorTests
{
    TextEditor m_TextEditor;

    static IEnumerable textWithCodePointBoundaryIndices
    {
        get
        {
			yield return new TestCaseData(null, new[] { 0 });
            yield return new TestCaseData("", new[] { 0 });
            yield return new TestCaseData("abc", new[] { 0, 1, 2, 3 });

            yield return new TestCaseData("\U0001f642", new[] { 0, 2 }).SetName("(U+1F642)");
            yield return new TestCaseData("\U0001f642\U0001f643", new[] { 0, 2, 4 }).SetName("(U+1F642)(U+1F643)");
            yield return new TestCaseData("a\U0001f642b\U0001f643c", new[] { 0, 1, 3, 4, 6, 7 }).SetName("a(U+1F642)b(U+1F643)c");

            // Unstable - https://jira.unity3d.com/browse/UUM-19454
            // yield return new TestCaseData("Hello 😁 World", new[] { 0, 1, 2, 3, 4, 5, 6, 8, 9, 10, 11, 12, 13, 14 }).SetName("Hello (U+1F601) World");
            // yield return new TestCaseData("見ざる🙈、聞かざる🙉、言わざる🙊", new[] { 0, 1, 2, 3, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 19 }).SetName("Three wise monkeys");
        }
    }

    static IEnumerable textWithWordStartAndEndIndices
    {
        get
        {
			yield return new TestCaseData(null, new int[0], new int[0]);
            yield return new TestCaseData("", new int[0], new int[0]);
            yield return new TestCaseData(" ", new int[0], new int[0]);
            yield return new TestCaseData("one two three", new[] { 0, 4, 8 }, new[] { 3, 7, 13 });

            // Unstable - https://jira.unity3d.com/browse/UUM-19454
            // yield return new TestCaseData("\U00010000 \U00010001 \U00010002\U00010003", new[] { 0, 3, 6 }, new[] { 2, 5, 10 }).SetName("(U+10000) (U+10001) (U+10002)(U+10003)");
            // yield return new TestCaseData("Hello 😁 World", new[] { 0, 6, 9 }, new[] { 5, 8, 14 }).SetName("Hello (U+1F601) World");
            // yield return new TestCaseData("見ざる🙈、聞かざる🙉、言わざる🙊", new[] { 0, 3, 6, 10, 13, 17 }, new[] { 3, 6, 10, 13, 17, 19 }).SetName("Three wise monkeys");
        }
    }

    // A sequences of punctuation characters is currently considered a word when deleting
    static IEnumerable textWithWordStartAndEndIndicesWherePunctuationIsAWord
    {
        get
        {
            yield return new TestCaseData(" ,. abc,. ", new[] { 1, 4, 7 }, new[] { 3, 7, 9 });
        }
    }

    // but not when moving/selecting
    static IEnumerable textWithWordStartAndEndIndicesWherePunctuationIsNotAWord
    {
        get
        {
            yield return new TestCaseData(" ,. abc,. ", new[] { 4 }, new[] { 7 });
        }
    }

    static IEnumerable textWithLineStartIndices
    {
        get
        {
            yield return new TestCaseData("\n\na\nbc\n\nd\n ", new[] { 0, 1, 2, 4, 7, 8, 10 }).SetName("(LF)(LF)a(LF)bc(LF)(LF)d(LF)");
            yield return new TestCaseData("\n\na\nbc\n\U0001f642\n\U0001f642\U0001f643\n\n ", new[] { 0, 1, 2, 4, 7, 10, 15, 16 }).SetName("(LF)(LF)a(LF)bc(LF)(U+1F642)(LF)(U+1F642)(U+1F643)(LF)(LF) ");
            yield return new TestCaseData("\n\na\nbc\n🙂\n🙂🙃\n\n ", new[] { 0, 1, 2, 4, 7, 10, 15, 16 }).SetName("(LF)(LF)a(LF)bc(LF)(U+1F642)(LF)(U+1F642)(U+1F643)(LF)(LF) ");
        }
    }

    static IEnumerable textWithLineEndIndices
    {
        get
        {
            yield return new TestCaseData("\n\na\nbc\n\nd\n ", new[] { 0, 1, 3, 6, 7, 9, 11 }).SetName("(LF)(LF)a(LF)bc(LF)(LF)d(LF) ");
            yield return new TestCaseData("\n\na\nbc\n\U0001f642\n\U0001f642\U0001f643\n\n ", new[] { 0, 1, 3, 6, 9, 14, 15, 17 }).SetName("(LF)(LF)a(LF)bc(LF)(U+1F642)(LF)(U+1F642)(U+1F643)(LF)(LF) ");
            yield return new TestCaseData("\n\na\nbc\n🙂\n🙂🙃\n\n ", new[] { 0, 1, 3, 6, 9, 14, 15, 17 }).SetName("(LF)(LF)a(LF)bc(LF)(U+1F642)(LF)(U+1F642)(U+1F643)(LF)(LF) ");
        }
    }

    static IEnumerable textWithExpectedCursorAndSelectIndicesWhenSelectingCurrentWordAtIndex
    {
        get
        {
			yield return new TestCaseData(null, new[] { 0 }, new[] { 0 });
            yield return new TestCaseData("", new[] { 0 }, new[] { 0 });
            yield return new TestCaseData(" ", new[] { 1, 1 }, new[] { 0, 0 });
            yield return new TestCaseData("a", new[] { 1, 1 }, new[] { 0, 0 });
            yield return new TestCaseData("ab", new[] { 2, 2, 2 }, new[] { 0, 0, 0 });
            yield return new TestCaseData("ab  cd", new[] { 2, 2, 4, 4, 6, 6, 6 }, new[] { 0, 0, 2, 2, 4, 4, 4 });
            yield return new TestCaseData("a,,  ,,  ,,b", new[] { 1, 3, 3, 5, 5, 7, 7, 9, 9, 11, 11, 12, 12 }, new[] { 0, 1, 1, 3, 3, 5, 5, 7, 7, 9, 9, 11, 11 });
        }
    }

    [SetUp]
    public void TestSetup()
    {
        m_TextEditor = new TextEditor();
        m_TextEditor.DetectFocusChange();
    }

    [Test]
    public void SetText_MovesCursorAndSelectIndicesToNextCodePointIndexIfInvalid()
    {
        m_TextEditor.text = "ab";
        m_TextEditor.UpdateTextHandle();

        m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = 1;

        m_TextEditor.text = "\U0001f642";
        m_TextEditor.UpdateTextHandle();

        Assert.AreEqual(2, m_TextEditor.stringCursorIndex, "cursorIndex at invalid code point index");
        Assert.AreEqual(2, m_TextEditor.stringSelectIndex, "selectIndex at invalid code point index");
    }

    [Test, TestCaseSource("textWithCodePointBoundaryIndices")]
    public void SetCursorAndSelectIndices_MovesToNextCodePointIndexIfInvalid(string text, int[] codePointIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.stringCursorIndex = index;
            m_TextEditor.stringSelectIndex = index;

            var nextCodePointIndex = index == GetLength(text) ? index : codePointIndices.First(codePointIndex => codePointIndex > index);
            if (codePointIndices.Contains(index))
                Assert.AreEqual(index, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} should not change if it's already at a valid code point index", index));
            else
                Assert.AreEqual(nextCodePointIndex, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to next code point index", index));
            if (codePointIndices.Contains(index))
                Assert.AreEqual(index, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} should not change if it's already at a valid code point index", index));
            else
                Assert.AreEqual(nextCodePointIndex, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to next code point index", index));
        }
    }

    [Test, Ignore("Disabled due to instability UUM-19454")]
    [TestCaseSource("textWithWordStartAndEndIndices")]
    [TestCaseSource("textWithWordStartAndEndIndicesWherePunctuationIsAWord")]
    public void DeleteWordBack_DeletesBackToPreviousWordStart(string text, int[] wordStartIndices, int[] wordEndIndices)
    {
        for (var stringIndex = 0; stringIndex <= GetLength(text); stringIndex++)
        {
            m_TextEditor.text = text;
            m_TextEditor.UpdateTextHandle();
            m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = stringIndex;
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.DeleteWordBack();

            var previousWordStart = wordStartIndices.Reverse().FirstOrDefault(i => i < oldCursorIndex);
            Assert.AreEqual(previousWordStart, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to previous word start", oldCursorIndex));
            Assert.AreEqual(previousWordStart, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to previous word start", oldSelectIndex));
            Assert.AreEqual(text.Remove(previousWordStart, oldCursorIndex - previousWordStart), m_TextEditor.text, string.Format("wrong resulting text for cursorIndex {0}", oldCursorIndex));
        }
    }

    [Test, Ignore("Disabled due to instability UUM-19454")]
    [TestCaseSource("textWithWordStartAndEndIndices")]
    [TestCaseSource("textWithWordStartAndEndIndicesWherePunctuationIsAWord")]
    public void DeleteWordForward_DeletesForwardToNextWordStart(string text, int[] wordStartIndices, int[] wordEndIndices)
    {
        for (var stringIndex = 0; stringIndex <= GetLength(text); stringIndex++)
        {
            m_TextEditor.text = text;
            m_TextEditor.UpdateTextHandle();
            m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = stringIndex;
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.DeleteWordForward();

            var nextWordStart = oldCursorIndex == text.Length ? text.Length : wordStartIndices.Concat(new[] { text.Length }).First(i => i > oldCursorIndex);
            Assert.AreEqual(oldCursorIndex, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} should not change", oldCursorIndex));
            Assert.AreEqual(oldSelectIndex, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} should not change", oldSelectIndex));
            Assert.AreEqual(text.Remove(oldCursorIndex, nextWordStart - oldCursorIndex), m_TextEditor.text, string.Format("wrong resulting text for cursorIndex {0}", oldCursorIndex));
        }
    }

    [Test, TestCaseSource("textWithCodePointBoundaryIndices")]
    public void Delete_RemovesCodePointRightOfCursor(string text, int[] codePointIndices)
    {
        for (var i = 0; i < codePointIndices.Length; i++)
        {
            var codePointIndex = codePointIndices[i];
            m_TextEditor.text = text;
            m_TextEditor.UpdateTextHandle();
            m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = codePointIndex;

            m_TextEditor.Delete();

            var nextCodePointIndex = i < codePointIndices.Length - 1 ? codePointIndices[i + 1] : codePointIndex;
            Assert.AreEqual(codePointIndex, m_TextEditor.stringCursorIndex, "cursorIndex should not change");
            Assert.AreEqual(codePointIndex, m_TextEditor.stringSelectIndex, "selectIndex should not change");
			var expectedText = text == null? "" : text.Remove(codePointIndex, nextCodePointIndex - codePointIndex);
            Assert.AreEqual(expectedText, m_TextEditor.text, string.Format("wrong resulting text for cursorIndex {0}", codePointIndex));
        }
    }

    [Test, TestCaseSource("textWithCodePointBoundaryIndices")]
    public void Backspace_RemovesCodePointLeftOfCursor(string text, int[] codePointIndices)
    {
        for (var i = codePointIndices.Length - 1; i >= 0; i--)
        {
            var codePointIndex = codePointIndices[i];
            m_TextEditor.text = text;
            m_TextEditor.UpdateTextHandle();
            m_TextEditor.m_TextEditing.stringCursorIndex = m_TextEditor.m_TextEditing.stringSelectIndex = codePointIndex;
            var oldCursorIndex = m_TextEditor.m_TextEditing.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.m_TextEditing.stringSelectIndex;

            m_TextEditor.Backspace();
            m_TextEditor.UpdateTextHandle();

            var previousCodePointIndex = i > 0 ? codePointIndices[i - 1] : codePointIndex;
            var codePointLength = codePointIndex - previousCodePointIndex;
            Assert.AreEqual(oldCursorIndex - codePointLength, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to before removed code point", oldCursorIndex));
            Assert.AreEqual(oldSelectIndex - codePointLength, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to before removed code point", oldSelectIndex));
			var expectedText = text == null ? "" : text.Remove(previousCodePointIndex, codePointLength);
            Assert.AreEqual(expectedText, m_TextEditor.text);
        }
    }

    [Test, TestCaseSource("textWithCodePointBoundaryIndices")]
    public void MoveRight_SkipsInvalidCodePointIndices(string text, int[] codePointIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();
        m_TextEditor.cursorIndex = m_TextEditor.selectIndex = 0;

        foreach (var expectedIndex in codePointIndices.Skip(1))
        {
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.MoveRight();

            Assert.AreEqual(expectedIndex, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to next code point index", oldCursorIndex));
            Assert.AreEqual(expectedIndex, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to next code point index", oldSelectIndex));
        }

		var length = GetLength(text);
        Assert.AreEqual(length, m_TextEditor.stringCursorIndex, "cursorIndex did not reach end");
        Assert.AreEqual(length, m_TextEditor.stringSelectIndex, "selectIndex did not reach end");

        m_TextEditor.MoveRight();

        Assert.AreEqual(length, m_TextEditor.stringCursorIndex, "cursorIndex at end should not change");
        Assert.AreEqual(length, m_TextEditor.stringSelectIndex, "selectIndex at end should not change");
    }

    [Test, TestCaseSource("textWithCodePointBoundaryIndices")]
    public void MoveLeft_SkipsInvalidCodePointIndices(string text, int[] codePointIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();
        m_TextEditor.cursorIndex = m_TextEditor.selectIndex = GetLength(text);

        foreach (var expectedIndex in codePointIndices.Reverse().Skip(1))
        {
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.MoveLeft();

            Assert.AreEqual(expectedIndex, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to previous code point index", oldCursorIndex));
            Assert.AreEqual(expectedIndex, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to previous code point index", oldSelectIndex));
        }

        Assert.AreEqual(0, m_TextEditor.stringCursorIndex, "cursorIndex did not reach start");
        Assert.AreEqual(0, m_TextEditor.stringSelectIndex, "selectIndex did not reach start");

        m_TextEditor.MoveLeft();

        Assert.AreEqual(0, m_TextEditor.stringCursorIndex, "cursorIndex at start should not change");
        Assert.AreEqual(0, m_TextEditor.stringSelectIndex, "selectIndex at start should not change");
    }

    [Test, TestCaseSource("textWithLineStartIndices")]
    public void MoveLineStart_MovesCursorAfterPreviousLineFeed(string text, int[] lineStartIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = index;
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.MoveLineStart();

            var lineStart = lineStartIndices.Reverse().FirstOrDefault(i => i <= oldCursorIndex);
            Assert.AreEqual(lineStart, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to line start", oldCursorIndex));
            Assert.AreEqual(lineStart, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to line start", oldSelectIndex));
        }
    }

    [Test, TestCaseSource("textWithLineEndIndices")]
    public void MoveLineEnd_MovesCursorBeforeNextLineFeed(string text, int[] lineEndIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = index;
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.MoveLineEnd();

            var lineEnd = lineEndIndices.First(i => i >= oldCursorIndex);
            Assert.AreEqual(lineEnd, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to line end", oldCursorIndex));
            Assert.AreEqual(lineEnd, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to line end", oldSelectIndex));
        }
    }

    [Test]
    public void MoveTextStart_MovesCursorToStartOfText()
    {
        m_TextEditor.text = "Hello World";
        m_TextEditor.UpdateTextHandle();
        m_TextEditor.cursorIndex = m_TextEditor.selectIndex = 5;

        m_TextEditor.MoveTextStart();

        Assert.AreEqual(0, m_TextEditor.cursorIndex, "cursorIndex did not move to start of text");
        Assert.AreEqual(0, m_TextEditor.selectIndex, "selectIndex did not move to start of text");
    }

    [Test]
    public void MoveTextEnd_MovesCursorToEndOfText()
    {
        m_TextEditor.text = "Hello World";
        m_TextEditor.UpdateTextHandle();
        m_TextEditor.cursorIndex = m_TextEditor.selectIndex = 5;

        m_TextEditor.MoveTextEnd();

        Assert.AreEqual(m_TextEditor.text.Length, m_TextEditor.cursorIndex, "cursorIndex did not move to end of text");
        Assert.AreEqual(m_TextEditor.text.Length, m_TextEditor.selectIndex, "selectIndex did not move to end of text");
    }

    [Test, TestCaseSource("textWithCodePointBoundaryIndices")]
    public void SelectLeft_ExpandSelectionToPreviousCodePoint(string text, int[] codePointIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();
        m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = GetLength(text);

        foreach (var expectedCursorIndex in codePointIndices.Reverse().Skip(1))
        {
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.SelectLeft();

            Assert.AreEqual(expectedCursorIndex, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to previous code point index", oldCursorIndex));
            Assert.AreEqual(oldSelectIndex, m_TextEditor.stringSelectIndex, "selectIndex should not change");
        }

        Assert.AreEqual(0, m_TextEditor.stringCursorIndex, "cursorIndex did not reach start");

        m_TextEditor.SelectLeft();

        Assert.AreEqual(0, m_TextEditor.stringCursorIndex, "cursorIndex at start should not change");
    }

    [Test, TestCaseSource("textWithCodePointBoundaryIndices")]
    public void SelectRight_ExpandSelectionToNextCodePoint(string text, int[] codePointIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();
        m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = 0;

        foreach (var expectedCursorIndex in codePointIndices.Skip(1))
        {
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.SelectRight();

            Assert.AreEqual(expectedCursorIndex, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to next code point index", oldCursorIndex));
            Assert.AreEqual(oldSelectIndex, m_TextEditor.stringSelectIndex, "selectIndex should not change");
        }

        Assert.AreEqual(GetLength(text), m_TextEditor.stringCursorIndex, "cursorIndex did not reach end");

        m_TextEditor.SelectRight();

        Assert.AreEqual(GetLength(text), m_TextEditor.stringCursorIndex, "cursorIndex at end should not change");
    }

    [Test]
    [TestCaseSource("textWithWordStartAndEndIndices")]
    [TestCaseSource("textWithWordStartAndEndIndicesWherePunctuationIsNotAWord")]
    public void MoveWordRight_MovesCursorToNextWordEnd(string text, int[] wordStartIndices, int[] wordEndIndices)
    {
        if (text != null && text.Any(char.IsSurrogate))
            return; // char.IsLetterOrDigit(string, int) does not currently work correctly with surrogates

        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.cursorIndex = m_TextEditor.selectIndex = index;
            var oldCursorIndex = m_TextEditor.cursorIndex;
            var oldSelectIndex = m_TextEditor.selectIndex;

            m_TextEditor.MoveWordRight();

            var nextWordEnd = wordEndIndices.FirstOrDefault(i => i > oldCursorIndex);
            if (nextWordEnd == 0)
                nextWordEnd = GetLength(text);
            Assert.AreEqual(nextWordEnd, m_TextEditor.cursorIndex, string.Format("cursorIndex {0} did not move to next word start", oldCursorIndex));
            Assert.AreEqual(nextWordEnd, m_TextEditor.selectIndex, string.Format("selectIndex {0} did not move to next word start", oldSelectIndex));
        }
    }

    [Test, Ignore("Disabled due to instability UUM-19454")]
    [TestCaseSource("textWithWordStartAndEndIndices")]
    [TestCaseSource("textWithWordStartAndEndIndicesWherePunctuationIsAWord")]
    public void MoveToStartOfNextWord_MovesCursorToNextWordStart(string text, int[] wordStartIndices, int[] wordEndIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = index;
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.MoveToStartOfNextWord();

			var length = GetLength(text);
            var nextWordStart = oldCursorIndex == length ? length : wordStartIndices.Concat(new[] { length }).First(i => i > oldCursorIndex);
            Assert.AreEqual(nextWordStart, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to start of next word", oldCursorIndex));
            Assert.AreEqual(nextWordStart, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to start of next word", oldSelectIndex));
        }
    }

    [Test, Ignore("Disabled due to instability UUM-19454")]
    [TestCaseSource("textWithWordStartAndEndIndices")]
    [TestCaseSource("textWithWordStartAndEndIndicesWherePunctuationIsAWord")]
    public void MoveToEndOfPreviousWord_MovesCursorToPreviousWordStart(string text, int[] wordStartIndices, int[] wordEndIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.stringCursorIndex = m_TextEditor.stringSelectIndex = index;
            var oldCursorIndex = m_TextEditor.stringCursorIndex;
            var oldSelectIndex = m_TextEditor.stringSelectIndex;

            m_TextEditor.MoveToEndOfPreviousWord();

            var previousWordStart = wordStartIndices.Reverse().FirstOrDefault(i => i < oldCursorIndex);
            Assert.AreEqual(previousWordStart, m_TextEditor.stringCursorIndex, string.Format("cursorIndex {0} did not move to previous word start", oldCursorIndex));
            Assert.AreEqual(previousWordStart, m_TextEditor.stringSelectIndex, string.Format("selectIndex {0} did not move to previous word start", oldSelectIndex));
        }
    }

    [Test]
    [TestCaseSource("textWithWordStartAndEndIndices")]
    [TestCaseSource("textWithWordStartAndEndIndicesWherePunctuationIsAWord")]
    public void FindStartOfNextWord_ReturnsIndexOfNextWordStart(string text, int[] wordStartIndices, int[] wordEndIndices)
    {
        if (text != null && text.Any(char.IsSurrogate))
            return; // char.IsLetterOrDigit(string, int) does not currently work correctly with surrogates

        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
			var length = GetLength(text);
            var nextWordStart = index == length ? length : wordStartIndices.Concat(new[] {length}).First(i => i > index);
            Assert.AreEqual(nextWordStart, m_TextEditor.FindStartOfNextWord(index));
        }
    }

    [Test]
    [TestCaseSource("textWithWordStartAndEndIndices")]
    [TestCaseSource("textWithWordStartAndEndIndicesWherePunctuationIsNotAWord")]
    public void MoveWordLeft_MovesCursorToPreviousWordStart(string text, int[] wordStartIndices, int[] wordEndIndices)
    {
        if (text != null && text.Any(char.IsSurrogate))
            return; // char.IsLetterOrDigit(string, int) does not currently work correctly with surrogates

        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.cursorIndex = m_TextEditor.selectIndex = index;
            var oldCursorIndex = m_TextEditor.cursorIndex;
            var oldSelectIndex = m_TextEditor.selectIndex;

            m_TextEditor.MoveWordLeft();

            var previousWordStart = wordStartIndices.Reverse().FirstOrDefault(i => i < oldCursorIndex);
            Assert.AreEqual(previousWordStart, m_TextEditor.cursorIndex, string.Format("cursorIndex {0} did not move to previous word start", oldCursorIndex));
            Assert.AreEqual(previousWordStart, m_TextEditor.selectIndex, string.Format("selectIndex {0} did not move to previous word start", oldSelectIndex));
        }
    }

    [Test, TestCaseSource("textWithExpectedCursorAndSelectIndicesWhenSelectingCurrentWordAtIndex")]
    public void SelectCurrentWord(string text, int[] expectedCursorIndices, int[] expectedSelectIndices)
    {
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();

        for (var index = 0; index <= GetLength(text); index++)
        {
            m_TextEditor.cursorIndex = m_TextEditor.selectIndex = index;
            var oldCursorIndex = m_TextEditor.cursorIndex;

            m_TextEditor.SelectCurrentWord();

            Assert.AreEqual(expectedCursorIndices[index], m_TextEditor.cursorIndex, string.Format("wrong cursorIndex for initial cursorIndex {0}", oldCursorIndex));
            Assert.AreEqual(expectedSelectIndices[index], m_TextEditor.selectIndex, string.Format("wrong selectIndex for initial cursorIndex {0}", oldCursorIndex));
        }
    }

    [Test]
    public void HandleKeyEvent_WithControlAKeyDownEvent_MovesCursorToStartOfLineOnMacOS_SelectsAllElsewhere()
    {
        const string text = "foo";
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();
        m_TextEditor.MoveLineEnd();
        var controlAKeyDownEvent = new Event { type = EventType.KeyDown, keyCode = KeyCode.A, modifiers = EventModifiers.Control };

        m_TextEditor.HandleKeyEvent(controlAKeyDownEvent);

        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
        {
            Assert.That(m_TextEditor.SelectedText, Is.Empty, "Selected text was not empty");
            Assert.That(m_TextEditor.cursorIndex, Is.EqualTo(0), "Cursor did not move to start of line");
        }
        else
            Assert.That(m_TextEditor.SelectedText, Is.EqualTo(text), "Text was not selected");
    }

    [Test]
    public void HandleKeyEvent_WithCommandAKeyDownEvent_SelectsAllOnMacOS()
    {
        if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.MacOSX)
            Assert.Ignore("Test is only applicable on macOS");

        const string text = "foo";
        m_TextEditor.text = text;
        m_TextEditor.UpdateTextHandle();
        var commandAKeyDownEvent = new Event { type = EventType.KeyDown, keyCode = KeyCode.A, modifiers = EventModifiers.Command };

        m_TextEditor.HandleKeyEvent(commandAKeyDownEvent);

        Assert.That(m_TextEditor.SelectedText, Is.EqualTo(text), "Text was not selected");
    }

	int GetLength(string text) => text == null ? 0 : text.Length;
}
