using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    public class CustomTokenStream : ITokenStream
    {
        protected internal IList<IToken> tokens;
        protected internal int p = -1;
        protected internal bool fetchedEOF;

        public CustomTokenStream(IList<IToken> tokens)
        {
            this.tokens = tokens;
        }

        public virtual IList<IToken> GetTokens()
        {
            return tokens;
        }

        public virtual int Index
        {
            get
            {
                return p;
            }
        }

        public virtual string SourceName
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual ITokenSource TokenSource
        {
            get
            {
                return null;
            }
        }

        public virtual int Mark()
        {
            return 0;
        }

        public virtual void Release(int marker)
        {
        }

        public virtual void Reset()
        {
            Seek(0);
        }

        public virtual void Seek(int index)
        {
            LazyInit();
            p = AdjustSeekIndex(index);
        }

        public virtual int Size
        {
            get
            {
                return tokens.Count;
            }
        }

        public virtual void Consume()
        {
            bool skipEofCheck;
            if (p >= 0)
            {
                if (fetchedEOF)
                {
                    skipEofCheck = p < tokens.Count - 1;
                }
                else
                {
                    skipEofCheck = p < tokens.Count;
                }
            }
            else
            {
                skipEofCheck = false;
            }
            if (!skipEofCheck && La(1) == IntStreamConstants.Eof)
            {
                throw new InvalidOperationException("cannot consume EOF");
            }
            if (Sync(p + 1))
            {
                p = AdjustSeekIndex(p + 1);
            }
        }

        protected internal virtual bool Sync(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            Get(i);
            return true;
        }

        protected internal virtual int Fetch(int n)
        {
            if (fetchedEOF)
            {
                return 0;
            }
            Get(n);
            return n;
        }

        public virtual IToken Get(int i)
        {
            if (i < 0 || i >= tokens.Count)
            {
                throw new ArgumentOutOfRangeException("token index " + i + " out of range 0.." + (tokens.Count - 1));
            }
            return tokens[i];
        }

        public virtual int La(int i)
        {
            return Lt(i).Type;
        }

        protected internal virtual IToken Lb(int k)
        {
            if ((p - k) < 0)
            {
                return null;
            }
            return tokens[p - k];
        }

        [return: NotNull]
        public virtual IToken Lt(int k)
        {
            LazyInit();
            if (k == 0)
            {
                return null;
            }
            if (k < 0)
            {
                return Lb(-k);
            }
            int i = p + k - 1;
            Sync(i);
            if (i >= tokens.Count)
            {
                // return EOF token
                // EOF must be last token
                return tokens[tokens.Count - 1];
            }
            //		if ( i>range ) range = i;
            return tokens[i];
        }

     
        protected internal virtual int AdjustSeekIndex(int i)
        {
            return i;
        }

        protected internal void LazyInit()
        {
            if (p == -1)
            {
                Setup();
            }
        }

        protected internal virtual void Setup()
        {
            Sync(0);
            p = AdjustSeekIndex(0);
        }

        protected internal virtual int NextTokenOnChannel(int i, int channel)
        {
            Sync(i);
            if (i >= Size)
            {
                return Size - 1;
            }
            IToken token = tokens[i];
            while (token.Channel != channel)
            {
                if (token.Type == TokenConstants.Eof)
                {
                    return i;
                }
                i++;
                Sync(i);
                token = tokens[i];
            }
            return i;
        }

        protected internal virtual int PreviousTokenOnChannel(int i, int channel)
        {
            Sync(i);
            if (i >= Size)
            {
                // the EOF token is on every channel
                return Size - 1;
            }
            while (i >= 0)
            {
                IToken token = tokens[i];
                if (token.Type == TokenConstants.Eof || token.Channel == channel)
                {
                    return i;
                }
                i--;
            }
            return i;
        }

        /// <summary>Get the text of all tokens in this buffer.</summary>
        [return: NotNull]
        public virtual string GetText()
        {
            return GetText(Interval.Of(0, Size - 1));
        }

        [return: NotNull]
        public virtual string GetText(Interval interval)
        {
            int start = interval.a;
            int stop = interval.b;
            if (start < 0 || stop < 0)
            {
                return string.Empty;
            }
            Fill();
            if (stop >= tokens.Count)
            {
                stop = tokens.Count - 1;
            }
            StringBuilder buf = new StringBuilder();
            for (int i = start; i <= stop; i++)
            {
                IToken t = tokens[i];
                if (t.Type == TokenConstants.Eof)
                {
                    break;
                }
                buf.Append(t.Text);
            }
            return buf.ToString();
        }

        [return: NotNull]
        public virtual string GetText(RuleContext ctx)
        {
            return GetText(ctx.SourceInterval);
        }

        [return: NotNull]
        public virtual string GetText(IToken start, IToken stop)
        {
            if (start != null && stop != null)
            {
                return GetText(Interval.Of(start.TokenIndex, stop.TokenIndex));
            }
            return string.Empty;
        }

        public virtual void Fill()
        {
        }
    }
}
