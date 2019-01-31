using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mackiloha.Render;
using ImGuiNET;
using System.Reflection;

namespace Mackiloha.UI.Components
{
    public class MiloComponent
    {
        private readonly MiloObject Milo;

        public MiloComponent(MiloObject milo)
        {
            Milo = milo;
        }

        public void Render()
        {
            string name = Milo.Name;
            if (ImGui.InputText("Name", ref name, 0xFF)) Milo.Name = name;
            ImGui.LabelText("Type", Milo.Type);

            // Interfaces
            RenderInterfaces();
        }

        private void RenderInterfaces()
        {
            var interfaces = Milo
                .GetType()
                .GetInterfaces()
                .Where(x => x.Namespace == "Mackiloha.Render" && x.GetProperties().Length > 0)
                .OrderBy(y => y.Name)
                .ToList();

            var milo = Milo as object;
            foreach (var intface in interfaces)
            {
                if (!ImGui.TreeNodeEx(intface.Name.Substring(1),
                    ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Selected))
                    continue;

                RenderObject(ref milo, intface);
                ImGui.TreePop();
            }
        }

        private void RenderObject(ref object obj, Type type)
        {
            var properties = type.GetProperties();

            ImGui.BeginGroup();
            foreach(var prop in properties)
            {
                RenderInput(ref obj, prop);
                ImGui.Separator();
            }
            ImGui.EndGroup();
            
            var fields = type.GetFields();
        }

        private void RenderInput(ref object obj, PropertyInfo info)
        {
            var propName = info.Name;
            var propValue = info.GetValue(obj);
            var propType = propValue.GetType();

            if (propType.IsPrimitive || propType == typeof(string))
            {
                int i = 0;
                float f = 0.0f;
                double d = 0.0d;
                bool b = false;

                switch (propValue)
                {
                    case byte ub:
                        i = ub;

                        if (ImGui.InputInt(propName, ref i)
                            && (i >= byte.MinValue && i <= byte.MaxValue))
                            info.SetValue(obj, i);
                        break;
                    case sbyte sb:
                        i = sb;

                        if (ImGui.InputInt(propName, ref i)
                            && (i >= sbyte.MinValue && i <= sbyte.MaxValue))
                            info.SetValue(obj, i);
                        break;
                    case ushort us:
                        i = us;

                        if (ImGui.InputInt(propName, ref i)
                            && (i >= ushort.MinValue && i <= ushort.MaxValue))
                            info.SetValue(obj, i);
                        break;
                    case short ss:
                        i = ss;

                        if (ImGui.InputInt(propName, ref i)
                            && (i >= short.MinValue && i <= short.MaxValue))
                            info.SetValue(obj, i);
                        break;
                    case uint ui:
                        i = (int)ui;

                        if (ImGui.InputInt(propName, ref i)
                            && i >= uint.MinValue)
                            info.SetValue(obj, i);
                        break;
                    case int si:
                        i = si;

                        if (ImGui.InputInt(propName, ref i)
                            && (i >= int.MinValue && i <= int.MaxValue))
                            info.SetValue(obj, i);
                        break;
                    case ulong ul:
                        i = (int)ul;

                        if (ImGui.InputInt(propName, ref i)
                            && i >= uint.MinValue)
                            info.SetValue(obj, i);
                        break;
                    case long sl:
                        i = (int)sl;

                        if (ImGui.InputInt(propName, ref i)
                            && (i >= int.MinValue && i <= int.MaxValue))
                            info.SetValue(obj, i);
                        break;
                    case float f2:
                        f = f2;
                        
                        if (ImGui.DragFloat(propName, ref f))
                            info.SetValue(obj, f);
                        break;
                    case double d2:
                        d = d2;

                        if (ImGui.InputDouble(propName, ref d))
                            info.SetValue(obj, d);
                        break;
                    case bool b2:
                        b = b2;

                        if (ImGui.Checkbox(propName, ref b))
                            info.SetValue(obj, b);
                        break;
                    case string s2:
                        string s = s2;
                        
                        if (ImGui.InputText(propName, ref s, 0xFF))
                            info.SetValue(obj, s);
                        break;
                }

                return;
            }

            int[] nums = new[] { 0 };
            var num_ints = nums.GetType().GetInterfaces();

            //var ints = propType.GetInterfaces().Contains(x => x == typeof(ICollection));
            var nums2 = nums as ICollection<int>;
            //nums2.Add(40);
            List<int> num3 = new List<int>();
            //num3.IsFixedSize;
            var read = nums2.IsReadOnly;
            if (propType == typeof(Matrix4))
            {
                var mat = (Matrix4)propValue;
                var r1 = new System.Numerics.Vector3(mat.M11, mat.M12, mat.M13);
                var r2 = new System.Numerics.Vector3(mat.M21, mat.M22, mat.M23);
                var r3 = new System.Numerics.Vector3(mat.M31, mat.M32, mat.M33);

                if (!ImGui.TreeNodeEx(propName, ImGuiTreeNodeFlags.DefaultOpen))
                    return;

                //ImGui.Text(propName);
                if (ImGui.InputFloat3($"[0]##{propName}", ref r1))
                {
                    mat.M11 = r1.X;
                    mat.M12 = r1.Y;
                    mat.M13 = r1.Z;
                }
                if (ImGui.InputFloat3($"[1]##{propName}", ref r2))
                {
                    mat.M21 = r2.X;
                    mat.M22 = r2.Y;
                    mat.M23 = r2.Z;
                }
                if (ImGui.InputFloat3($"[2]##{propName}", ref r3))
                {
                    mat.M31 = r3.X;
                    mat.M32 = r3.Y;
                    mat.M33 = r3.Z;
                }

                info.SetValue(obj, mat);
                ImGui.TreePop();
                return;
            }
        }
    }
}
