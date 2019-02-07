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
        public MiloObject Milo;

        public MiloComponent()
        {

        }

        public void Render()
        {
            if (Milo == null)
                return;

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
                var val = prop.GetValue(obj);
                RenderInput(ref val, prop.Name);
                
                if (prop.CanWrite) prop.SetValue(obj, val);
                ImGui.Separator();
            }
            ImGui.EndGroup();
            
            var fields = type.GetFields();
            ImGui.BeginGroup();
            foreach (var f in fields.Where(x => x.IsPublic))
            {
                var val = f.GetValue(obj);
                RenderInput(ref val, f.Name);
                
                f.SetValue(obj, val);
                ImGui.Separator();
            }
            ImGui.EndGroup();
        }
        
        private void RenderInput(ref object obj, string name)
        {
            var objType = obj.GetType();

            if (objType.IsPrimitive || objType == typeof(string))
            {
                int i = 0;
                float f = 0.0f;
                double d = 0.0d;
                bool b = false;

                switch (obj)
                {
                    case byte ub:
                        i = ub;

                        if (ImGui.InputInt(name, ref i)
                            && (i >= byte.MinValue && i <= byte.MaxValue))
                            obj = i;
                        break;
                    case sbyte sb:
                        i = sb;

                        if (ImGui.InputInt(name, ref i)
                            && (i >= sbyte.MinValue && i <= sbyte.MaxValue))
                            obj = i;
                        break;
                    case ushort us:
                        i = us;

                        if (ImGui.InputInt(name, ref i)
                            && (i >= ushort.MinValue && i <= ushort.MaxValue))
                            obj = i;
                        break;
                    case short ss:
                        i = ss;

                        if (ImGui.InputInt(name, ref i)
                            && (i >= short.MinValue && i <= short.MaxValue))
                            obj = i;
                        break;
                    case uint ui:
                        i = (int)ui;

                        if (ImGui.InputInt(name, ref i)
                            && i >= uint.MinValue)
                            obj = i;
                        break;
                    case int si:
                        i = si;

                        if (ImGui.InputInt(name, ref i)
                            && (i >= int.MinValue && i <= int.MaxValue))
                            obj = i;
                        break;
                    case ulong ul:
                        i = (int)ul;

                        if (ImGui.InputInt(name, ref i)
                            && i >= uint.MinValue)
                            obj = i;
                        break;
                    case long sl:
                        i = (int)sl;

                        if (ImGui.InputInt(name, ref i)
                            && (i >= int.MinValue && i <= int.MaxValue))
                            obj = i;
                        break;
                    case float f2:
                        f = f2;

                        if (ImGui.DragFloat(name, ref f))
                            obj = f;
                        break;
                    case double d2:
                        d = d2;

                        if (ImGui.InputDouble(name, ref d))
                            obj = d;
                        break;
                    case bool b2:
                        b = b2;

                        if (ImGui.Checkbox(name, ref b))
                            obj = b;
                        break;
                    case string s2:
                        string s = s2;

                        if (ImGui.InputText(name, ref s, 0xFF))
                            obj = s;
                        break;
                }

                return;
            }

            if (obj is ICollection collection)
            {
                if (!ImGui.TreeNodeEx(name))
                    return;

                var idx = 0;
                foreach (var item in collection)
                {
                    var temp = item;

                    //RenderObject(ref temp, item.GetType()); // What if a struct?
                    RenderInput(ref temp, $"[{idx++}]");
                }

                ImGui.TreePop();
                return;
            }

            if (objType == typeof(Matrix4))
            {
                var mat = (Matrix4)obj;
                var r1 = new System.Numerics.Vector3(mat.M11, mat.M12, mat.M13);
                var r2 = new System.Numerics.Vector3(mat.M21, mat.M22, mat.M23);
                var r3 = new System.Numerics.Vector3(mat.M31, mat.M32, mat.M33);

                if (!ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.DefaultOpen))
                    return;

                //ImGui.Text(propName);
                if (ImGui.InputFloat3($"[0]##{name}", ref r1))
                {
                    mat.M11 = r1.X;
                    mat.M12 = r1.Y;
                    mat.M13 = r1.Z;
                }
                if (ImGui.InputFloat3($"[1]##{name}", ref r2))
                {
                    mat.M21 = r2.X;
                    mat.M22 = r2.Y;
                    mat.M23 = r2.Z;
                }
                if (ImGui.InputFloat3($"[2]##{name}", ref r3))
                {
                    mat.M31 = r3.X;
                    mat.M32 = r3.Y;
                    mat.M33 = r3.Z;
                }
                
                obj = mat;
                ImGui.TreePop();
                return;
            }

            if (objType.IsEnum)
            {
                var selectedName = objType.GetEnumName(obj);
                var selectedInt = Convert.ChangeType(obj, objType.GetEnumUnderlyingType());
                var enumValues = objType.GetEnumValues();
                
                if (ImGui.BeginCombo(name, $"{selectedName} ({selectedInt})"))
                {
                    foreach (var v in enumValues)
                    {
                        var enumName = objType.GetEnumName(v);
                        var enumInt = Convert.ChangeType(v, objType.GetEnumUnderlyingType());

                        if (ImGui.Selectable($"{enumName} ({enumInt})", obj.Equals(v)))
                            obj = v;
                    }
                    ImGui.EndCombo();
                }
                return;
            }

            // Assume class/struct
            if (!ImGui.TreeNodeEx(name))
                return;

            //RenderObject(ref propValue, propType);
            RenderObject(ref obj, objType);
            ImGui.TreePop();
        }
    }
}
