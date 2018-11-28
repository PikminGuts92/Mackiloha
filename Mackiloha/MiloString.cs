using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha
{
    public struct MiloString
    {
        private readonly string _value;
        
        private MiloString(string s) => _value = s ?? "";

        #region Overloaded Operators
        public static implicit operator string(MiloString ms) => ms.Value;
        public static implicit operator MiloString(string s) => new MiloString(s);
        
        public static bool operator ==(MiloString a, MiloString b) => a.Equals(b);
        public static bool operator !=(MiloString a, MiloString b) => !(a == b);

        public override bool Equals(object obj) => (obj is MiloString) && ((MiloString)obj).Value == Value;
        public override int GetHashCode() => Value.GetHashCode();
        #endregion
        
        private string Value => _value ?? "";
        public override string ToString() => Value;
    }
}
