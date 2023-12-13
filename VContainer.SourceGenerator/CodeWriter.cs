using System;
using System.Text;

namespace VContainer.SourceGenerator
{
    public class CodeWriter
    {
        readonly struct IndentScope : IDisposable
        {
            readonly CodeWriter source;

            public IndentScope(CodeWriter source)
            {
                this.source = source;
                source.IncreasaeIndent();
            }

            public void Dispose()
            {
                source.DecreaseIndent();
            }
        }

        readonly struct BlockScope : IDisposable
        {
            readonly CodeWriter source;

            public BlockScope(CodeWriter source, string? startLine = null)
            {
                this.source = source;
                source.AppendLine(startLine);
                source.BeginBlock();
            }

            public void Dispose()
            {
                source.EndBlock();
            }
        }

        readonly StringBuilder buffer = new();
        int indentLevel;

        public void AppendLine(string value = "")
        {
            if (string.IsNullOrEmpty(value))
            {
                buffer.AppendLine();
            }
            else
            {
                buffer.AppendLine($"{new string(' ', indentLevel * 4)} {value}");
            }
        }

        public override string ToString() => buffer.ToString();

        public IDisposable BeginIndentScope() => new IndentScope(this);
        public IDisposable BeginBlockScope(string? startLine = null) => new BlockScope(this, startLine);

        public void IncreasaeIndent()
        {
            indentLevel++;
        }

        public void DecreaseIndent()
        {
            if (indentLevel > 0)
                indentLevel--;
        }

        public void BeginBlock()
        {
            AppendLine("{");
            IncreasaeIndent();
        }

        public void EndBlock()
        {
            DecreaseIndent();
            AppendLine("}");
        }

        public void Clear()
        {
            buffer.Clear();
            indentLevel = 0;
        }
    }
}