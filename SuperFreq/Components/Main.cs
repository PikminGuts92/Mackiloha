using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Numerics = System.Numerics;
using System.Text;
using ImGuiNET;
using Mackiloha;
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.Render;
using SuperFreq.Extensions;

namespace SuperFreq.Components
{
    public class Main
    {
        private string MiloPath { get; set; }
        private MiloSerializer Serializer { get; set; }
        private MiloObjectDir Milo { get; set; }

        private string SelectedType { get; set; }
        private MiloObject SelectedEntry { get; set; }

        public Main()
        {
            var miloPath = Environment.GetCommandLineArgs()
                .Skip(1)
                .FirstOrDefault();

            if (miloPath == null)
                return;

            var miloFile = MiloFile.ReadFromFile(miloPath);

            Serializer = new MiloSerializer(new SystemInfo()
            {
                Version = miloFile.Version,
                BigEndian = miloFile.BigEndian
            });

            using (var ms = new MemoryStream(miloFile.Data))
            {
                Milo = Serializer.ReadFromStream<MiloObjectDir>(ms);
                if (Milo == null) return;
                
                List<MiloObject> miloObjects = new List<MiloObject>();

                foreach (var entry in Milo.Entries)
                {
                    var entryBytes = entry as MiloObjectBytes;
                    if (entryBytes == null)
                        continue;

                    try
                    {
                        MiloObject miloObj = null;

                        switch (entry.Type)
                        {
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
                    var remObj = Milo.Entries.First(x => x.Type == miloObj.Type && x.Name == miloObj.Name);
                    Milo.Entries.Remove(remObj);

                    Milo.Entries.Add(miloObj);
                }
                
                Milo.SortEntriesByType();
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
                ImGui.Begin("Object");

                /* string name = SelectedEntry.Name;
                if (ImGui.InputText("Name", ref name, 255))
                    SelectedEntry.Name = name; */

                ImGui.LabelText("Name", SelectedEntry.Name);
                ImGui.LabelText("Type", SelectedEntry.Type);


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
