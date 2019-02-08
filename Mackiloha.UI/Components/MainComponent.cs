﻿using System;
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
    public class MainComponent
    {
        private string MiloPath { get; set; }
        private MiloSerializer Serializer { get; set; }

        private MiloObjectDir _milo;
        private MiloObjectDir Milo { get => _milo; set { _milo = value; MiloChanged?.Invoke(value); } }

        private string SelectedType { get; set; }

        private MiloObject _selectedEntry;
        private MiloObject SelectedEntry { get => _selectedEntry; set { _selectedEntry = value; MiloComponent.Milo = value; } }
        
        public event Action<MiloObjectDir> MiloChanged;

        private readonly IFileDialog FileDialog;
        private readonly MiloComponent MiloComponent;

        public MainComponent(IFileDialog fileDialog, MiloComponent miloComponent)
        {
            FileDialog = fileDialog;
            MiloComponent = miloComponent;
        }

        public void LoadMilo(string path)
        {
            SelectedEntry = null;

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
                            case "Font":
                                miloObj = Serializer.ReadFromMiloObjectBytes<Font>(entryBytes);
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

        public void SaveMilo(string path)
        {
            // TODO: Set block compression type
            MiloFile mf = new MiloFile();
            
            using (var ms = new MemoryStream())
            {
                Serializer.WriteToStream(ms, Milo);
                ms.Seek(0, SeekOrigin.Begin);
                mf.Data = ms.ToArray();
            }

            mf.WriteToFile(path);
        }

        public void Render()
        {
            var openMiloModal = false;
            var saveMiloModal = false;

            // Menu bar
            {
                ImGui.BeginMainMenuBar();
                
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open"))
                        openMiloModal = true;

                    ImGui.Separator();
                    //ImGui.MenuItem("Save");
                    if (ImGui.MenuItem("Save As"))
                        saveMiloModal = true;

                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit"))
                        Environment.Exit(0);

                    ImGui.EndMenu();
                }

                ImGui.MenuItem("Options");
                ImGui.MenuItem("Help");

                ImGui.EndMainMenuBar();
            }

            if (openMiloModal && FileDialog.OpenFile())
            {
                openMiloModal = false;
                if (FileDialog.Selection.Length > 0)
                    LoadMilo(FileDialog.Selection[0]);
            }

            if (saveMiloModal && FileDialog.SaveFile())
            {
                saveMiloModal = false;
                if (FileDialog.Selection.Length > 0)
                    SaveMilo(FileDialog.Selection[0]);
            }

            /* if (openMiloModal)
                ImGui.OpenPopup("OpenMiloFile");

            bool open = true;
            ImGui.SetNextWindowSize(new Numerics.Vector2(600, 400), ImGuiCond.FirstUseEver);
            
            if (ImGui.BeginPopupModal("OpenMiloFile", ref open,
                ImGuiWindowFlags.NoTitleBar))
            {
                FileDialog.Render();

                ImGui.EndPopup();
            }*/

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
                MiloComponent.Render();
                ImGui.End();
            }
        }

        private void OpenMilo()
        {
            // TODO: Integrate OpenFileDialog somehow
            //ImGui.OpenPopup("OpenFile");

            //ImGui.OpenPopup("Open");

            ImGui.OpenPopup("OpenMiloFile");

            bool open = false;
            if (ImGui.BeginPopupModal("OpenMiloFile", ref open,
                ImGuiWindowFlags.AlwaysAutoResize))
            {
                var ofd = new OpenFileDialogImGui();
                ofd.Render();

                ImGui.EndPopup();
            }

            
            //ImGui.EndPopup();
        }
    }
}
