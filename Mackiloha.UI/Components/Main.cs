using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Numerics = System.Numerics;
using System.Reflection;
using System.Text;
using ImGuiNET;
using Mackiloha;
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.Render;
using Mackiloha.UI.Extensions;

namespace Mackiloha.UI.Components
{
    public class Main
    {
        private string MiloPath { get; set; }
        private MiloSerializer Serializer { get; set; }

        private MiloObjectDir _milo;
        private MiloObjectDir Milo { get => _milo; set { _milo = value; MiloChanged?.Invoke(value); } }

        private string SelectedType { get; set; }
        private MiloObject SelectedEntry { get; set; }
        
        public event Action<MiloObjectDir> MiloChanged;

        public Main() { }

        public void LoadMilo(string path)
        {
            if (path == null)
            {
                Milo = null;
                return;
            }

            var miloFile = MiloFile.ReadFromFile(path);

            Serializer = new MiloSerializer(new SystemInfo()
            {
                Version = miloFile.Version,
                BigEndian = miloFile.BigEndian
            });

            using (var ms = new MemoryStream(miloFile.Data))
            {
                var milo = Serializer.ReadFromStream<MiloObjectDir>(ms);
                if (milo == null)
                {
                    Milo = null;
                    return;
                };

                List<MiloObject> miloObjects = new List<MiloObject>();

                foreach (var entry in milo.Entries)
                {
                    var entryBytes = entry as MiloObjectBytes;
                    if (entryBytes == null)
                        continue;

                    try
                    {
                        MiloObject miloObj = null;

                        switch (entry.Type)
                        {
                            case "Cam":
                                miloObj = Serializer.ReadFromMiloObjectBytes<Cam>(entryBytes);
                                break;
                            case "Environ":
                                miloObj = Serializer.ReadFromMiloObjectBytes<Environ>(entryBytes);
                                break;
                            case "Mat":
                                miloObj = Serializer.ReadFromMiloObjectBytes<Mat>(entryBytes);
                                break;
                            case "Mesh":
                                miloObj = Serializer.ReadFromMiloObjectBytes<Mesh>(entryBytes);
                                break;
                            case "Tex":
                                miloObj = Serializer.ReadFromMiloObjectBytes<Tex>(entryBytes);
                                break;
                            case "View":
                                miloObj = Serializer.ReadFromMiloObjectBytes<View>(entryBytes);
                                break;
                            default:
                                continue;
                        }

                        if (miloObj == null) continue; // Shouldn't be...
                        miloObjects.Add(miloObj);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                foreach (var miloObj in miloObjects)
                {
                    var remObj = milo.Entries.First(x => x.Type == miloObj.Type && x.Name == miloObj.Name);
                    milo.Entries.Remove(remObj);

                    milo.Entries.Add(miloObj);
                }

                milo.SortEntriesByType();
                Milo = milo;
            }
        }

        public void Render()
        {
            // Menu bar
            {
                ImGui.BeginMainMenuBar();

                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open"))
                        OpenMilo();

                    ImGui.Separator();
                    ImGui.MenuItem("Save");
                    ImGui.MenuItem("Save As");
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit"))
                        Environment.Exit(0);

                    ImGui.EndMenu();
                }

                ImGui.MenuItem("Options");
                ImGui.MenuItem("Help");

                ImGui.EndMainMenuBar();
            }

            // Archive explorer
            if (Milo != null)
            {
                ImGui.Begin("Archive");
                ImGui.Columns(2, "Split", true);
                
                if (ImGui.TreeNodeEx("RndDir", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    foreach (var type in Milo.Entries.Select(x => x.Type).Distinct().OrderBy(y => y))
                    {
                        if (ImGui.Selectable(type))
                        {
                            if (SelectedType != type)
                                SelectedEntry = null; // Reset

                            SelectedType = type;
                        }

                        /*
                        if (ImGui.TreeNodeEx(type))
                        {
                            foreach (var entry in Milo.FilterByType(type).OrderBy(x => x.Name))
                            {
                                ImGui.TreeNodeEx(entry.Name, ImGuiTreeNodeFlags.Leaf);
                                ImGui.TreePop();
                            }

                            ImGui.TreePop();
                        }*/
                    }
                }

                ImGui.NextColumn();
                ImGui.BeginChild("Files");
                if (SelectedType != null)
                {
                    //ImGui.ImageButton(IntPtr.Zero, new Numerics.Vector2(25, 25));
                    foreach (var entry in Milo.FilterByType(SelectedType).OrderBy(x => x.Name))
                    {
                        //ImGui.BeginGroup();

                        if (ImGui.Selectable(entry.Name, SelectedEntry == entry))
                            SelectedEntry = entry;

                        //ImGui.ImageButton(IntPtr.Zero, new Numerics.Vector2(25, 25));

                        //ImGui.EndGroup();
                    }                    
                }
                ImGui.EndChild();

                ImGui.End();
            }

            if (SelectedEntry != null)
            {
                ImGui.Begin("Object 2");
                new MiloComponent(SelectedEntry).Render();
                ImGui.End();

                ImGui.Begin("Object");

                string name = SelectedEntry.Name;
                ImGui.PushItemWidth(-1.0f);
                if (ImGui.InputText("Name", ref name, 255))
                    SelectedEntry.Name = name;
                ImGui.PopItemWidth();
                
                //ImGui.LabelText("Name", SelectedEntry.Name);
                ImGui.LabelText("Type", SelectedEntry.Type);

                // Reflection stuff
                var interfaces = SelectedEntry
                    .GetType()
                    .GetInterfaces()
                    .Where(x => x.Namespace == "Mackiloha.Render" && x.GetProperties().Length > 0)
                    .OrderBy(y => y.Name)
                    .ToList();

                void TextInputFloat(string label, float initValue, Action<float> callback)
                {
                    string buffer = initValue.ToString();

                    ImGui.PushItemWidth(-1.0f); // Hides label
                    if (ImGui.InputText(label, ref buffer, 255))
                    {
                        if (float.TryParse(buffer, out float value))
                            callback(value);
                        else
                            callback(initValue);
                    }

                    ImGui.PopItemWidth();
                };

                void TextInputPrimitive(string label, ref object primitiveObj)
                {
                    string buffer = primitiveObj.ToString();
                    var initValue = primitiveObj;

                    ImGui.PushItemWidth(-1.0f); // Hides label
                    if (ImGui.InputText("##" + label, ref buffer, 255))
                    {
                        if (primitiveObj.GetType() == typeof(string))
                        {
                            primitiveObj = buffer;
                        }
                        else
                        {
                            try
                            {
                                var parseMethod = primitiveObj.GetType()
                                    .GetMethod("Parse", new[] { typeof(string) });

                                var result = parseMethod.Invoke(null, new[] { buffer });
                                primitiveObj = result;
                            }
                            catch
                            {
                                primitiveObj = initValue;
                            }
                        }
                    }

                    ImGui.PopItemWidth();
                };

                void InputBoolean(string label, ref object boolObj)
                {
                    var boolValue = (bool)boolObj;

                    ImGui.PushItemWidth(-1.0f); // Hides label
                    if (ImGui.Checkbox("##" + label, ref boolValue))
                    {
                        boolObj = boolValue;
                    }

                    ImGui.PopItemWidth();
                };

                string GetStackPath(Stack<string> stack)
                {
                    return string.Join(".", stack.Reverse().ToArray());
                }

                void RenderObject(string objName, ref object obj, Stack<string> stack)
                {
                    var objType = obj.GetType();
                    ImGui.Columns(1);

                    stack.Push(objName);
                    if (objType.IsPrimitive || objType == typeof(string))
                    {
                        ImGui.BeginGroup();
                        ImGui.Columns(2, "_" + GetStackPath(stack), false);

                        ImGui.Text(objName);
                        ImGui.NextColumn();

                        if (objType == typeof(bool))
                        {
                            InputBoolean(objName, ref obj);
                        }
                        else
                        {
                            TextInputPrimitive(objName, ref obj);
                        }
                        ImGui.NextColumn();
                        ImGui.EndGroup();
                    }
                    else if (objType.IsValueType) // struct
                    {
                        if (ImGui.TreeNodeEx(objName))
                        {
                            foreach (var field in objType.GetFields())
                            {
                                stack.Push(field.Name);

                                var fieldName = field.Name;
                                var fieldValue = field.GetValue(obj);
                                var fieldType = field.GetType();



                                stack.Pop();
                            }

                            ImGui.TreePop();
                        }
                    }

                    stack.Pop();
                }

                void RenderProperties(object obj, Type type, Stack<string> stack)
                {
                    var props = type.GetProperties();

                    ImGui.BeginGroup();
                    foreach (var prop in type.GetProperties())
                    {
                        stack.Push(prop.Name);

                        var propName = prop.Name;
                        var propValue = prop.GetValue(obj);
                        var propType = propValue.GetType();
                        
                        // Check if generic collection
                        if (propType.IsGenericType && propValue is IList)
                        {
                            var list = propValue as IList;
                            var listType = propType.GetGenericArguments().First();

                            ImGui.Separator();
                            stack.Pop();
                            continue;

                            var type2 = propType.GetGenericTypeDefinition(); //propType.GetGenericTypeDefinition() == typeof(ICollection<>)

                            var collect = type2.GetInterface(nameof(IList));
                        }

                        RenderObject(propName, ref propValue, stack);
                        prop.SetValue(obj, propValue);

                        //List<string> test;
                        //switch ()
                        /*
                        switch (propValue)
                        {
                            case Matrix4 mat:
                                ImGui.Columns(4, "_" + GetStackPath(stack), false);

                                ImGui.BeginGroup();

                                // Row 1
                                ImGui.Text(propName);
                                ImGui.NextColumn();
                                
                                TextInputFloat("##R1_X" + GetStackPath(stack), mat.M11, f => mat.M11 = f);
                                ImGui.NextColumn();

                                TextInputFloat("##R1_Y" + GetStackPath(stack), mat.M12, f => mat.M12 = f);
                                ImGui.NextColumn();

                                TextInputFloat("##R1_Z" + GetStackPath(stack), mat.M13, f => mat.M13 = f);
                                ImGui.NextColumn();

                                // Row 2
                                ImGui.Text("");
                                ImGui.NextColumn();

                                TextInputFloat("##R2_X" + GetStackPath(stack), mat.M21, f => mat.M21 = f);
                                ImGui.NextColumn();

                                TextInputFloat("##R2_Y" + GetStackPath(stack), mat.M22, f => mat.M22 = f);
                                ImGui.NextColumn();

                                TextInputFloat("##R2_Z" + GetStackPath(stack), mat.M23, f => mat.M23 = f);
                                ImGui.NextColumn();

                                // Row 3
                                ImGui.Text("");
                                ImGui.NextColumn();

                                TextInputFloat("##R3_X" + GetStackPath(stack), mat.M31, f => mat.M31 = f);
                                ImGui.NextColumn();

                                TextInputFloat("##R3_Y" + GetStackPath(stack), mat.M32, f => mat.M32 = f);
                                ImGui.NextColumn();

                                TextInputFloat("##R3_Z" + GetStackPath(stack), mat.M33, f => mat.M33 = f);
                                ImGui.NextColumn();
                                
                                ImGui.EndGroup();

                                prop.SetValue(obj, mat);
                                break;
                            default:
                                ImGui.Text(propName);
                                break;
                        }*/
                        ImGui.Separator();
                        stack.Pop();
                    }
                    ImGui.EndGroup();
                }

                var objStack = new Stack<string>();
                objStack.Push("SelectedEntry");

                foreach (var intface in interfaces)
                {
                    ImGui.Columns(1);
                    if (!ImGui.TreeNodeEx(intface.Name.Substring(1), ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Selected))
                        continue;

                    // Render properties
                    RenderProperties(SelectedEntry, intface, objStack);
                    ImGui.TreePop();
                    continue;

                    switch (intface.Name)
                    {
                        case nameof(ITrans):
                            var trans = SelectedEntry as ITrans;

                            if (ImGui.TreeNodeEx(nameof(ITrans).Substring(1), ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                ImGui.Columns(4, "_", false);

                                ImGui.BeginGroup();
                                ImGui.Text("Position");
                                //ImGui.NextColumn();
                                ImGui.NextColumn();

                                TextInputFloat("##P_X", trans.Mat1.M11, f =>
                                {
                                    var mat = trans.Mat1;
                                    mat.M11 = f;
                                    trans.Mat1 = mat;
                                });
                                ImGui.NextColumn();

                                TextInputFloat("##P_Y", trans.Mat1.M12, f =>
                                {
                                    var mat = trans.Mat1;
                                    mat.M12 = f;
                                    trans.Mat1 = mat;
                                });
                                ImGui.NextColumn();

                                TextInputFloat("##P_Z", trans.Mat1.M13, f =>
                                {
                                    var mat = trans.Mat1;
                                    mat.M13 = f;
                                    trans.Mat1 = mat;
                                });
                                ImGui.NextColumn();
                                
                                ImGui.EndGroup();

                                /*
                                ImGui.BeginGroup();
                                ImGui.Text("Rotation");
                                ImGui.EndGroup();

                                ImGui.BeginGroup();
                                ImGui.Text("Scale");
                                ImGui.EndGroup();*/
                            }
                            break;
                    }
                }

                ImGui.End();
            }
        }

        private void OpenMilo()
        {
            // TODO: Integrate OpenFileDialog somehow
            //ImGui.OpenPopup("OpenFile");

        }
    }
}
