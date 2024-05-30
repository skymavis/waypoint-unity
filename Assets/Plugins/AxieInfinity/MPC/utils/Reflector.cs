using System.Collections.Generic;
using System.Reflection;

namespace SM.ID.Utils
{
    public static class Reflector
    {
        private static readonly Dictionary<System.Type, FieldInfo[]> cachedFieldInfos = new Dictionary<System.Type, FieldInfo[]>();
        private static readonly List<FieldInfo> _reusableList = new List<FieldInfo>(1024);
        public static FieldInfo[] Reflect(System.Type type)
        {
            //UnityEngine.Assertions.Assert.AreEqual(0, _reusableList.Count, "Reusable list in Reflector was not empty!");
            FieldInfo[] cachedResult;
            if (cachedFieldInfos.TryGetValue(type, out cachedResult))
            {
                return cachedResult;
            }
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            for (var fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                var field = fields[fieldIndex];
                _reusableList.Add(field);
            }
            var resultAsArray = _reusableList.ToArray();
            _reusableList.Clear();
            cachedFieldInfos[type] = resultAsArray;
            return resultAsArray;
        }
    }
}
