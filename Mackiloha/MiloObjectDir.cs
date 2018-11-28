using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mackiloha
{
    public class MiloObjectDir : MiloObject, ISerializable, IEnumerable<MiloObject>
    {
        public List<MiloObject> Entries { get; } = new List<MiloObject>();

        public MiloObject this[int idx] => Entries[idx];
        public MiloObject this[string name] => name != null ? Entries.FirstOrDefault(x => name.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase)) : null;
        
        public T Find<T>(string name) where T : MiloObject => name != null ? Entries.Where(x => x is T).Select(x => x as T).FirstOrDefault(x => name.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase)) : default(T);
        public List<T> Find<T>() where T : MiloObject => Entries.Where(x => x is T).Select(x => x as T).ToList();

        public IEnumerator<MiloObject> GetEnumerator() => Entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();
    }
}
