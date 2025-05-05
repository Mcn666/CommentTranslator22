using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.VSIntelliSenseTweaks.Utilities
{
    public struct CharKind
    {
        private const byte isLetter = 1;
        private const byte isUpper = 2;

        private byte flags;

        public CharKind(char c)
        {
            this.flags = default;
            flags |= char.IsLetter(c) ? isLetter : default;
            flags |= char.IsUpper(c) ? isUpper : default;
        }

        public bool IsLetter => (flags & isLetter) != 0;
        public bool IsUpper => (flags & isUpper) != 0;
    }
}
